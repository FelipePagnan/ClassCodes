using Caixinhas.Domain.Common;
using Caixinhas.Domain.Enums;
using Caixinhas.Domain.Events;
using Caixinhas.Domain.Exceptions;
using Caixinhas.Domain.ValueObjects;

namespace Caixinhas.Domain.Entities;

/// <summary>
/// Raiz de agregação da "caixinha". Controla as transações e mantém o
/// ValorTotal e o Status sempre consistentes.
/// </summary>
public class ObjetivoFinanceiro : BaseEntity
{
    /// <summary>Tamanho máximo aceito para a imagem de capa.</summary>
    public const int TamanhoMaximoCapaBytes = 5 * 1024 * 1024;

    private readonly List<Transacao> _transacoes = new();

    // Construtor sem parâmetros para o EF Core.
    private ObjetivoFinanceiro() { }

    public ObjetivoFinanceiro(
        string titulo,
        decimal quantidadeAlvo,
        DateTime? prazo = null,
        decimal? quantidadeIdealAporteMensal = null)
    {
        DefinirTitulo(titulo);
        DefinirQuantidadeAlvo(quantidadeAlvo);
        DefinirPrazo(prazo);
        DefinirAporteMensalIdeal(quantidadeIdealAporteMensal);

        Status = StatusObjetivo.EmProgresso;
        ValorTotal = 0m;
        DataCriacao = DateTime.Now;

        AdicionarEvento(new ObjetivoCriadoEvent(Id, Titulo, QuantidadeAlvo, Prazo));
    }

    public string Titulo { get; private set; } = string.Empty;
    public decimal QuantidadeAlvo { get; private set; }
    public DateTime? Prazo { get; private set; }
    public decimal? QuantidadeIdealAporteMensal { get; private set; }
    public StatusObjetivo Status { get; private set; } = StatusObjetivo.EmProgresso;

    /// <summary>
    /// Saldo da caixinha, mantido incrementalmente a cada transação
    /// (requisito PLUS) e persistido em coluna própria. Nunca é recalculado
    /// somando as transações na hora da consulta.
    /// </summary>
    public decimal ValorTotal { get; private set; }

    /// <summary>Imagem de capa do objetivo armazenada como bytes.</summary>
    public byte[]? CapaObjetivo { get; private set; }

    public IReadOnlyCollection<Transacao> Transacoes => _transacoes.AsReadOnly();

    /// <summary>Transações ativas, ignorando as removidas por soft delete.</summary>
    public IEnumerable<Transacao> TransacoesAtivas => _transacoes.Where(t => !t.EstaDeletado);

    public Dinheiro Saldo => Dinheiro.Criar(ValorTotal);
    public Dinheiro Meta => Dinheiro.Criar(QuantidadeAlvo);

    public bool TemCapa => CapaObjetivo is { Length: > 0 };

    public int TotalTransacoes => TransacoesAtivas.Count();

    public decimal ValorRestante => Math.Max(0, QuantidadeAlvo - ValorTotal);

    public decimal PercentualConcluido => QuantidadeAlvo == 0
        ? 0
        : Math.Min(100, Math.Round(ValorTotal / QuantidadeAlvo * 100, 2));

    public bool MetaAlcancada => ValorTotal >= QuantidadeAlvo;

    public int? DiasRestantes => Prazo is null
        ? null
        : (int)(Prazo.Value.Date - DateTime.Today).TotalDays;

    public bool PrazoVencido => Prazo is not null
        && Prazo.Value.Date < DateTime.Today
        && !MetaAlcancada;

    /// <summary>Quanto falta guardar por mês para bater a meta até o prazo.</summary>
    public decimal? AporteMensalNecessario
    {
        get
        {
            if (Prazo is null || MetaAlcancada) return null;

            var meses = MesesAte(Prazo.Value);
            return meses <= 0 ? ValorRestante : Math.Round(ValorRestante / meses, 2);
        }
    }

    /// <summary>
    /// Aceita transações apenas quando está em progresso. Caixinhas completas,
    /// pausadas ou canceladas ficam congeladas.
    /// </summary>
    public bool AceitaTransacoes => Status is StatusObjetivo.EmProgresso;

