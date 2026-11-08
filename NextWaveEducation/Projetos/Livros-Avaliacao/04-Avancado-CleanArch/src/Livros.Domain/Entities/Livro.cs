using Livros.Domain.Common;
using Livros.Domain.Enums;
using Livros.Domain.Events;
using Livros.Domain.Exceptions;
using Livros.Domain.ValueObjects;

namespace Livros.Domain.Entities;

/// <summary>
/// Raiz de agregação do livro. Controla as avaliações recebidas e
/// mantém a nota média consistente.
/// </summary>
public class Livro : BaseEntity
{
    /// <summary>Tamanho máximo aceito para a imagem de capa.</summary>
    public const int TamanhoMaximoCapaBytes = 5 * 1024 * 1024;

    /// <summary>Ano mínimo aceito para publicação (invenção da prensa móvel).</summary>
    public const int AnoMinimoPublicacao = 1450;

    private readonly List<Avaliacao> _avaliacoes = new();

    // Construtor sem parâmetros para o EF Core.
    private Livro() { }

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
        DefinirTitulo(titulo);
        DefinirIsbn(isbn);
        DefinirAutor(autor);
        DefinirDescricao(descricao);
        DefinirAnoDePublicacao(anoDePublicacao);
        DefinirQuantidadePaginas(quantidadePaginas);

        if (!Enum.IsDefined(genero))
            throw new RegraNegocioException($"O gênero '{genero}' é inválido.");

        Editora = editora?.Trim() ?? string.Empty;
        Genero = genero;
        DataCriacao = DateTime.Now;
        NotaMedia = 0;

