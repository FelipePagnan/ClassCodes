using Livros.Api.Enums;

namespace Livros.Api.Dtos;

// ------------------------------- LIVROS -------------------------------

public record CriarLivroDto(
    string Titulo,
    string Descricao,
    string ISBN,
    string Autor,
    string Editora,
    GeneroLivroEnum Genero,
    int AnoDePublicacao,
    int QuantidadePaginas);

public record AtualizarLivroDto(
    string Titulo,
    string Descricao,
    string ISBN,
    string Autor,
    string Editora,
    GeneroLivroEnum Genero,
    int AnoDePublicacao,
    int QuantidadePaginas);

public record LivroDto(
    int Id,
    string Titulo,
    string Autor,
    string Editora,
    string ISBN,
    GeneroLivroEnum Genero,
    int AnoDePublicacao,
    int QuantidadePaginas,
    decimal NotaMedia,
    int TotalAvaliacoes,
    bool TemCapa,
    DateTime DataCriacao);

public record LivroDetalheDto(
    int Id,
    string Titulo,
    string Descricao,
    string Autor,
    string Editora,
    string ISBN,
    GeneroLivroEnum Genero,
    int AnoDePublicacao,
    int QuantidadePaginas,
    decimal NotaMedia,
    bool TemCapa,
    DateTime DataCriacao,
    List<AvaliacaoDto> Avaliacoes);

// ------------------------------ USUÁRIOS ------------------------------

public record CriarUsuarioDto(string Nome, string Email);

public record AtualizarUsuarioDto(string Nome, string Email);

public record UsuarioDto(
    int Id,
    string Nome,
    string Email,
    int TotalAvaliacoes,
    double? NotaMediaAtribuida,
    DateTime DataCriacao);

public record UsuarioDetalheDto(
    int Id,
    string Nome,
    string Email,
    int TotalAvaliacoes,
    double? NotaMediaAtribuida,
    DateTime DataCriacao,
    List<AvaliacaoDoUsuarioDto> Avaliacoes);

// ----------------------------- AVALIAÇÕES -----------------------------

public record CriarAvaliacaoDto(
    int IdUsuario,
    int Nota,
    string Descricao,
    DateTime DataInicioLeitura,
    DateTime DataFimLeitura);

public record AvaliacaoDto(
    int Id,
    int Nota,
    string Descricao,
    int IdUsuario,
    string? NomeUsuario,
    DateTime DataInicioLeitura,
    DateTime DataFimLeitura,
    int DiasDeLeitura,
    DateTime DataCriacao);

public record AvaliacaoDoUsuarioDto(
    int Id,
    int Nota,
    string Descricao,
    int IdLivro,
    string? TituloLivro,
    DateTime DataInicioLeitura,
    DateTime DataFimLeitura,
    DateTime DataCriacao);

// ------------------------------- OUTROS -------------------------------

public record ErroDto(string Mensagem);
