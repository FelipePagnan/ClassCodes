using System.Text.RegularExpressions;
using Clinica.Api.Dominio.Excecoes;

namespace Clinica.Api.Dominio.ObjetosDeValor;

/// <summary>
/// E-mail como objeto de valor, sempre normalizado em minúsculas — assim
/// "Maria@Exemplo.com" e "maria@exemplo.com" são a mesma coisa, o que evita
/// cadastro duplicado por diferença de caixa.
/// </summary>
public partial record Email
{
    private Email(string valor) => Valor = valor;

    public string Valor { get; }

    public static Email Criar(string? entrada)
    {
        if (string.IsNullOrWhiteSpace(entrada))
            throw new RegraNegocioException("O e-mail é obrigatório.");

        var normalizado = entrada.Trim().ToLowerInvariant();

        if (!Padrao().IsMatch(normalizado))
            throw new RegraNegocioException($"O e-mail '{entrada}' é inválido.");

        if (normalizado.Length > 200)
            throw new RegraNegocioException("O e-mail deve ter no máximo 200 caracteres.");

        return new Email(normalizado);
    }

    public static Email? CriarOpcional(string? entrada)
        => string.IsNullOrWhiteSpace(entrada) ? null : Criar(entrada);

    /// <summary>Reconstrói sem validar. Uso exclusivo do Entity Framework.</summary>
    public static Email Reconstruir(string valor) => new(valor);

    public static bool EhValido(string? entrada)
        => !string.IsNullOrWhiteSpace(entrada)
           && Padrao().IsMatch(entrada.Trim().ToLowerInvariant());

    /// <summary>
    /// Regex compilada em tempo de build pelo gerador de código do .NET,
    /// então não há custo de montar o autômato a cada chamada.
    /// </summary>
    [GeneratedRegex(@"^[^@\s]+@[^@\s.]+(\.[^@\s.]+)+$", RegexOptions.CultureInvariant)]
    private static partial Regex Padrao();

    public override string ToString() => Valor;
}
