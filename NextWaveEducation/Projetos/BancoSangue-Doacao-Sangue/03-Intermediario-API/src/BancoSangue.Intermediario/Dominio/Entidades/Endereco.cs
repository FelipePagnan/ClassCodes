namespace BancoSangue.Intermediario.Dominio.Entidades;

public class Endereco : EntidadeBase
{
    protected Endereco() { }

    public Endereco(string logradouro, string cidade, string estado, string cep)
    {
        Logradouro = logradouro;
        Cidade = cidade;
        Estado = estado;
        CEP = cep;
    }

    public string Logradouro { get; private set; } = string.Empty;
    public string Cidade { get; private set; } = string.Empty;
    public string Estado { get; private set; } = string.Empty;
    public string CEP { get; private set; } = string.Empty;

    public int DoadorId { get; private set; }
    public Doador? Doador { get; private set; }

    public void VincularDoador(Doador doador)
    {
        Doador = doador;
        DoadorId = doador.Id;
    }
}
