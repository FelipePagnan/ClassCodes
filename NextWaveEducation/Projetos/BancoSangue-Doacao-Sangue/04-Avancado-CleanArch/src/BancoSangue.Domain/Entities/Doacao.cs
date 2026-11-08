using BancoSangue.Domain.Common;
using BancoSangue.Domain.Exceptions;

namespace BancoSangue.Domain.Entities;

public class Doacao : BaseEntity
{
    /// <summary>Quantidade mínima de sangue aceita por doação, em mililitros.</summary>
    public const int QuantidadeMinimaML = 420;

    /// <summary>Quantidade máxima de sangue aceita por doação, em mililitros.</summary>
    public const int QuantidadeMaximaML = 470;

    // Construtor sem parâmetros para o EF Core.
    private Doacao() { }

    internal Doacao(int doadorId, int quantidadeML, DateTime dataDoacao)
    {
        // Regra de negócio: a quantidade doada deve estar entre 420ml e 470ml.
        if (quantidadeML < QuantidadeMinimaML || quantidadeML > QuantidadeMaximaML)
            throw new RegraNegocioException(
                $"A quantidade doada deve estar entre {QuantidadeMinimaML}ml e {QuantidadeMaximaML}ml. " +
                $"Valor informado: {quantidadeML}ml.");

        if (dataDoacao > DateTime.Now.AddMinutes(5))
            throw new RegraNegocioException("A data da doação não pode estar no futuro.");

        DoadorId = doadorId;
        QuantidadeML = quantidadeML;
        DataDoacao = dataDoacao;
    }

    public int DoadorId { get; private set; }
    public DateTime DataDoacao { get; private set; }
    public int QuantidadeML { get; private set; }

    public Doador Doador { get; private set; } = null!;
}
