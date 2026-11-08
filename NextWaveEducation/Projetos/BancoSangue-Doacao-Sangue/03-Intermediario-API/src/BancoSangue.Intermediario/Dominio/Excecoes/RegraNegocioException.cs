namespace BancoSangue.Intermediario.Dominio.Excecoes;

public class RegraNegocioException : Exception
{
    public RegraNegocioException(string mensagem) : base(mensagem) { }
}
