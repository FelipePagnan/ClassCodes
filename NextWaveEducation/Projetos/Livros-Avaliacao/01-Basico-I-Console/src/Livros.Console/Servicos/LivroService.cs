using Livros.Console.Entidades;
using Livros.Console.Enums;
using Livros.Console.Excecoes;

namespace Livros.Console.Servicos;

/// <summary>
/// Gerenciamento de livros: cadastro, atualização, remoção, listagem e detalhes.
/// </summary>
public class LivroService
{
    private readonly List<Livro> _livros = new();
    private int _proximoId = 1;

    public IReadOnlyList<Livro> ListarTodos() => _livros.AsReadOnly();

    public Livro? BuscarPorId(int id) => _livros.FirstOrDefault(l => l.Id == id);

    public Livro ObterPorId(int id)
        => BuscarPorId(id) ?? throw new RegraNegocioException($"Livro com id {id} não encontrado.");

    public Livro? BuscarPorIsbn(string isbn)
    {
        var normalizado = NormalizarIsbn(isbn);
        return _livros.FirstOrDefault(l => NormalizarIsbn(l.ISBN) == normalizado);
    }

    public IEnumerable<Livro> Pesquisar(string termo)
    {
        if (string.IsNullOrWhiteSpace(termo)) return ListarTodos();

        return _livros.Where(l =>
            l.Titulo.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
            l.Autor.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
            l.Editora.Contains(termo, StringComparison.OrdinalIgnoreCase));
    }

    public Livro Cadastrar(Livro livro)
    {
        ValidarDados(livro);

        // Regra de negócio: livros não podem ser cadastrados com o mesmo ISBN.
        if (BuscarPorIsbn(livro.ISBN) is not null)
            throw new RegraNegocioException($"Já existe um livro cadastrado com o ISBN '{livro.ISBN}'.");

        livro.Id = _proximoId++;
        livro.DataCriacao = DateTime.Now;
        livro.RecalcularNotaMedia();

        _livros.Add(livro);
        return livro;
    }

    public Livro Atualizar(int id, Livro dados)
    {
        var livro = ObterPorId(id);
        ValidarDados(dados);

        // Outro livro já usa esse ISBN?
        var existente = BuscarPorIsbn(dados.ISBN);
        if (existente is not null && existente.Id != id)
            throw new RegraNegocioException($"Já existe outro livro cadastrado com o ISBN '{dados.ISBN}'.");

        livro.Titulo = dados.Titulo;
        livro.Descricao = dados.Descricao;
        livro.ISBN = dados.ISBN;
        livro.Autor = dados.Autor;
        livro.Editora = dados.Editora;
        livro.Genero = dados.Genero;
        livro.AnoDePublicacao = dados.AnoDePublicacao;
        livro.QuantidadePaginas = dados.QuantidadePaginas;

        return livro;
    }

    public void Remover(int id)
    {
        var livro = ObterPorId(id);
        _livros.Remove(livro);
    }

    /// <summary>
    /// Define a imagem de capa do livro a partir de um arquivo em disco.
    /// </summary>
    public Livro DefinirCapa(int id, string caminhoArquivo)
    {
        var livro = ObterPorId(id);

        if (!File.Exists(caminhoArquivo))
            throw new RegraNegocioException($"Arquivo não encontrado: {caminhoArquivo}");

        var extensao = Path.GetExtension(caminhoArquivo).ToLowerInvariant();
        string[] permitidas = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"];
        if (!permitidas.Contains(extensao))
            throw new RegraNegocioException(
                $"Extensão '{extensao}' não permitida. Use: {string.Join(", ", permitidas)}");

        var bytes = File.ReadAllBytes(caminhoArquivo);

        const int tamanhoMaximoBytes = 5 * 1024 * 1024;
        if (bytes.Length > tamanhoMaximoBytes)
            throw new RegraNegocioException("A imagem de capa deve ter no máximo 5MB.");

        livro.CapaLivro = bytes;
        return livro;
    }

    /// <summary>
    /// Relatório da quantidade de livros lidos num determinado ano,
    /// com base na data de fim de leitura das avaliações.
    /// </summary>
    public RelatorioAnual GerarRelatorioAnual(int ano)
    {
        var avaliacoesDoAno = _livros
            .SelectMany(l => l.Avaliacoes.Select(a => new { Livro = l, Avaliacao = a }))
            .Where(x => x.Avaliacao.DataFimLeitura.Year == ano)
            .ToList();

        var itens = avaliacoesDoAno
            .GroupBy(x => x.Livro.Id)
            .Select(g => new ItemRelatorioAnual(
                g.First().Livro.Titulo,
                g.First().Livro.Autor,
                g.First().Livro.QuantidadePaginas,
                g.Count(),
                Math.Round(g.Average(x => (double)x.Avaliacao.Nota), 2)))
            .OrderByDescending(i => i.NotaMedia)
            .ToList();

        return new RelatorioAnual(
            ano,
            itens.Count,
            avaliacoesDoAno.Count,
            itens.Sum(i => i.QuantidadePaginas),
            itens);
    }

    private static void ValidarDados(Livro livro)
    {
        if (string.IsNullOrWhiteSpace(livro.Titulo))
            throw new RegraNegocioException("O título é obrigatório.");

        if (livro.Titulo.Trim().Length > 250)
            throw new RegraNegocioException("O título deve ter no máximo 250 caracteres.");

        if (string.IsNullOrWhiteSpace(livro.ISBN))
            throw new RegraNegocioException("O ISBN é obrigatório.");

        if (!IsbnValido(livro.ISBN))
            throw new RegraNegocioException(
                $"O ISBN '{livro.ISBN}' é inválido. Informe 10 ou 13 dígitos (hífens são aceitos).");

        if (string.IsNullOrWhiteSpace(livro.Autor))
            throw new RegraNegocioException("O autor é obrigatório.");

        if (!Enum.IsDefined(livro.Genero))
            throw new RegraNegocioException("O gênero informado é inválido.");

        if (livro.AnoDePublicacao < 1450 || livro.AnoDePublicacao > DateTime.Today.Year + 1)
            throw new RegraNegocioException(
                $"O ano de publicação deve estar entre 1450 e {DateTime.Today.Year + 1}.");

        if (livro.QuantidadePaginas <= 0)
            throw new RegraNegocioException("A quantidade de páginas deve ser maior que zero.");

        if (livro.QuantidadePaginas > 50_000)
            throw new RegraNegocioException("A quantidade de páginas informada é inválida.");
    }

    private static bool IsbnValido(string isbn)
    {
        var digitos = NormalizarIsbn(isbn);
        return digitos.Length is 10 or 13 && digitos.All(c => char.IsDigit(c) || c is 'X');
    }

    private static string NormalizarIsbn(string isbn)
        => new(isbn.Where(c => char.IsLetterOrDigit(c)).Select(char.ToUpperInvariant).ToArray());
}

public record ItemRelatorioAnual(
    string Titulo,
    string Autor,
    int QuantidadePaginas,
    int QuantidadeLeituras,
    double NotaMedia);

public record RelatorioAnual(
    int Ano,
    int LivrosLidos,
    int TotalLeituras,
    int PaginasLidas,
    List<ItemRelatorioAnual> Itens);
