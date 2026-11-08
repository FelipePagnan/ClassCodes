using Caixinhas.Intermediario.Dominio.Entidades;

namespace Caixinhas.Intermediario.Aplicacao.ViewModels;

// ------------------------------ OBJETIVOS ------------------------------

public class ObjetivoViewModel
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public decimal QuantidadeAlvo { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal ValorRestante { get; set; }
    public decimal PercentualConcluido { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? Prazo { get; set; }
    public int? DiasRestantes { get; set; }
    public bool PrazoVencido { get; set; }
    public decimal? QuantidadeIdealAporteMensal { get; set; }
    public decimal? AporteMensalNecessario { get; set; }
    public int TotalTransacoes { get; set; }
    public bool TemCapa { get; set; }
    public DateTime DataCriacao { get; set; }

    public static ObjetivoViewModel De(ObjetivoFinanceiro o) => new()
    {
        Id = o.Id,
        Titulo = o.Titulo,
        QuantidadeAlvo = o.QuantidadeAlvo,
        ValorTotal = o.ValorTotal,
        ValorRestante = o.ValorRestante,
        PercentualConcluido = o.PercentualConcluido,
        Status = o.Status.ToString(),
        Prazo = o.Prazo,
        DiasRestantes = o.DiasRestantes,
        PrazoVencido = o.PrazoVencido,
        QuantidadeIdealAporteMensal = o.QuantidadeIdealAporteMensal,
        AporteMensalNecessario = o.AporteMensalNecessario,
        TotalTransacoes = o.TotalTransacoes,
        TemCapa = o.TemCapa,
        DataCriacao = o.DataCriacao
    };
}

public class ObjetivoDetalheViewModel : ObjetivoViewModel
{
    public decimal TotalDepositado { get; set; }
    public decimal TotalSacado { get; set; }
    public int QuantidadeDepositos { get; set; }
    public int QuantidadeSaques { get; set; }
    public DateTime? UltimaMovimentacao { get; set; }
    public List<TransacaoViewModel> Transacoes { get; set; } = new();

    public static ObjetivoDetalheViewModel DeDetalhe(ObjetivoFinanceiro o)
    {
        var baseVm = De(o);
        var transacoes = o.Transacoes.Where(t => !t.EstaDeletado).ToList();

        return new ObjetivoDetalheViewModel
        {
            Id = baseVm.Id,
            Titulo = baseVm.Titulo,
            QuantidadeAlvo = baseVm.QuantidadeAlvo,
            ValorTotal = baseVm.ValorTotal,
            ValorRestante = baseVm.ValorRestante,
            PercentualConcluido = baseVm.PercentualConcluido,
            Status = baseVm.Status,
            Prazo = baseVm.Prazo,
            DiasRestantes = baseVm.DiasRestantes,
            PrazoVencido = baseVm.PrazoVencido,
            QuantidadeIdealAporteMensal = baseVm.QuantidadeIdealAporteMensal,
            AporteMensalNecessario = baseVm.AporteMensalNecessario,
            TotalTransacoes = baseVm.TotalTransacoes,
            TemCapa = baseVm.TemCapa,
            DataCriacao = baseVm.DataCriacao,
            TotalDepositado = transacoes.Where(t => t.EhDeposito).Sum(t => t.Quantidade),
            TotalSacado = transacoes.Where(t => !t.EhDeposito).Sum(t => t.Quantidade),
            QuantidadeDepositos = transacoes.Count(t => t.EhDeposito),
            QuantidadeSaques = transacoes.Count(t => !t.EhDeposito),
            UltimaMovimentacao = transacoes.Count == 0 ? null : transacoes.Max(t => t.DataTransacao),
            Transacoes = transacoes
                .OrderByDescending(t => t.DataTransacao)
                .ThenByDescending(t => t.Id)
                .Select(TransacaoViewModel.De)
                .ToList()
        };
    }
}

// ------------------------------ TRANSAÇÕES -----------------------------

public class TransacaoViewModel
{
    public int Id { get; set; }
    public int ObjetivoFinanceiroId { get; set; }
    public decimal Quantidade { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public DateTime DataTransacao { get; set; }
    public DateTime DataCriacao { get; set; }

    public static TransacaoViewModel De(Transacao t) => new()
    {
        Id = t.Id,
        ObjetivoFinanceiroId = t.ObjetivoFinanceiroId,
        Quantidade = t.Quantidade,
        Tipo = t.Tipo.ToString(),
        DataTransacao = t.DataTransacao,
        DataCriacao = t.DataCriacao
    };
}

/// <summary>Resposta do cadastro de transação, com o saldo já atualizado.</summary>
public class ResultadoTransacaoViewModel
{
    public TransacaoViewModel Transacao { get; set; } = new();
    public string TituloObjetivo { get; set; } = string.Empty;
    public decimal ValorTotalAtualizado { get; set; }
    public decimal ValorRestante { get; set; }
    public decimal PercentualConcluido { get; set; }
    public string StatusObjetivo { get; set; } = string.Empty;
    public bool MetaAlcancada { get; set; }
    public string? Mensagem { get; set; }
}

/// <summary>Uma linha do extrato, com o saldo corrido após a movimentação.</summary>
public class LinhaExtratoViewModel
{
    public int Id { get; set; }
    public DateTime Data { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public decimal SaldoApos { get; set; }
}

// ------------------------------ SIMULAÇÃO ------------------------------

public class MesProjetadoViewModel
{
    public int Mes { get; set; }
    public DateTime Data { get; set; }
    public decimal Aporte { get; set; }
    public decimal Rendimento { get; set; }
    public decimal SaldoAcumulado { get; set; }
    public decimal PercentualDaMeta { get; set; }
}

public class SimulacaoViewModel
{
    public int ObjetivoId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public decimal SaldoInicial { get; set; }
    public decimal QuantidadeAlvo { get; set; }
    public decimal AporteMensal { get; set; }
    public decimal TaxaJurosMensalPercentual { get; set; }
    public int QuantidadeMeses { get; set; }
    public decimal SaldoFinal { get; set; }
    public decimal TotalAportado { get; set; }
    public decimal TotalRendimentos { get; set; }
    public int? MesQueAtingeMeta { get; set; }
    public DateTime? DataEstimadaDaMeta { get; set; }
    public decimal? ValorFaltanteAoFim { get; set; }
    public List<MesProjetadoViewModel> Meses { get; set; } = new();

    public static SimulacaoViewModel De(ResultadoSimulacao r) => new()
    {
        ObjetivoId = r.ObjetivoId,
        Titulo = r.Titulo,
        SaldoInicial = r.SaldoInicial,
        QuantidadeAlvo = r.QuantidadeAlvo,
        AporteMensal = r.AporteMensal,
        TaxaJurosMensalPercentual = r.TaxaJurosMensalPercentual,
        QuantidadeMeses = r.QuantidadeMeses,
        SaldoFinal = r.SaldoFinal,
        TotalAportado = r.TotalAportado,
        TotalRendimentos = r.TotalRendimentos,
        MesQueAtingeMeta = r.MesQueAtingeMeta,
        DataEstimadaDaMeta = r.MesQueAtingeMeta is null
            ? null
            : DateTime.Today.AddMonths(r.MesQueAtingeMeta.Value),
        ValorFaltanteAoFim = r.ValorFaltanteAoFim,
        Meses = r.Meses.Select(m => new MesProjetadoViewModel
        {
            Mes = m.Mes,
            Data = m.Data,
            Aporte = m.Aporte,
            Rendimento = m.Rendimento,
            SaldoAcumulado = m.SaldoAcumulado,
            PercentualDaMeta = m.PercentualDaMeta
        }).ToList()
    };
}

// ------------------------------ RELATÓRIOS -----------------------------

public class ItemRelatorioViewModel
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public decimal QuantidadeAlvo { get; set; }
    public decimal PercentualConcluido { get; set; }
    public int QuantidadeDepositos { get; set; }
    public int QuantidadeSaques { get; set; }
    public decimal TotalDepositado { get; set; }
    public decimal TotalSacado { get; set; }
    public DateTime? UltimaMovimentacao { get; set; }
    public DateTime? Prazo { get; set; }
    public int? DiasRestantes { get; set; }
    public bool PrazoVencido { get; set; }
    public decimal? AporteMensalNecessario { get; set; }
}

public class EvolucaoMensalViewModel
{
    public string Mes { get; set; } = string.Empty;
    public decimal Depositos { get; set; }
    public decimal Saques { get; set; }
    public decimal Liquido { get; set; }
    public decimal SaldoAcumulado { get; set; }
}

public class RelatorioEvolucaoViewModel
{
    public DateTime GeradoEm { get; set; }
    public int TotalObjetivos { get; set; }
    public int ObjetivosCompletos { get; set; }
    public int ObjetivosEmProgresso { get; set; }
    public int ObjetivosPausados { get; set; }
    public int ObjetivosCancelados { get; set; }
    public decimal PatrimonioTotal { get; set; }
    public decimal SomaDasMetas { get; set; }
    public decimal PercentualGeralConcluido { get; set; }
    public List<ItemRelatorioViewModel> Objetivos { get; set; } = new();
    public List<EvolucaoMensalViewModel> EvolucaoMensal { get; set; } = new();
}