    // ----------------------------- Comportamento -----------------------------

    public void Atualizar(
        string titulo,
        decimal quantidadeAlvo,
        DateTime? prazo,
        decimal? quantidadeIdealAporteMensal)
    {
        if (EstaDeletado)
            throw new RegraNegocioException($"O objetivo '{Titulo}' foi removido.");

        DefinirTitulo(titulo);

        // A meta não pode ficar abaixo do que já foi guardado.
        if (quantidadeAlvo < ValorTotal)
            throw new RegraNegocioException(
                $"A nova meta (R$ {quantidadeAlvo:N2}) é menor que o saldo atual " +
                $"(R$ {ValorTotal:N2}).");

        DefinirQuantidadeAlvo(quantidadeAlvo);
        DefinirPrazo(prazo);
        DefinirAporteMensalIdeal(quantidadeIdealAporteMensal);

        // Baixar a meta pode completar a caixinha; subir pode reabri-la.
        ReavaliarStatus();
    }

    /// <summary>
    /// Registra uma transação, atualiza o ValorTotal e reavalia o status.
    /// Dispara TransacaoRegistradaEvent e, quando aplicável, MetaAlcancadaEvent.
    /// </summary>
    public Transacao RegistrarTransacao(
        decimal quantidade,
        TipoTransacao tipo,
        DateTime dataTransacao)
    {
        if (EstaDeletado)
            throw new RegraNegocioException($"O objetivo '{Titulo}' foi removido.");

        if (!AceitaTransacoes)
            throw new RegraNegocioException(
                $"O objetivo '{Titulo}' está {Status} e não aceita novas transações.");

        if (dataTransacao.Date < DataCriacao.Date)
            throw new RegraNegocioException(
                $"A data da transação ({dataTransacao:dd/MM/yyyy}) não pode ser anterior " +
                $"à criação do objetivo ({DataCriacao:dd/MM/yyyy}).");

        // O construtor da transação valida quantidade, tipo e data.
        var transacao = new Transacao(Id, quantidade, tipo, dataTransacao);

        // Um saque não pode deixar a caixinha com saldo negativo.
        if (tipo == TipoTransacao.Saque && transacao.Quantidade > ValorTotal)
            throw new RegraNegocioException(
                $"Saldo insuficiente em '{Titulo}'. Disponível: R$ {ValorTotal:N2}, " +
                $"solicitado: R$ {transacao.Quantidade:N2}.");

        var saldoAnterior = ValorTotal;
        var statusAnterior = Status;

        _transacoes.Add(transacao);

        // O ValorTotal é atualizado aqui, no cadastro da transação.
        ValorTotal = Dinheiro.Criar(saldoAnterior + transacao.Efeito, "saldo").Valor;
        ReavaliarStatus(statusAnterior);

        AdicionarEvento(new TransacaoRegistradaEvent(
            Id, Titulo, transacao.Quantidade, tipo, transacao.DataTransacao,
            saldoAnterior, ValorTotal));

        if (statusAnterior != StatusObjetivo.Completo && Status == StatusObjetivo.Completo)
        {
            AdicionarEvento(new MetaAlcancadaEvent(
                Id, Titulo, QuantidadeAlvo, ValorTotal, Prazo, DiasRestantes ?? 0));
        }

        return transacao;
    }

    /// <summary>
    /// Soft delete da transação, desfazendo o efeito dela no ValorTotal.
    /// </summary>
    public void RemoverTransacao(int idTransacao)
    {
        var transacao = _transacoes.FirstOrDefault(t => t.Id == idTransacao && !t.EstaDeletado)
            ?? throw NaoEncontradoException.Para("Transação", idTransacao);

        var saldoDepois = ValorTotal - transacao.Efeito;

        // Remover um depósito pode deixar o saldo negativo: não permitimos.
        if (saldoDepois < 0)
            throw new RegraNegocioException(
                $"Remover esta transação deixaria o saldo em R$ {saldoDepois:N2}. " +
                "Remova primeiro os saques posteriores.");

        var statusAnterior = Status;

        transacao.Remover();
        ValorTotal = Dinheiro.Criar(saldoDepois, "saldo").Valor;
        ReavaliarStatus(statusAnterior);
    }

