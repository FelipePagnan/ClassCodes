using Caixinhas.Api.Enums;
using Caixinhas.Api.Excecoes;

namespace Caixinhas.Api.Entidades;

public class Transacao
{
    public const int CasasDecimais = 2;

    public int Id { get; set; }
    public decimal Quantidade { get; set; }
    public TipoTransacao Tipo { get; set; }

    /// <summary>Data em que a movimentação aconteceu. Pode ser hoje ou passada.</summary>
    public DateTime DataTransacao { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.Now;

    /// <summary>Soft delete: sai dos cálculos mas continua no banco.</summary>
    public bool EstaDeletado { get; set; }

    public int ObjetivoFinanceiroId { get; set; }
    public ObjetivoFinanceiro? ObjetivoFinanceiro { get; set; }

    /// <summary>Efeito no saldo: positivo para depósito, negativo para saque.</summary>
    public decimal Efeito => Tipo == TipoTransacao.Deposito ? Quantidade : -Quantidade;

    /// <summary>
    /// Regras de negócio: a quantidade não pode ser negativa e deve ter
    /// no máximo duas casas decimais.
    /// </summary>
    public static void ValidarQuantidade(decimal quantidade)
    {
        if (quantidade < 0)
            throw new RegraNegocioException(
                $"A quantidade da transação não pode ser negativa. Valor informado: {quantidade}.");

        if (quantidade == 0)
            throw new RegraNegocioException("A quantidade da transação deve ser maior que zero.");

        if (decimal.Round(quantidade, CasasDecimais) != quantidade)
            throw new RegraNegocioException(
                $"A quantidade deve ter no máximo {CasasDecimais} casas decimais. " +
                $"Valor informado: {quantidade}.");
    }

    public static void ValidarDataTransacao(DateTime dataTransacao)
    {
        if (dataTransacao.Date > DateTime.Today)
            throw new RegraNegocioException(
                $"A data da transação ({dataTransacao:dd/MM/yyyy}) não pode estar no futuro.");

        if (dataTransacao.Year < 1900)
            throw new RegraNegocioException("A data da transação é inválida.");
    }
}