        AdicionarEvento(new LivroCadastradoEvent(Id, Titulo, Autor, IsbnValor));
    }

    public string Titulo { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;

    /// <summary>ISBN persistido. Exposto como Value Object através de <see cref="Isbn"/>.</summary>
    public string IsbnValor { get; private set; } = string.Empty;

    /// <summary>Versão normalizada do ISBN, persistida para garantir unicidade.</summary>
    public string IsbnNormalizado { get; private set; } = string.Empty;

    public Isbn Isbn => ValueObjects.Isbn.Criar(IsbnValor);

    public string Autor { get; private set; } = string.Empty;
    public string Editora { get; private set; } = string.Empty;
    public GeneroLivroEnum Genero { get; private set; }
    public int AnoDePublicacao { get; private set; }
    public int QuantidadePaginas { get; private set; }

    /// <summary>
    /// Média das notas recebidas. Recalculada a cada avaliação cadastrada
    /// e persistida nesta coluna.
    /// </summary>
    public decimal NotaMedia { get; private set; }

    /// <summary>Imagem da capa armazenada como bytes.</summary>
    public byte[]? CapaLivro { get; private set; }

    public IReadOnlyCollection<Avaliacao> Avaliacoes => _avaliacoes.AsReadOnly();

    public bool TemCapa => CapaLivro is { Length: > 0 };

    public int TotalAvaliacoes => _avaliacoes.Count;

    /// <summary>Quantas avaliações o livro recebeu em cada nota de 1 a 5.</summary>
    public IReadOnlyDictionary<int, int> DistribuicaoDeNotas =>
        Enumerable.Range(Avaliacao.NotaMinima, Avaliacao.NotaMaxima)
            .ToDictionary(nota => nota, nota => _avaliacoes.Count(a => a.Nota == nota));

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
        DefinirTitulo(titulo);
        DefinirIsbn(isbn);
        DefinirAutor(autor);
        DefinirDescricao(descricao);
        DefinirAnoDePublicacao(anoDePublicacao);
        DefinirQuantidadePaginas(quantidadePaginas);

        if (!Enum.IsDefined(genero))
            throw new RegraNegocioException($"O gênero '{genero}' é inválido.");

        Editora = editora?.Trim() ?? string.Empty;
        Genero = genero;
    }

    /// <summary>Define a imagem de capa a partir dos bytes do arquivo enviado.</summary>
    public void DefinirCapa(byte[] bytes)
    {
        if (bytes is null || bytes.Length == 0)
            throw new RegraNegocioException("O arquivo de capa está vazio.");

        if (bytes.Length > TamanhoMaximoCapaBytes)
            throw new RegraNegocioException(
                $"A imagem de capa deve ter no máximo {TamanhoMaximoCapaBytes / 1024 / 1024}MB.");

        CapaLivro = bytes;
    }

    public void RemoverCapa() => CapaLivro = null;

    /// <summary>
    /// Adiciona uma avaliação aplicando as regras de nota e de período de
    /// leitura, e dispara o evento que leva ao recálculo da nota média.
    /// </summary>
    public Avaliacao AdicionarAvaliacao(
        int idUsuario,
        int nota,
        string descricao,
        DateTime dataInicioLeitura,
        DateTime dataFimLeitura)
    {
        if (_avaliacoes.Any(a => a.IdUsuario == idUsuario))
            throw new RegraNegocioException($"Este usuário já avaliou o livro '{Titulo}'.");

        // O Value Object aplica a regra "início não pode ser maior que fim".
        var periodo = PeriodoLeitura.Criar(dataInicioLeitura, dataFimLeitura);

        var avaliacao = new Avaliacao(Id, idUsuario, nota, descricao, periodo);
        _avaliacoes.Add(avaliacao);

        AdicionarEvento(new AvaliacaoCadastradaEvent(Id, Titulo, idUsuario, nota, periodo.Fim));

        return avaliacao;
    }

    /// <summary>
    /// Recalcula a nota média a partir das avaliações e dispara o evento
    /// de atualização quando o valor muda.
    /// </summary>
    public void RecalcularNotaMedia()
    {
        var anterior = NotaMedia;

        NotaMedia = _avaliacoes.Count == 0
            ? 0
            : Math.Round((decimal)_avaliacoes.Average(a => a.Nota), 2);

        if (NotaMedia != anterior)
        {
            AdicionarEvento(new NotaMediaAtualizadaEvent(
                Id, Titulo, anterior, NotaMedia, _avaliacoes.Count));
        }
    }

    private void DefinirTitulo(string titulo)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new RegraNegocioException("O título é obrigatório.");

        if (titulo.Trim().Length > 250)
            throw new RegraNegocioException("O título deve ter no máximo 250 caracteres.");

        Titulo = titulo.Trim();
    }

    private void DefinirIsbn(string isbn)
    {
        var vo = ValueObjects.Isbn.Criar(isbn);
        IsbnValor = vo.Valor;
        IsbnNormalizado = vo.Normalizado;
    }

    private void DefinirAutor(string autor)
    {
        if (string.IsNullOrWhiteSpace(autor))
            throw new RegraNegocioException("O autor é obrigatório.");

        if (autor.Trim().Length > 200)
            throw new RegraNegocioException("O autor deve ter no máximo 200 caracteres.");

        Autor = autor.Trim();
    }

    private void DefinirDescricao(string descricao)
    {
        if (descricao is { Length: > 4000 })
            throw new RegraNegocioException("A descrição deve ter no máximo 4000 caracteres.");

        Descricao = descricao?.Trim() ?? string.Empty;
    }

    private void DefinirAnoDePublicacao(int ano)
    {
        var anoMaximo = DateTime.Today.Year + 1;

        if (ano < AnoMinimoPublicacao || ano > anoMaximo)
            throw new RegraNegocioException(
                $"O ano de publicação deve estar entre {AnoMinimoPublicacao} e {anoMaximo}.");

        AnoDePublicacao = ano;
    }

    private void DefinirQuantidadePaginas(int paginas)
    {
        if (paginas <= 0)
            throw new RegraNegocioException("A quantidade de páginas deve ser maior que zero.");

        if (paginas > 50_000)
            throw new RegraNegocioException("A quantidade de páginas informada é inválida.");

        QuantidadePaginas = paginas;
    }
}
