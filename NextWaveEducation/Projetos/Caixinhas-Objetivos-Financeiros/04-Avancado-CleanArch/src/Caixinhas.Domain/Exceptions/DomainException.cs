namespace Caixinhas.Domain.Exceptions;

/// <summary>Exceção base para violações de invariantes do domínio.</summary>
public abstract class DomainException : Exception
{
    protected DomainException(string mensagem) : base(mensagem) { }
}

/// <summary>Regra de negócio violada (valor negativo, casas decimais, saldo...).</summary>
public class RegraNegocioException : DomainException
{
    public RegraNegocioException(string mensagem) : base(mensagem) { }
}

/// <summary>Recurso não encontrado.</summary>
public class NaoEncontradoException : DomainException
{
    public NaoEncontradoException(string mensagem) : base(mensagem) { }

    public static NaoEncontradoException Para(string entidade, object chave)
        => new($"{entidade} com identificador '{chave}' não foi encontrado(a).");
}
