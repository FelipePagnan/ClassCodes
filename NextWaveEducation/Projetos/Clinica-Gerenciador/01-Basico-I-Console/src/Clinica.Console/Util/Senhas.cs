using System.Security.Cryptography;

namespace Clinica.Console.Util;

/// <summary>
/// Hash de senha com PBKDF2 (o mesmo algoritmo que o ASP.NET Identity usa
/// por baixo), sem dependência externa. Cada senha recebe um salt próprio,
/// então duas senhas iguais geram hashes diferentes.
/// </summary>
public static class Senhas
{
    private const int TamanhoSaltBytes = 16;
    private const int TamanhoHashBytes = 32;
    private const int Iteracoes = 100_000;

    private static readonly HashAlgorithmName _algoritmo = HashAlgorithmName.SHA256;

    public static (string Hash, string Salt) Gerar(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha))
            throw new ArgumentException("A senha não pode ser vazia.", nameof(senha));

        var salt = RandomNumberGenerator.GetBytes(TamanhoSaltBytes);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            senha, salt, Iteracoes, _algoritmo, TamanhoHashBytes);

        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    /// <summary>
    /// Confere a senha. A comparação é feita em tempo fixo para não vazar
    /// informação por quanto tempo a verificação leva.
    /// </summary>
    public static bool Conferir(string senha, string hashArmazenado, string saltArmazenado)
    {
        if (string.IsNullOrWhiteSpace(senha)
            || string.IsNullOrWhiteSpace(hashArmazenado)
            || string.IsNullOrWhiteSpace(saltArmazenado))
            return false;

        try
        {
            var salt = Convert.FromBase64String(saltArmazenado);
            var esperado = Convert.FromBase64String(hashArmazenado);

            var calculado = Rfc2898DeriveBytes.Pbkdf2(
                senha, salt, Iteracoes, _algoritmo, esperado.Length);

            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    /// Exigências mínimas de senha: 8 caracteres, com letra e número.
    /// </summary>
    public static bool SenhaForte(string? senha, out string motivo)
    {
        motivo = string.Empty;

        if (string.IsNullOrWhiteSpace(senha) || senha.Length < 8)
        {
            motivo = "A senha deve ter ao menos 8 caracteres.";
            return false;
        }

        if (!senha.Any(char.IsLetter))
        {
            motivo = "A senha deve conter ao menos uma letra.";
            return false;
        }

        if (!senha.Any(char.IsDigit))
        {
            motivo = "A senha deve conter ao menos um número.";
            return false;
        }

        return true;
    }
}
