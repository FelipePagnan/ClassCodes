using BancoSangue.Api.Enums;

namespace BancoSangue.Api.Dtos;

public record CriarDoadorDto(
    string NomeCompleto,
    string Email,
    DateTime DataNascimento,
    Genero Genero,
    double Peso,
    TipoSanguineo TipoSanguineo,
    FatorRh FatorRh,
    EnderecoDto? Endereco);

public record EnderecoDto(string Logradouro, string Cidade, string Estado, string CEP);

public record DoadorDto(
    int Id,
    string NomeCompleto,
    string Email,
    DateTime DataNascimento,
    int Idade,
    Genero Genero,
    double Peso,
    TipoSanguineo TipoSanguineo,
    FatorRh FatorRh,
    EnderecoDto? Endereco);

public record CriarDoacaoDto(int DoadorId, int QuantidadeML, DateTime? DataDoacao);

public record DoacaoDto(int Id, int DoadorId, string? NomeDoador, DateTime DataDoacao, int QuantidadeML);

public record EstoqueDto(TipoSanguineo TipoSanguineo, FatorRh FatorRh, int QuantidadeML, bool EstoqueBaixo);

public record ErroDto(string Mensagem);
