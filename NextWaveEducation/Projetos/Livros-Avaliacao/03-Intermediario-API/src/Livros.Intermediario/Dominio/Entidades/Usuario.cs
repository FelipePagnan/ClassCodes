using System.Text.RegularExpressions;
using Livros.Intermediario.Dominio.Excecoes;

namespace Livros.Intermediario.Dominio.Entidades;

public class Usuario : EntidadeBase
{
    private static readonly Regex _padraoEmail = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    protected Usuario() { }

    public Usuario(string nome, string email)
    {
        DefinirNome(nome);
        DefinirEmail(email);
        DataCriacao = DateTime.Now;
        Avaliacoes = new List<Avaliacao>();
    }

    public string Email { get; private set; } = string.Empty;
    public string Nome { get; private set; } = string.Empty;

    public List<Avaliacao> Avaliacoes { get; private set; } = new();

    public int TotalAvaliacoes => Avaliacoes.Count;

    public double? NotaMediaAtribuida => Avaliacoes.Count == 0
        ? null
        : Math.Round(Avaliacoes.Average(a => a.Nota), 2);

    public void Atualizar(string nome, string email)
    {
        DefinirNome(nome);
        DefinirEmail(email);
    }

    /// <summary>
    /// Garante que o usuário pode ser removido (não possui avaliações).
    /// </summary>
    public void GarantirQuePodeSerRemovido()
    {
        if (Avaliacoes.Count > 0)
            throw new RegraNegocioException(
                $"'{Nome}' possui {Avaliacoes.Count} avaliação(ões) e não pode ser removido.");
    }

    private void DefinirNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length < 3)
            throw new RegraNegocioException("O nome é obrigatório e deve ter ao menos 3 caracteres.");

        Nome = nome.Trim();
    }

    private void DefinirEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new RegraNegocioException("O e-mail é obrigatório.");

        var normalizado = email.Trim().ToLowerInvariant();

        if (!_padraoEmail.IsMatch(normalizado))
            throw new RegraNegocioException($"O e-mail '{email}' é inválido.");

        Email = normalizado;
    }
}
