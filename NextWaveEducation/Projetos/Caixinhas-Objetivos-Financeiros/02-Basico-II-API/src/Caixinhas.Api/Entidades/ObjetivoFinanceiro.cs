using Caixinhas.Api.Enums;
using Caixinhas.Api.Excecoes;

namespace Caixinhas.Api.Entidades;

public class ObjetivoFinanceiro
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public decimal QuantidadeAlvo { get; set; }
    public DateTime? Prazo { get; set; }
    public decimal? QuantidadeIdealAporteMensal { get; set; }
    public StatusObjetivo Status { get; set; } = StatusObjetivo.EmProgresso;

    public List<Transacao> Transacoes { get; set; } = new();

    public DateTime DataCriacao { get; set; } = DateTime.Now;

    /// <summary>Soft delete: sai das listagens mas continua no banco.</summary>
    public bool EstaDeletado { get; set; }

    /// <summary>
    /// Saldo mantido incrementalmente a cada transação (requisito PLUS),
    /// em vez de somar todas as transações a cada consulta.
    /// </summary>
    public decimal ValorTotal { get; set; }

    public decimal ValorRestante => Math.Max(0, QuantidadeAlvo - ValorTotal);

    public decimal PercentualConcluido => QuantidadeAlvo == 0
        ? 0
        : Math.Min(100, Math.Round(ValorTotal / QuantidadeAlvo * 100, 2));

    public bool MetaAlcancada => ValorTotal >= QuantidadeAlvo;

    public int? DiasRestantes => Prazo is null
        ? null
        : (int)(Prazo.Value.Date - DateTime.Today).TotalDays;

    public bool AceitaTransacoes => Status is StatusObjetivo.EmProgresso;

    /// <summary>
    /// Registra a transação, atualiza o ValorTotal e reavalia o status.
    /// </summary>
    public Transacao RegistrarTransacao(decimal quantidade, TipoTransacao tipo, DateTime dataTransacao)
    {
        if (!AceitaTransacoes)
            throw new RegraNegocioException(
                $"O objetivo '{Titulo}' está {Status} e não aceita novas transações.");

        Transacao.ValidarQuantidade(quantidade);
        Transacao.ValidarDataTransacao(dataTransacao);

        if (tipo == TipoTransacao.Saque && quantidade > ValorTotal)
            throw new RegraNegocioException(
                $"Saldo insuficiente em '{Titulo}'. Disponível: R$ {ValorTotal:N2}, " +
                $"solicitado: R$ {quantidade:N2}.");

        var transacao = new Transacao
        {
            ObjetivoFinanceiroId = Id,
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

    /// <summary>Soft delete da transação, desfazendo o efeito no ValorTotal.</summary>
    public void RemoverTransacao(Transacao transacao)
    {
        var saldoDepois = ValorTotal - transacao.Efeito;

        if (saldoDepois < 0)
            throw new RegraNegocioException(
                $"Remover esta transação deixaria o saldo em R$ {saldoDepois:N2}.");

        transacao.EstaDeletado = true;
        ValorTotal = saldoDepois;
        ReavaliarStatus();
    }

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
}
