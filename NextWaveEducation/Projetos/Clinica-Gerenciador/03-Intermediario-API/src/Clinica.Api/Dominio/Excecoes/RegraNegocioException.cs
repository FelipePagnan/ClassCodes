namespace Clinica.Api.Dominio.Excecoes;

/// <summary>
/// Lançada quando uma regra de negócio é violada (CPF inválido, conflito de
/// agenda, atendimento já realizado, etc).
/// </summary>
public class RegraNegocioException : Exception
{
    public RegraNegocioException(string mensagem) : base(mensagem) { }
}
