namespace BancoSangue.Api.Entidades;

public class Doacao
{
    public int Id { get; set; }
    public int DoadorId { get; set; }
    public DateTime DataDoacao { get; set; }
    public int QuantidadeML { get; set; }

    public Doador? Doador { get; set; }
}
