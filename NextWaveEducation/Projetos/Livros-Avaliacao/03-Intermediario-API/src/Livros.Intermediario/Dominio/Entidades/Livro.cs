using Livros.Intermediario.Dominio.Enums;
using Livros.Intermediario.Dominio.Excecoes;

namespace Livros.Intermediario.Dominio.Entidades;

public class Livro : EntidadeBase
{
    /// <summary>Tamanho máximo aceito para a imagem de capa.</summary>
    public const int TamanhoMaximoCapaBytes = 5 * 1024 * 1024;

    protected Livro() { }

    public Livro(
        string titulo,
        string descricao,
        string isbn,
        string autor,
        string editora,
        GeneroLivroEnum genero,
        int anoDePublicacao,
        int quantidadePaginas)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new RegraNegocioException("O título é obrigatório.");

        if (!IsbnValido(isbn))
            throw new RegraNegocioException(
                $"O ISBN '{isbn}' é inválido. Informe 10 ou 13 dígitos.");

        if (string.IsNullOrWhiteSpace(autor))
            throw new RegraNegocioException("O autor é obrigatório.");

        if (quantidadePaginas <= 0)
            throw new RegraNegocioException("A quantidade de páginas deve ser maior que zero.");

        Titulo = titulo.Trim();
        Descricao = descricao?.Trim() ?? string.Empty;
        ISBN = isbn.Trim();
        Autor = autor.Trim();
        Editora = editora?.Trim() ?? string.Empty;
        Genero = genero;
        AnoDePublicacao = anoDePublicacao;
        QuantidadePaginas = quantidadePaginas;
        DataCriacao = DateTime.Now;
        NotaMedia = 0;
        Avaliacoes = new List<Avaliacao>();
    }

    public string Titulo { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public string ISBN { get; private set; } = string.Empty;
    public string Autor { get; private set; } = string.Empty;
    public string Editora { get; private set; } = string.Empty;
    public GeneroLivroEnum Genero { get; private set; }
    public int AnoDePublicacao { get; private set; }
    public int QuantidadePaginas { get; private set; }

    /// <summary>Média das notas, recalculada a cada avaliação cadastrada.</summary>
    public decimal NotaMedia { get; private set; }

    /// <summary>Imagem da capa armazenada como bytes.</summary>
    public byte[]? CapaLivro { get; private set; }

    public List<Avaliacao> Avaliacoes { get; private set; } = new();

    public bool TemCapa => CapaLivro is { Length: > 0 };

    /// <summary>
    /// Normaliza o ISBN (sem hífens, maiúsculo) para comparação de duplicidade.
    /// </summary>
    public string IsbnNormalizado => NormalizarIsbn(ISBN);

    public void Atualizar(
        string titulo,
        string descricao,
        string isbn,
        string autor,
        string editora,
        GeneroLivroEnum genero,
        int anoDePublicacao,
        int quantidadePaginas)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new RegraNegocioException("O título é obrigatório.");

        if (!IsbnValido(isbn))
            throw new RegraNegocioException($"O ISBN '{isbn}' é inválido. Informe 10 ou 13 dígitos.");

        if (string.IsNullOrWhiteSpace(autor))
            throw new RegraNegocioException("O autor é obrigatório.");

        if (quantidadePaginas <= 0)
            throw new RegraNegocioException("A quantidade de páginas deve ser maior que zero.");

        Titulo = titulo.Trim();
        Descricao = descricao?.Trim() ?? string.Empty;
        ISBN = isbn.Trim();
        Autor = autor.Trim();
        Editora = editora?.Trim() ?? string.Empty;
        Genero = genero;
        AnoDePublicacao = anoDePublicacao;
        QuantidadePaginas = quantidadePaginas;
    }

    /// <summary>
    /// Define a imagem de capa a partir dos bytes do arquivo enviado.
    /// </summary>
    public void DefinirCapa(byte[] bytes)
    {
        if (bytes is null || bytes.Length == 0)
            throw new RegraNegocioException("O arquivo de capa está vazio.");

        if (bytes.Length > TamanhoMaximoCapaBytes)
            throw new RegraNegocioException(
                $"A imagem de capa deve ter no máximo {TamanhoMaximoCapaBytes / 1024 / 1024}MB.");

        CapaLivro = bytes;
    }

    /// <summary>
    /// Adiciona uma avaliação e recalcula a nota média do livro.
    /// </summary>
    public Avaliacao AdicionarAvaliacao(
        int idUsuario,
        int nota,
        string descricao,
        DateTime dataInicioLeitura,
        DateTime dataFimLeitura)
    {
        if (Avaliacoes.Any(a => a.IdUsuario == idUsuario))
            throw new RegraNegocioException($"Este usuário já avaliou o livro '{Titulo}'.");

        var avaliacao = new Avaliacao(Id, idUsuario, nota, descricao, dataInicioLeitura, dataFimLeitura);
        Avaliacoes.Add(avaliacao);
        RecalcularNotaMedia();

        return avaliacao;
    }

    /// <summary>
    /// Recalcula a nota média a partir das avaliações cadastradas.
    /// </summary>
    public void RecalcularNotaMedia()
    {
        NotaMedia = Avaliacoes.Count == 0
            ? 0
            : Math.Round((decimal)Avaliacoes.Average(a => a.Nota), 2);
    }

    public static bool IsbnValido(string? isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn)) return false;
        var digitos = NormalizarIsbn(isbn);
        return digitos.Length is 10 or 13 && digitos.All(c => char.IsDigit(c) || c == 'X');
    }

    public static string NormalizarIsbn(string isbn)
        => new(isbn.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());
}