    /// <summary>
    /// Remove o objetivo por soft delete, junto com as transações dele.
    /// </summary>
    public void Remover()
    {
        if (ValorTotal > 0)
            throw new RegraNegocioException(
                $"'{Titulo}' ainda tem R$ {ValorTotal:N2} guardados. " +
                "Saque o valor antes de remover o objetivo.");

        MarcarComoDeletado();

        foreach (var transacao in _transacoes)
            transacao.Remover();
    }

    /// <summary>
    /// Recalcula o ValorTotal somando as transações ativas e devolve a
    /// diferença encontrada. Rede de segurança usada pelo HostedService
    /// para conferir o valor mantido incrementalmente.
    /// </summary>
    public decimal ConferirValorTotal()
    {
        var esperado = TransacoesAtivas.Sum(t => t.Efeito);
        var divergencia = esperado - ValorTotal;

        if (divergencia == 0) return 0m;

        var statusAnterior = Status;
        ValorTotal = esperado;
        ReavaliarStatus(statusAnterior);

        return divergencia;
    }

    /// <summary>Define a imagem de capa a partir dos bytes do arquivo enviado.</summary>
    public void DefinirCapa(byte[] bytes)
    {
        if (bytes is null || bytes.Length == 0)
            throw new RegraNegocioException("O arquivo de capa está vazio.");

        if (bytes.Length > TamanhoMaximoCapaBytes)
            throw new RegraNegocioException(
                $"A imagem de capa deve ter no máximo {TamanhoMaximoCapaBytes / 1024 / 1024}MB.");

        CapaObjetivo = bytes;
    }

    public void RemoverCapa() => CapaObjetivo = null;

    public void Pausar()
    {
        if (Status == StatusObjetivo.Cancelado)
            throw new RegraNegocioException("Um objetivo cancelado não pode ser pausado.");

        if (Status == StatusObjetivo.Pausado) return;

        AlterarStatus(StatusObjetivo.Pausado);
    }

    public void Retomar()
    {
        if (Status == StatusObjetivo.Cancelado)
            throw new RegraNegocioException("Um objetivo cancelado não pode ser retomado.");

        AlterarStatus(MetaAlcancada ? StatusObjetivo.Completo : StatusObjetivo.EmProgresso);
    }

    public void Cancelar()
    {
        if (Status == StatusObjetivo.Cancelado) return;

        AlterarStatus(StatusObjetivo.Cancelado);
    }

    /// <summary>
    /// Projeta a evolução da caixinha mês a mês, somando aportes mensais e o
    /// rendimento sobre o saldo acumulado (requisito PLUS).
    /// </summary>
    public ResultadoSimulacao Simular(
        decimal aporteMensal,
        int quantidadeMeses,
        decimal taxaJurosMensalPercentual)
    {
        var aporte = Dinheiro.Criar(aporteMensal, "aporte mensal").Valor;

        if (quantidadeMeses is <= 0 or > 600)
            throw new RegraNegocioException("A quantidade de meses deve estar entre 1 e 600.");

        if (taxaJurosMensalPercentual is < 0 or > 100)
            throw new RegraNegocioException("A taxa de juros mensal deve estar entre 0% e 100%.");

        var taxa = taxaJurosMensalPercentual / 100m;
        var saldo = ValorTotal;
        var totalAportado = 0m;
        var totalRendimentos = 0m;
        int? mesQueAtingeMeta = MetaAlcancada ? 0 : null;

        var meses = new List<MesProjetado>(quantidadeMeses);

        for (var mes = 1; mes <= quantidadeMeses; mes++)
        {
            // O aporte entra no começo do mês e rende junto com o saldo.
            saldo += aporte;
            totalAportado += aporte;

            var rendimento = Math.Round(saldo * taxa, 2);
            saldo = Math.Round(saldo + rendimento, 2);
            totalRendimentos += rendimento;

            if (mesQueAtingeMeta is null && saldo >= QuantidadeAlvo)
                mesQueAtingeMeta = mes;

            meses.Add(new MesProjetado(
                mes,
                DateTime.Today.AddMonths(mes),
                aporte,
                rendimento,
                saldo,
                QuantidadeAlvo == 0 ? 0 : Math.Min(100, Math.Round(saldo / QuantidadeAlvo * 100, 2))));
        }

        return new ResultadoSimulacao(
            Id,
            Titulo,
            ValorTotal,
            QuantidadeAlvo,
            aporte,
            taxaJurosMensalPercentual,
            quantidadeMeses,
            Math.Round(saldo, 2),
            Math.Round(totalAportado, 2),
            Math.Round(totalRendimentos, 2),
            mesQueAtingeMeta,
            mesQueAtingeMeta is null ? Math.Round(QuantidadeAlvo - saldo, 2) : null,
            meses);
    }

