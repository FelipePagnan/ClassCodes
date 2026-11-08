namespace BancoSangue.Intermediario.Aplicacao.InputModels;

public class CriarDoacaoInputModel
{
    public int DoadorId { get; set; }
    public int QuantidadeML { get; set; }

    /// <summary>Data da doação. Se não informada, usa a data/hora atual.</summary>
    public DateTime? DataDoacao { get; set; }
}
