namespace BancoSangue.Domain.Exceptions;

/// <summary>Exceção base para violações de invariantes do domínio.</summary>
public abstract class DomainException : Exception
{
    protected DomainException(string mensagem) : base(mensagem) { }
}

/// <summary>Regra de negócio violada (idade, peso, intervalo, quantidade, e-mail duplicado...).</summary>
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
