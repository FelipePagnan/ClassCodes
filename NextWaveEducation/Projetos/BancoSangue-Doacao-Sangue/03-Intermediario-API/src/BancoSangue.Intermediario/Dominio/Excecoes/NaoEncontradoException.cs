namespace BancoSangue.Intermediario.Dominio.Excecoes;

public class NaoEncontradoException : Exception
{
    public NaoEncontradoException(string mensagem) : base(mensagem) { }
}
