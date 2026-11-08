using Clinica.Api.Dominio.Excecoes;
using Clinica.Api.Util;

namespace Clinica.Api.Dominio.ObjetosDeValor;

/// <summary>
/// Celular brasileiro como objeto de valor: DDD válido + 9 dígitos começando
/// em 9. É por esse número que o SMS de confirmação e de lembrete sai, então
/// vale garantir o formato na entrada em vez de descobrir o erro no envio.
/// </summary>
public record Celular
{
    private Celular(string valor) => Valor = valor;

    /// <summary>Somente os 11 dígitos.</summary>
    public string Valor { get; }

    /// <summary>Formatado para exibir: (11) 98765-4321.</summary>
    public string Formatado => Documentos.FormatarCelular(Valor);

    /// <summary>DDD, útil para relatório por região.</summary>
    public string Ddd => Valor.Length >= 2 ? Valor[..2] : string.Empty;

    public static Celular Criar(string? entrada)
    {
        if (string.IsNullOrWhiteSpace(entrada))
            throw new RegraNegocioException("O celular é obrigatório.");

        if (!Documentos.CelularValido(entrada))
            throw new RegraNegocioException(
                $"O celular '{entrada}' é inválido. Informe DDD + 9 dígitos, " +
                "ex: (11) 98765-4321.");

        return new Celular(Documentos.SomenteDigitos(entrada));
    }

    /// <summary>Versão opcional: devolve null quando não há número informado.</summary>
    public static Celular? CriarOpcional(string? entrada)
        => string.IsNullOrWhiteSpace(entrada) ? null : Criar(entrada);

    /// <summary>Reconstrói sem validar. Uso exclusivo do Entity Framework.</summary>
    public static Celular Reconstruir(string valor) => new(valor);

    public override string ToString() => Formatado;
}
