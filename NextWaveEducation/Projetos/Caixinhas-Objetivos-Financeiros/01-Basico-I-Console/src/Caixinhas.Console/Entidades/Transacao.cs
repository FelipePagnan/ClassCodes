using Caixinhas.Console.Enums;
using Caixinhas.Console.Excecoes;

namespace Caixinhas.Console.Entidades;

public class Transacao
{
    /// <summary>Número máximo de casas decimais aceito numa transação.</summary>
    public const int CasasDecimais = 2;

    public int Id { get; set; }
    public decimal Quantidade { get; set; }
    public TipoTransacao Tipo { get; set; }

    /// <summary>Data em que a movimentação aconteceu. Pode ser hoje ou passada.</summary>
    public DateTime DataTransacao { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.Now;

    /// <summary>Soft delete: a transação sai dos cálculos mas continua no histórico.</summary>
    public bool EstaDeletado { get; set; }

    public int ObjetivoFinanceiroId { get; set; }
    public ObjetivoFinanceiro? ObjetivoFinanceiro { get; set; }

    /// <summary>
    /// Efeito da transação no saldo: positivo para depósito, negativo para saque.
    /// </summary>
    public decimal Efeito => Tipo == TipoTransacao.Deposito ? Quantidade : -Quantidade;

    public string Sinal => Tipo == TipoTransacao.Deposito ? "+" : "-";

    /// <summary>
    /// Valida as regras de negócio da transação: valor positivo e com no
    /// máximo duas casas decimais.
    /// </summary>
    public static void ValidarQuantidade(decimal quantidade)
    {
        // Regra de negócio: a quantidade não pode ser negativa.
        if (quantidade < 0)
            throw new RegraNegocioException(
                $"A quantidade da transação não pode ser negativa. Valor informado: {quantidade}.");

        if (quantidade == 0)
            throw new RegraNegocioException("A quantidade da transação deve ser maior que zero.");

        // Regra de negócio: até duas casas decimais de precisão.
        if (decimal.Round(quantidade, CasasDecimais) != quantidade)
            throw new RegraNegocioException(
                $"A quantidade deve ter no máximo {CasasDecimais} casas decimais. " +
                $"Valor informado: {quantidade}.");
    }

    public static void ValidarDataTransacao(DateTime dataTransacao)
    {
        // A data pode ser hoje ou passada, nunca futura.
        if (dataTransacao.Date > DateTime.Today)
            throw new RegraNegocioException(
                $"A data da transação ({dataTransacao:dd/MM/yyyy}) não pode estar no futuro.");

        if (dataTransacao.Year < 1900)
            throw new RegraNegocioException("A data da transação é inválida.");
    }

    public override string ToString()
        => $"[{Id}] {DataTransacao:dd/MM/yyyy} {Sinal} R$ {Quantidade,10:N2}  ({Tipo})";
}
