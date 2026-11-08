using Livros.Domain.Exceptions;

namespace Livros.Domain.ValueObjects;

/// <summary>
/// Value Object do ISBN. Guarda o valor como informado e expõe a versão
/// normalizada (sem hífens, maiúscula) usada para detectar duplicidade.
/// </summary>
public sealed record Isbn
{
    private Isbn(string valor, string normalizado)
    {
        Valor = valor;
        Normalizado = normalizado;
    }

    /// <summary>ISBN como foi informado, com hífens.</summary>
    public string Valor { get; }

    /// <summary>ISBN apenas com letras e dígitos, em maiúsculas.</summary>
    public string Normalizado { get; }

    public static Isbn Criar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new RegraNegocioException("O ISBN é obrigatório.");

        var normalizado = Normalizar(valor);

        if (normalizado.Length is not (10 or 13))
            throw new RegraNegocioException(
                $"O ISBN '{valor}' é inválido: deve ter 10 ou 13 dígitos (hífens são aceitos).");

        // Num ISBN-10 apenas o último caractere pode ser 'X'.
        if (!normalizado.All(c => char.IsDigit(c) || c == 'X'))
            throw new RegraNegocioException($"O ISBN '{valor}' contém caracteres inválidos.");

        if (normalizado.Length == 13 && normalizado.Contains('X'))
            throw new RegraNegocioException($"Um ISBN-13 não pode conter a letra 'X': '{valor}'.");

        if (normalizado.Length == 10 && normalizado[..9].Contains('X'))
            throw new RegraNegocioException(
                $"Num ISBN-10 a letra 'X' só é aceita como último dígito: '{valor}'.");

        return new Isbn(valor.Trim(), normalizado);
    }

    public static bool EhValido(string? valor)
    {
        try
        {
            Criar(valor);
            return true;
        }
        catch (RegraNegocioException)
        {
            return false;
        }
    }

    public static string Normalizar(string valor)
        => new(valor.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());

    public override string ToString() => Valor;
}
