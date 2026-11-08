namespace Caixinhas.Console.Excecoes;

/// <summary>
/// Lançada quando uma regra de negócio é violada (valor negativo, mais de
/// duas casas decimais, saldo insuficiente, objetivo cancelado, etc).
/// </summary>
public class RegraNegocioException : Exception
{
    public RegraNegocioException(string mensagem) : base(mensagem) { }
}