    // ------------------------------- Privados --------------------------------

    /// <summary>
    /// Marca como Completo ao bater a meta e volta a Em Progresso se o saldo
    /// cair abaixo dela. Caixinhas canceladas ou pausadas não mudam sozinhas.
    /// </summary>
    private void ReavaliarStatus(StatusObjetivo? statusAnterior = null)
    {
        if (Status is StatusObjetivo.Cancelado or StatusObjetivo.Pausado) return;

        var novo = MetaAlcancada ? StatusObjetivo.Completo : StatusObjetivo.EmProgresso;
        if (novo == Status) return;

        var anterior = statusAnterior ?? Status;
        Status = novo;

        AdicionarEvento(new StatusObjetivoAlteradoEvent(Id, Titulo, anterior, novo));
    }

    private void AlterarStatus(StatusObjetivo novo)
    {
        var anterior = Status;
        Status = novo;

        AdicionarEvento(new StatusObjetivoAlteradoEvent(Id, Titulo, anterior, novo));
    }

    private void DefinirTitulo(string titulo)
    {
        if (string.IsNullOrWhiteSpace(titulo) || titulo.Trim().Length < 3)
            throw new RegraNegocioException("O título é obrigatório e deve ter ao menos 3 caracteres.");

        if (titulo.Trim().Length > 120)
            throw new RegraNegocioException("O título deve ter no máximo 120 caracteres.");

        Titulo = titulo.Trim();
    }

    private void DefinirQuantidadeAlvo(decimal quantidadeAlvo)
        => QuantidadeAlvo = Dinheiro.CriarPositivo(quantidadeAlvo, "quantidade alvo").Valor;

    private void DefinirPrazo(DateTime? prazo)
    {
        if (prazo is not null && prazo.Value.Date <= DateTime.Today)
            throw new RegraNegocioException("O prazo, quando informado, deve ser uma data futura.");

        Prazo = prazo?.Date;
    }

    private void DefinirAporteMensalIdeal(decimal? aporte)
    {
        QuantidadeIdealAporteMensal = aporte is null
            ? null
            : Dinheiro.CriarPositivo(aporte.Value, "aporte mensal ideal").Valor;
    }

    private static int MesesAte(DateTime data)
    {
        var hoje = DateTime.Today;
        var meses = ((data.Year - hoje.Year) * 12) + data.Month - hoje.Month;
        if (data.Day < hoje.Day) meses--;
        return Math.Max(0, meses);
    }
}

/// <summary>Um mês dentro da projeção de evolução.</summary>
public record MesProjetado(
    int Mes,
    DateTime Data,
    decimal Aporte,
    decimal Rendimento,
    decimal SaldoAcumulado,
    decimal PercentualDaMeta);

/// <summary>Resultado da simulação de evolução da caixinha.</summary>
public record ResultadoSimulacao(
    int ObjetivoId,
    string Titulo,
    decimal SaldoInicial,
    decimal QuantidadeAlvo,
    decimal AporteMensal,
    decimal TaxaJurosMensalPercentual,
    int QuantidadeMeses,
    decimal SaldoFinal,
    decimal TotalAportado,
    decimal TotalRendimentos,
    int? MesQueAtingeMeta,
    decimal? ValorFaltanteAoFim,
    List<MesProjetado> Meses);
