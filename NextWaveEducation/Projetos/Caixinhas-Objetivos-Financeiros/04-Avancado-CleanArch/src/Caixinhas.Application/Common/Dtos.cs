using Caixinhas.Domain.Entities;

namespace Caixinhas.Application.Common;

// ------------------------------ OBJETIVOS ------------------------------

public record ObjetivoDto(
    int Id,
    string Titulo,
    decimal QuantidadeAlvo,
    decimal ValorTotal,
    decimal ValorRestante,
    decimal PercentualConcluido,
    string Status,
    DateTime? Prazo,
    int? DiasRestantes,
    bool PrazoVencido,
    decimal? QuantidadeIdealAporteMensal,
    decimal? AporteMensalNecessario,
    int TotalTransacoes,
    bool TemCapa,
    DateTime DataCriacao)
{
    public static ObjetivoDto De(ObjetivoFinanceiro o) => new(
        o.Id,
        o.Titulo,
        o.QuantidadeAlvo,
        o.ValorTotal,
        o.ValorRestante,
        o.PercentualConcluido,
        o.Status.ToString(),
        o.Prazo,
        o.DiasRestantes,
        o.PrazoVencido,
        o.QuantidadeIdealAporteMensal,
        o.AporteMensalNecessario,
        o.TotalTransacoes,
        o.TemCapa,
        o.DataCriacao);
}

public record ResumoMovimentacaoDto(
    int QuantidadeDepositos,
    int QuantidadeSaques,
    decimal TotalDepositado,
    decimal TotalSacado,
    DateTime? UltimaMovimentacao);

public record ObjetivoDetalheDto(
    ObjetivoDto Objetivo,
    ResumoMovimentacaoDto Resumo,
    List<TransacaoDto> Transacoes)
{
    public static ObjetivoDetalheDto De(ObjetivoFinanceiro o)
    {
        var transacoes = o.TransacoesAtivas.ToList();

        return new ObjetivoDetalheDto(
            ObjetivoDto.De(o),
            new ResumoMovimentacaoDto(
                transacoes.Count(t => t.EhDeposito),
                transacoes.Count(t => !t.EhDeposito),
                transacoes.Where(t => t.EhDeposito).Sum(t => t.Quantidade),
                transacoes.Where(t => !t.EhDeposito).Sum(t => t.Quantidade),
                transacoes.Count == 0 ? null : transacoes.Max(t => t.DataTransacao)),
            transacoes
                .OrderByDescending(t => t.DataTransacao)
                .ThenByDescending(t => t.Id)
                .Select(TransacaoDto.De)
                .ToList());
    }
}

// ------------------------------ TRANSAÇÕES -----------------------------

public record TransacaoDto(
    int Id,
    int ObjetivoFinanceiroId,
    decimal Quantidade,
    string Tipo,
    DateTime DataTransacao,
    DateTime DataCriacao)
{
    public static TransacaoDto De(Transacao t) => new(
        t.Id,
        t.ObjetivoFinanceiroId,
        t.Quantidade,
        t.Tipo.ToString(),
        t.DataTransacao,
        t.DataCriacao);
}

/// <summary>
/// Resposta do cadastro de transação, com o ValorTotal já atualizado pela
/// entidade na mesma operação.
/// </summary>
public record ResultadoTransacaoDto(
    TransacaoDto Transacao,
    string TituloObjetivo,
    decimal ValorTotalAnterior,
    decimal ValorTotalAtualizado,
    decimal ValorRestante,
    decimal PercentualConcluido,
    string StatusObjetivo,
    bool MetaAlcancada,
    string? Mensagem);

public record LinhaExtratoDto(
    int Id,
    DateTime Data,
    string Tipo,
    decimal Quantidade,
    decimal SaldoApos);

// ------------------------------ SIMULAÇÃO ------------------------------

public record MesProjetadoDto(
    int Mes,
    DateTime Data,
    decimal Aporte,
    decimal Rendimento,
    decimal SaldoAcumulado,
    decimal PercentualDaMeta);

public record SimulacaoDto(
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
    DateTime? DataEstimadaDaMeta,
    decimal? ValorFaltanteAoFim,
    List<MesProjetadoDto> Meses)
{
    public static SimulacaoDto De(ResultadoSimulacao r) => new(
        r.ObjetivoId,
        r.Titulo,
        r.SaldoInicial,
        r.QuantidadeAlvo,
        r.AporteMensal,
        r.TaxaJurosMensalPercentual,
        r.QuantidadeMeses,
        r.SaldoFinal,
        r.TotalAportado,
        r.TotalRendimentos,
        r.MesQueAtingeMeta,
        r.MesQueAtingeMeta is null ? null : DateTime.Today.AddMonths(r.MesQueAtingeMeta.Value),
        r.ValorFaltanteAoFim,
        r.Meses.Select(m => new MesProjetadoDto(
            m.Mes, m.Data, m.Aporte, m.Rendimento, m.SaldoAcumulado, m.PercentualDaMeta))
            .ToList());
}

// ------------------------------ RELATÓRIOS -----------------------------

public record ItemRelatorioDto(
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

public record EvolucaoMensalDto(
    string Mes,
    decimal Depositos,
    decimal Saques,
    decimal Liquido,
    decimal SaldoAcumulado);

public record RelatorioEvolucaoDto(
    DateTime GeradoEm,
    int TotalObjetivos,
    int ObjetivosCompletos,
    int ObjetivosEmProgresso,
    int ObjetivosPausados,
    int ObjetivosCancelados,
    decimal PatrimonioTotal,
    decimal SomaDasMetas,
    decimal PercentualGeralConcluido,
    List<ItemRelatorioDto> Objetivos,
    List<EvolucaoMensalDto> EvolucaoMensal);

public record CapaObjetivoDto(byte[] Conteudo, string TipoConteudo);
