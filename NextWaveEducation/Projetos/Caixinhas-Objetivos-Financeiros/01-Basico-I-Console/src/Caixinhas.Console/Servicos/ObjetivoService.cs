using Caixinhas.Console.Entidades;
using Caixinhas.Console.Enums;
using Caixinhas.Console.Excecoes;

namespace Caixinhas.Console.Servicos;

/// <summary>
/// Gerenciamento das caixinhas: cadastro, atualização, remoção (soft delete),
/// listagem, detalhes, capa e relatórios.
/// </summary>
public class ObjetivoService
{
    private readonly List<ObjetivoFinanceiro> _objetivos = new();
    private int _proximoId = 1;
    private int _proximoIdTransacao = 1;

    /// <summary>Lista apenas os objetivos ativos (ignora os soft-deletados).</summary>
    public IReadOnlyList<ObjetivoFinanceiro> ListarTodos()
        => _objetivos.Where(o => !o.EstaDeletado).ToList().AsReadOnly();

    /// <summary>Lista tudo, inclusive os removidos. Útil para auditoria.</summary>
    public IReadOnlyList<ObjetivoFinanceiro> ListarIncluindoRemovidos()
        => _objetivos.AsReadOnly();

    public IEnumerable<ObjetivoFinanceiro> ListarPorStatus(StatusObjetivo status)
        => ListarTodos().Where(o => o.Status == status);

    public ObjetivoFinanceiro? BuscarPorId(int id)
        => _objetivos.FirstOrDefault(o => o.Id == id && !o.EstaDeletado);

    public ObjetivoFinanceiro ObterPorId(int id)
        => BuscarPorId(id)
           ?? throw new RegraNegocioException($"Objetivo financeiro {id} não encontrado.");

