using BancoSangue.Intermediario.Dominio.Excecoes;

namespace BancoSangue.Intermediario.Dominio.Entidades;

public class Doacao : EntidadeBase
{
    public const int QuantidadeMinimaML = 420;
    public const int QuantidadeMaximaML = 470;

    protected Doacao() { }

    public Doacao(int doadorId, int quantidadeML, DateTime dataDoacao)
    {
        // Regra de negócio: a quantidade de ml doados deve estar entre 420ml e 470ml.
        if (quantidadeML < QuantidadeMinimaML || quantidadeML > QuantidadeMaximaML)
            throw new RegraNegocioException(
                $"A quantidade doada deve estar entre {QuantidadeMinimaML}ml e {QuantidadeMaximaML}ml.");

        DoadorId = doadorId;
        QuantidadeML = quantidadeML;
        DataDoacao = dataDoacao;
    }

    public int DoadorId { get; private set; }
    public DateTime DataDoacao { get; private set; }
    public int QuantidadeML { get; private set; }

    public Doador? Doador { get; private set; }
}
