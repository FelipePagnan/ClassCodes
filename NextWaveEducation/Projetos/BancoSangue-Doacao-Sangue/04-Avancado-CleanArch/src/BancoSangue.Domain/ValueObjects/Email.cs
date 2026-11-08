using System.Text.RegularExpressions;
using BancoSangue.Domain.Exceptions;

namespace BancoSangue.Domain.ValueObjects;

/// <summary>
/// Value Object que garante que um e-mail só existe no domínio se for válido.
/// </summary>
public sealed record Email
{
    private static readonly Regex _padrao = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private Email(string valor) => Valor = valor;

    public string Valor { get; }

    public static Email Criar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new RegraNegocioException("O e-mail é obrigatório.");

        var normalizado = valor.Trim().ToLowerInvariant();

        if (normalizado.Length > 150)
            throw new RegraNegocioException("O e-mail deve ter no máximo 150 caracteres.");

        if (!_padrao.IsMatch(normalizado))
            throw new RegraNegocioException($"O e-mail '{valor}' não é válido.");

        return new Email(normalizado);
    }

    public override string ToString() => Valor;

    public static implicit operator string(Email email) => email.Valor;
}