    public ObjetivoFinanceiro Cadastrar(ObjetivoFinanceiro objetivo)
    {
        ValidarDados(objetivo);

        if (ListarTodos().Any(o =>
                o.Titulo.Equals(objetivo.Titulo.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new RegraNegocioException($"Já existe um objetivo chamado '{objetivo.Titulo}'.");

        objetivo.Id = _proximoId++;
        objetivo.Titulo = objetivo.Titulo.Trim();
        objetivo.DataCriacao = DateTime.Now;
        objetivo.Status = StatusObjetivo.EmProgresso;
        objetivo.EstaDeletado = false;

        _objetivos.Add(objetivo);
        return objetivo;
    }

    public ObjetivoFinanceiro Atualizar(
        int id,
        string titulo,
        decimal quantidadeAlvo,
        DateTime? prazo,
        decimal? aporteMensalIdeal)
    {
        var objetivo = ObterPorId(id);

        var dados = new ObjetivoFinanceiro
        {
            Titulo = titulo,
            QuantidadeAlvo = quantidadeAlvo,
            Prazo = prazo,
            QuantidadeIdealAporteMensal = aporteMensalIdeal,
            DataCriacao = objetivo.DataCriacao
        };
        ValidarDados(dados);

        if (ListarTodos().Any(o =>
                o.Id != id &&
                o.Titulo.Equals(titulo.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new RegraNegocioException($"Já existe outro objetivo chamado '{titulo}'.");

        // A meta não pode ficar abaixo do que já foi guardado.
        if (quantidadeAlvo < objetivo.ValorTotal)
            throw new RegraNegocioException(
                $"A nova meta (R$ {quantidadeAlvo:N2}) é menor que o saldo atual " +
                $"(R$ {objetivo.ValorTotal:N2}).");

        objetivo.Titulo = titulo.Trim();
        objetivo.QuantidadeAlvo = quantidadeAlvo;
        objetivo.Prazo = prazo?.Date;
        objetivo.QuantidadeIdealAporteMensal = aporteMensalIdeal;

        // A meta mudou, então o status pode mudar também.
        objetivo.RecalcularValorTotal();

        return objetivo;
    }

    /// <summary>Remoção lógica: marca EstaDeletado em vez de apagar o registro.</summary>
    public void Remover(int id)
    {
        var objetivo = ObterPorId(id);

        if (objetivo.ValorTotal > 0)
            throw new RegraNegocioException(
                $"'{objetivo.Titulo}' ainda tem R$ {objetivo.ValorTotal:N2} guardados. " +
                "Saque o valor antes de remover o objetivo.");

        objetivo.EstaDeletado = true;

        // As transações do objetivo acompanham a remoção.
        foreach (var transacao in objetivo.Transacoes)
            transacao.EstaDeletado = true;
    }

    public ObjetivoFinanceiro DefinirCapa(int id, string caminhoArquivo)
    {
        var objetivo = ObterPorId(id);

        if (!File.Exists(caminhoArquivo))
            throw new RegraNegocioException($"Arquivo não encontrado: {caminhoArquivo}");

        var extensao = Path.GetExtension(caminhoArquivo).ToLowerInvariant();
        string[] permitidas = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"];
        if (!permitidas.Contains(extensao))
            throw new RegraNegocioException(
                $"Extensão '{extensao}' não permitida. Use: {string.Join(", ", permitidas)}");

        var bytes = File.ReadAllBytes(caminhoArquivo);

        const int tamanhoMaximo = 5 * 1024 * 1024;
        if (bytes.Length > tamanhoMaximo)
            throw new RegraNegocioException("A imagem de capa deve ter no máximo 5MB.");

        objetivo.CapaObjetivo = bytes;
        return objetivo;
    }

    /// <summary>Entrega o próximo id de transação, mantido pelo serviço.</summary>
    public int ProximoIdTransacao() => _proximoIdTransacao++;

    /// <summary>
    /// Relatório da evolução de todas as caixinhas (requisito PLUS).
    /// </summary>
    public RelatorioEvolucao GerarRelatorioEvolucao(int mesesDeHistorico = 6)
    {
        var ativos = ListarTodos();

        var itens = ativos.Select(o =>
        {
            var transacoes = o.TransacoesAtivas.ToList();

            return new ItemRelatorioEvolucao(
                o.Id,
                o.Titulo,
                o.Status.ToString(),
                o.ValorTotal,
                o.QuantidadeAlvo,
                o.PercentualConcluido,
                transacoes.Count(t => t.Tipo == TipoTransacao.Deposito),
                transacoes.Count(t => t.Tipo == TipoTransacao.Saque),
                transacoes.Where(t => t.Tipo == TipoTransacao.Deposito).Sum(t => t.Quantidade),
                transacoes.Where(t => t.Tipo == TipoTransacao.Saque).Sum(t => t.Quantidade),
                transacoes.Count == 0 ? null : transacoes.Max(t => t.DataTransacao),
                o.Prazo,
                o.DiasRestantes,
                o.PrazoVencido,
                o.AporteMensalNecessario);
        })
        .OrderByDescending(i => i.PercentualConcluido)
        .ToList();

        // Evolução consolidada mês a mês, somando todas as caixinhas.
        var referencia = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var evolucaoMensal = new List<EvolucaoMensal>();
        var acumulado = 0m;

        for (var i = mesesDeHistorico - 1; i >= 0; i--)
        {
            var mes = referencia.AddMonths(-i);
            var fimDoMes = mes.AddMonths(1).AddDays(-1);

            var doMes = ativos
                .SelectMany(o => o.TransacoesAtivas)
                .Where(t => t.DataTransacao >= mes && t.DataTransacao <= fimDoMes)
                .ToList();

            var depositos = doMes.Where(t => t.Tipo == TipoTransacao.Deposito).Sum(t => t.Quantidade);
            var saques = doMes.Where(t => t.Tipo == TipoTransacao.Saque).Sum(t => t.Quantidade);

            // O acumulado considera tudo que aconteceu até o fim daquele mês.
            acumulado = ativos
                .SelectMany(o => o.TransacoesAtivas)
                .Where(t => t.DataTransacao <= fimDoMes)
                .Sum(t => t.Efeito);

            evolucaoMensal.Add(new EvolucaoMensal(
                $"{mes:MM/yyyy}", depositos, saques, depositos - saques, acumulado));
        }

        return new RelatorioEvolucao(
            DateTime.Now,
            ativos.Count,
            ativos.Count(o => o.Status == StatusObjetivo.Completo),
            ativos.Count(o => o.Status == StatusObjetivo.EmProgresso),
            ativos.Sum(o => o.ValorTotal),
            ativos.Sum(o => o.QuantidadeAlvo),
            itens,
            evolucaoMensal);
    }

    private static void ValidarDados(ObjetivoFinanceiro objetivo)
    {
        if (string.IsNullOrWhiteSpace(objetivo.Titulo))
            throw new RegraNegocioException("O título do objetivo é obrigatório.");

        if (objetivo.Titulo.Trim().Length < 3)
            throw new RegraNegocioException("O título deve ter ao menos 3 caracteres.");

        if (objetivo.Titulo.Trim().Length > 120)
            throw new RegraNegocioException("O título deve ter no máximo 120 caracteres.");

        if (objetivo.QuantidadeAlvo <= 0)
            throw new RegraNegocioException("A quantidade alvo deve ser maior que zero.");

        if (decimal.Round(objetivo.QuantidadeAlvo, 2) != objetivo.QuantidadeAlvo)
            throw new RegraNegocioException("A quantidade alvo deve ter no máximo 2 casas decimais.");

        if (objetivo.QuantidadeAlvo > 999_999_999m)
            throw new RegraNegocioException("A quantidade alvo informada é inválida.");

        if (objetivo.Prazo is not null && objetivo.Prazo.Value.Date <= DateTime.Today)
            throw new RegraNegocioException("O prazo, quando informado, deve ser uma data futura.");

        if (objetivo.QuantidadeIdealAporteMensal is { } aporte)
        {
            if (aporte <= 0)
                throw new RegraNegocioException("O aporte mensal ideal deve ser maior que zero.");

            if (decimal.Round(aporte, 2) != aporte)
                throw new RegraNegocioException(
                    "O aporte mensal ideal deve ter no máximo 2 casas decimais.");
        }
    }
}

public record ItemRelatorioEvolucao(
    int Id,
    string Titulo,
    string Status,
    decimal ValorTotal,
    decimal QuantidadeAlvo,
    decimal PercentualConcluido,
    int QuantidadeDepositos,
    int QuantidadeSaques,
    decimal TotalDepositado,
    decimal TotalSacado,
    DateTime? UltimaMovimentacao,
    DateTime? Prazo,
    int? DiasRestantes,
    bool PrazoVencido,
    decimal? AporteMensalNecessario);

public record EvolucaoMensal(
    string Mes,
    decimal Depositos,
    decimal Saques,
    decimal Liquido,
    decimal SaldoAcumulado);

public record RelatorioEvolucao(
    DateTime GeradoEm,
    int TotalObjetivos,
    int ObjetivosCompletos,
    int ObjetivosEmProgresso,
    decimal PatrimonioTotal,
    decimal SomaDasMetas,
    List<ItemRelatorioEvolucao> Objetivos,
    List<EvolucaoMensal> EvolucaoMensal);
