using Caixinhas.Console.Enums;
using Caixinhas.Console.Excecoes;

namespace Caixinhas.Console.Entidades;

/// <summary>
/// A "caixinha": um objetivo financeiro com meta, prazo e histórico de transações.
/// </summary>
public class ObjetivoFinanceiro
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public decimal QuantidadeAlvo { get; set; }

    /// <summary>Prazo para atingir a meta (opcional).</summary>
    public DateTime? Prazo { get; set; }

    /// <summary>Aporte mensal ideal informado pelo usuário (opcional).</summary>
    public decimal? QuantidadeIdealAporteMensal { get; set; }

    public StatusObjetivo Status { get; set; } = StatusObjetivo.EmProgresso;

    public List<Transacao> Transacoes { get; set; } = new();

    public DateTime DataCriacao { get; set; } = DateTime.Now;

    /// <summary>Soft delete: o objetivo sai das listagens mas continua no banco.</summary>
    public bool EstaDeletado { get; set; }

    /// <summary>
    /// Saldo atual da caixinha. É mantido incrementalmente a cada transação
    /// (requisito PLUS), em vez de somar todas as transações a cada consulta.
    /// </summary>
    public decimal ValorTotal { get; private set; }

    /// <summary>Imagem de capa do objetivo armazenada como bytes.</summary>
    public byte[]? CapaObjetivo { get; set; }

    public bool TemCapa => CapaObjetivo is { Length: > 0 };

    /// <summary>Transações ativas, ignorando as removidas por soft delete.</summary>
    public IEnumerable<Transacao> TransacoesAtivas => Transacoes.Where(t => !t.EstaDeletado);

    public decimal ValorRestante => Math.Max(0, QuantidadeAlvo - ValorTotal);

    /// <summary>Percentual da meta já alcançado, limitado a 100%.</summary>
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

    /// <summary>
    /// Quanto falta guardar por mês para bater a meta até o prazo.
    /// Calculado com base no que falta e nos meses restantes.
    /// </summary>
    public decimal? AporteMensalNecessario
    {
        get
        {
            if (Prazo is null || MetaAlcancada) return null;

            var mesesRestantes = MesesAte(Prazo.Value);
            if (mesesRestantes <= 0) return ValorRestante;

            return Math.Round(ValorRestante / mesesRestantes, 2);
        }
    }

    /// <summary>
    /// Aceita transações apenas quando está em progresso. Objetivos
    /// completos, pausados ou cancelados ficam congelados.
    /// </summary>
    public bool AceitaTransacoes => Status is StatusObjetivo.EmProgresso;

    // ----------------------------- Comportamento -----------------------------

    /// <summary>
    /// Registra uma transação, atualiza o ValorTotal e reavalia o status.
    /// Lança <see cref="RegraNegocioException"/> quando alguma regra falha.
    /// </summary>
    public Transacao RegistrarTransacao(
        decimal quantidade,
        TipoTransacao tipo,
        DateTime dataTransacao,
        int proximoId)
    {
        if (EstaDeletado)
            throw new RegraNegocioException($"O objetivo '{Titulo}' foi removido.");

        if (!AceitaTransacoes)
            throw new RegraNegocioException(
                $"O objetivo '{Titulo}' está {Status} e não aceita novas transações.");

        Transacao.ValidarQuantidade(quantidade);
        Transacao.ValidarDataTransacao(dataTransacao);

        if (dataTransacao.Date < DataCriacao.Date)
            throw new RegraNegocioException(
                $"A data da transação ({dataTransacao:dd/MM/yyyy}) não pode ser anterior " +
                $"à criação do objetivo ({DataCriacao:dd/MM/yyyy}).");

        // Um saque não pode deixar a caixinha com saldo negativo.
        if (tipo == TipoTransacao.Saque && quantidade > ValorTotal)
            throw new RegraNegocioException(
                $"Saldo insuficiente em '{Titulo}'. Disponível: R$ {ValorTotal:N2}, " +
                $"solicitado: R$ {quantidade:N2}.");

        var transacao = new Transacao
        {
            Id = proximoId,
            ObjetivoFinanceiroId = Id,
            ObjetivoFinanceiro = this,
            Quantidade = quantidade,
            Tipo = tipo,
            DataTransacao = dataTransacao.Date,
            DataCriacao = DateTime.Now
        };

        Transacoes.Add(transacao);

        // O ValorTotal é atualizado aqui, no cadastro da transação.
        ValorTotal += transacao.Efeito;
        ReavaliarStatus();

        return transacao;
    }

    /// <summary>
    /// Remove a transação por soft delete e desfaz o efeito dela no ValorTotal.
    /// </summary>
    public void RemoverTransacao(int idTransacao)
    {
        var transacao = Transacoes.FirstOrDefault(t => t.Id == idTransacao && !t.EstaDeletado)
            ?? throw new RegraNegocioException(
                $"Transação {idTransacao} não encontrada no objetivo '{Titulo}'.");

        // Remover um depósito pode deixar o saldo negativo: não permitimos.
        var saldoDepois = ValorTotal - transacao.Efeito;
        if (saldoDepois < 0)
            throw new RegraNegocioException(
                $"Remover esta transação deixaria o saldo em R$ {saldoDepois:N2}. " +
                "Remova primeiro os saques posteriores.");

        transacao.EstaDeletado = true;
        ValorTotal = saldoDepois;
        ReavaliarStatus();
    }

    /// <summary>
    /// Recalcula o ValorTotal somando todas as transações ativas.
    /// Serve como rede de segurança para conferir o valor incremental.
    /// </summary>
    public decimal RecalcularValorTotal()
    {
        ValorTotal = TransacoesAtivas.Sum(t => t.Efeito);
        ReavaliarStatus();
        return ValorTotal;
    }

    /// <summary>
    /// Marca como Completo ao bater a meta e volta a Em Progresso se o
    /// saldo cair abaixo dela (por exemplo após um saque).
    /// </summary>
    private void ReavaliarStatus()
    {
        if (Status is StatusObjetivo.Cancelado or StatusObjetivo.Pausado) return;

        Status = MetaAlcancada ? StatusObjetivo.Completo : StatusObjetivo.EmProgresso;
    }

    public void Pausar()
    {
        if (Status == StatusObjetivo.Cancelado)
            throw new RegraNegocioException("Um objetivo cancelado não pode ser pausado.");

        Status = StatusObjetivo.Pausado;
    }

    public void Retomar()
    {
        if (Status == StatusObjetivo.Cancelado)
            throw new RegraNegocioException("Um objetivo cancelado não pode ser retomado.");

        Status = MetaAlcancada ? StatusObjetivo.Completo : StatusObjetivo.EmProgresso;
    }

    public void Cancelar() => Status = StatusObjetivo.Cancelado;

    /// <summary>
    /// Projeta a evolução da caixinha mês a mês, somando aportes mensais e
    /// o rendimento sobre o saldo acumulado (requisito PLUS).
    /// </summary>
    public SimulacaoEvolucao Simular(
        decimal aporteMensal,
        int quantidadeMeses,
        decimal taxaJurosMensalPercentual)
    {
        if (aporteMensal < 0)
            throw new RegraNegocioException("O aporte mensal não pode ser negativo.");

        if (quantidadeMeses is <= 0 or > 600)
            throw new RegraNegocioException("A quantidade de meses deve estar entre 1 e 600.");

        if (taxaJurosMensalPercentual is < 0 or > 100)
            throw new RegraNegocioException("A taxa de juros mensal deve estar entre 0% e 100%.");

        var taxa = taxaJurosMensalPercentual / 100m;
        var saldo = ValorTotal;
        var totalAportado = 0m;
        var totalRendimentos = 0m;
        int? mesQueAtingeMeta = MetaAlcancada ? 0 : null;

        var meses = new List<MesSimulado>(quantidadeMeses);

        for (var mes = 1; mes <= quantidadeMeses; mes++)
        {
            // O aporte entra no começo do mês e rende junto com o saldo.
            saldo += aporteMensal;
            totalAportado += aporteMensal;

            var rendimento = Math.Round(saldo * taxa, 2);
            saldo = Math.Round(saldo + rendimento, 2);
            totalRendimentos += rendimento;

            if (mesQueAtingeMeta is null && saldo >= QuantidadeAlvo)
                mesQueAtingeMeta = mes;

            meses.Add(new MesSimulado(
                mes,
                DateTime.Today.AddMonths(mes),
                aporteMensal,
                rendimento,
                saldo,
                QuantidadeAlvo == 0 ? 0 : Math.Min(100, Math.Round(saldo / QuantidadeAlvo * 100, 2))));
        }

        return new SimulacaoEvolucao(
            Titulo,
            ValorTotal,
            QuantidadeAlvo,
            aporteMensal,
            taxaJurosMensalPercentual,
            quantidadeMeses,
            Math.Round(saldo, 2),
            Math.Round(totalAportado, 2),
            Math.Round(totalRendimentos, 2),
            mesQueAtingeMeta,
            meses);
    }

    private static int MesesAte(DateTime data)
    {
        var hoje = DateTime.Today;
        var meses = ((data.Year - hoje.Year) * 12) + data.Month - hoje.Month;
        if (data.Day < hoje.Day) meses--;
        return Math.Max(0, meses);
    }

    public override string ToString()
        => $"[{Id}] {Titulo,-28} R$ {ValorTotal,12:N2} / R$ {QuantidadeAlvo,12:N2}  " +
           $"({PercentualConcluido,6:N2}%)  {Status}";
}

/// <summary>Um mês dentro da projeção de evolução.</summary>
public record MesSimulado(
    int Mes,
    DateTime Data,
    decimal Aporte,
    decimal Rendimento,
    decimal SaldoAcumulado,
    decimal PercentualDaMeta);

/// <summary>Resultado da simulação de evolução da caixinha.</summary>
public record SimulacaoEvolucao(
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
    List<MesSimulado> Meses);
