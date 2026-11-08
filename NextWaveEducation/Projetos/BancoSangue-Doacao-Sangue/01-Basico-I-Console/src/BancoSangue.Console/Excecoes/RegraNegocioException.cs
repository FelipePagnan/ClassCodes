namespace BancoSangue.Console.Excecoes;

/// <summary>
/// Exceção lançada quando uma regra de negócio do domínio é violada
/// (ex: doador menor de idade, peso insuficiente, intervalo entre doações não respeitado, etc).
/// </summary>
public class RegraNegocioException : Exception
{
    public RegraNegocioException(string mensagem) : base(mensagem) { }
}
