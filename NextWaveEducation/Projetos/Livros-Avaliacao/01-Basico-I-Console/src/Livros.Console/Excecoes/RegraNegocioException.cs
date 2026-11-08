namespace Livros.Console.Excecoes;

/// <summary>
/// Lançada quando uma regra de negócio do domínio é violada
/// (ISBN duplicado, nota fora de 1 a 5, datas de leitura invertidas, etc).
/// </summary>
public class RegraNegocioException : Exception
{
    public RegraNegocioException(string mensagem) : base(mensagem) { }
}
