namespace Caixinhas.Intermediario.Dominio.Excecoes;

/// <summary>Regra de negócio violada.</summary>
public class RegraNegocioException : Exception
{
    public RegraNegocioException(string mensagem) : base(mensagem) { }
}

/// <summary>Recurso não encontrado.</summary>
public class NaoEncontradoException : Exception
{
    public NaoEncontradoException(string mensagem) : base(mensagem) { }

    public static NaoEncontradoException Para(string entidade, object chave)
        => new($"{entidade} com identificador '{chave}' não foi encontrado(a).");
}
