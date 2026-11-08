using Livros.Domain.Entities;

namespace Livros.Application.Common;

// ------------------------------- LIVROS -------------------------------

public record LivroDto(
    int Id,
    string Titulo,
    string Autor,
    string Editora,
    string Isbn,
    string Genero,
    int AnoDePublicacao,
    int QuantidadePaginas,
    decimal NotaMedia,
    int TotalAvaliacoes,
    bool TemCapa,
    DateTime DataCriacao)
{
    public static LivroDto De(Livro l) => new(
        l.Id,
        l.Titulo,
        l.Autor,
        l.Editora,
        l.IsbnValor,
        l.Genero.ToString(),
        l.AnoDePublicacao,
        l.QuantidadePaginas,
        l.NotaMedia,
        l.TotalAvaliacoes,
        l.TemCapa,
        l.DataCriacao);
}

public record LivroDetalheDto(
    LivroDto Livro,
    string Descricao,
    IReadOnlyDictionary<int, int> DistribuicaoDeNotas,
    List<AvaliacaoDto> Avaliacoes)
{
    public static LivroDetalheDto De(Livro l) => new(
        LivroDto.De(l),
        l.Descricao,
        l.DistribuicaoDeNotas,
        l.Avaliacoes
            .OrderByDescending(a => a.DataCriacao)
            .Select(AvaliacaoDto.De)
            .ToList());
}

// ------------------------------ USUÁRIOS ------------------------------

public record UsuarioDto(
    int Id,
    string Nome,
    string Email,
    int TotalAvaliacoes,
    double? NotaMediaAtribuida,
    DateTime DataCriacao)
{
    public static UsuarioDto De(Usuario u) => new(
        u.Id,
        u.Nome,
        u.EmailValor,
        u.TotalAvaliacoes,
        u.NotaMediaAtribuida,
        u.DataCriacao);
}

public record UsuarioDetalheDto(
    UsuarioDto Usuario,
    int TotalPaginasLidas,
    int LivrosLidosNoAnoCorrente,
    List<AvaliacaoDoUsuarioDto> Avaliacoes)
{
    public static UsuarioDetalheDto De(Usuario u) => new(
        UsuarioDto.De(u),
        u.TotalPaginasLidas,
        u.LivrosLidosNoAno(DateTime.Today.Year),
        u.Avaliacoes
            .OrderByDescending(a => a.DataFimLeitura)
            .Select(AvaliacaoDoUsuarioDto.De)
            .ToList());
}

// ----------------------------- AVALIAÇÕES -----------------------------

public record AvaliacaoDto(
    int Id,
    int Nota,
    string Descricao,
    int IdUsuario,
    string? NomeUsuario,
    DateTime DataInicioLeitura,
    DateTime DataFimLeitura,
    int DiasDeLeitura,
    DateTime DataCriacao)
{
    public static AvaliacaoDto De(Avaliacao a) => new(
        a.Id,
        a.Nota,
        a.Descricao,
        a.IdUsuario,
        a.Usuario?.Nome,
        a.DataInicioLeitura,
        a.DataFimLeitura,
        a.DiasDeLeitura,
        a.DataCriacao);
}

public record AvaliacaoDoUsuarioDto(
    int Id,
    int Nota,
    string Descricao,
    int IdLivro,
    string? TituloLivro,
    string? AutorLivro,
    DateTime DataInicioLeitura,
    DateTime DataFimLeitura,
    int DiasDeLeitura,
    double? PaginasPorDia)
{
    public static AvaliacaoDoUsuarioDto De(Avaliacao a) => new(
        a.Id,
        a.Nota,
        a.Descricao,
        a.IdLivro,
        a.Livro?.Titulo,
        a.Livro?.Autor,
        a.DataInicioLeitura,
        a.DataFimLeitura,
        a.DiasDeLeitura,
        a.PaginasPorDia);
}

/// <summary>
/// Resultado do cadastro de avaliação, já com a nota média do livro
/// recalculada pelo handler do evento de domínio.
/// </summary>
public record ResultadoAvaliacaoDto(
    AvaliacaoDto Avaliacao,
    string TituloLivro,
    decimal NotaMediaAtualizada,
    int TotalAvaliacoesDoLivro);

// ----------------------------- RELATÓRIOS -----------------------------

public record ItemRelatorioAnualDto(
    int IdLivro,
    string Titulo,
    string Autor,
    string Genero,
    int QuantidadePaginas,
    int QuantidadeLeituras,
    double NotaMedia);

public record RelatorioLivrosLidosDto(
    int Ano,
    int LivrosDistintosLidos,
    int TotalDeLeituras,
    int PaginasLidas,
    int LeitoresDistintos,
    double? NotaMediaGeral,
    double? MediaDiasPorLeitura,
    List<ItemRelatorioAnualDto> Livros,
    Dictionary<string, int> LeiturasPorMes);

// -------------------------- CONSULTA EXTERNA --------------------------

public record LivroExternoResultadoDto(
    string Titulo,
    string Descricao,
    string Isbn,
    string Autor,
    string Editora,
    string? Categoria,
    int AnoDePublicacao,
    int QuantidadePaginas,
    bool JaCadastrado);
