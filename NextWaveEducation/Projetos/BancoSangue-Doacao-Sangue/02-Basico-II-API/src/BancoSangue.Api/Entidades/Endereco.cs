namespace BancoSangue.Api.Entidades;

public class Endereco
{
    public int Id { get; set; }
    public string Logradouro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string CEP { get; set; } = string.Empty;

    public int DoadorId { get; set; }
    public Doador? Doador { get; set; }
}
