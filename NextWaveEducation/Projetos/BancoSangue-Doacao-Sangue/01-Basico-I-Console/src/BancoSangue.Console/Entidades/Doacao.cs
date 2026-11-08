namespace BancoSangue.Console.Entidades;

public class Doacao
{
    public int Id { get; set; }
    public int DoadorId { get; set; }
    public DateTime DataDoacao { get; set; }
    public int QuantidadeML { get; set; }

    public Doador? Doador { get; set; }

    public override string ToString()
        => $"Doação #{Id} em {DataDoacao:dd/MM/yyyy} - {QuantidadeML}ml";
}
