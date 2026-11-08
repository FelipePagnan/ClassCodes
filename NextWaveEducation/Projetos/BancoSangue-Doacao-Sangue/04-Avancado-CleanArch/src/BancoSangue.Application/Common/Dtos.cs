using BancoSangue.Domain.Entities;

namespace BancoSangue.Application.Common;

public record EnderecoDto(string Logradouro, string Cidade, string Estado, string Cep)
{
    public static EnderecoDto? De(Endereco? endereco) => endereco is null
        ? null
        : new EnderecoDto(endereco.Logradouro, endereco.Cidade, endereco.Estado, endereco.CEP);
}

public record DoadorDto(
    int Id,
    string NomeCompleto,
    string Email,
    DateTime DataNascimento,
    int Idade,
    string Genero,
    double Peso,
    string TipoSanguineo,
    bool AptoParaDoar,
    EnderecoDto? Endereco)
{
    public static DoadorDto De(Doador d) => new(
        d.Id,
        d.NomeCompleto,
        d.EmailValor,
        d.DataNascimento,
        d.Idade,
        d.Genero.ToString(),
        d.Peso,
        d.TipoSanguineoCompleto,
        d.EstaAptoParaDoar,
        EnderecoDto.De(d.Endereco));
}

public record DoacaoDto(int Id, int DoadorId, DateTime DataDoacao, int QuantidadeML)
{
    public static DoacaoDto De(Doacao d) => new(d.Id, d.DoadorId, d.DataDoacao, d.QuantidadeML);
}

public record DoadorDetalheDto(
    DoadorDto Doador,
    DateTime? UltimaDoacao,
    DateTime? ProximaDoacaoPermitida,
    int IntervaloMinimoDias,
    int TotalDoacoes,
    int TotalDoadoML,
    List<DoacaoDto> Doacoes)
{
    public static DoadorDetalheDto De(Doador d) => new(
        DoadorDto.De(d),
        d.UltimaDoacao,
        d.ProximaDoacaoPermitida,
        d.IntervaloMinimoDias,
        d.Doacoes.Count,
        d.Doacoes.Sum(x => x.QuantidadeML),
        d.Doacoes.OrderByDescending(x => x.DataDoacao).Select(DoacaoDto.De).ToList());
}

/// <summary>Usado no relatório de doações dos últimos 30 dias (inclui dados do doador).</summary>
public record DoacaoComDoadorDto(
    int Id,
    DateTime DataDoacao,
    int QuantidadeML,
    int DoadorId,
    string NomeDoador,
    string EmailDoador,
    string TipoSanguineo,
    int IdadeDoador,
    string? CidadeDoador)
{
    public static DoacaoComDoadorDto De(Doacao d) => new(
        d.Id,
        d.DataDoacao,
        d.QuantidadeML,
        d.DoadorId,
        d.Doador?.NomeCompleto ?? string.Empty,
        d.Doador?.EmailValor ?? string.Empty,
        d.Doador?.TipoSanguineoCompleto ?? string.Empty,
        d.Doador?.Idade ?? 0,
        d.Doador?.Endereco?.Cidade);
}

public record EstoqueDto(
    string TipoSanguineo,
    int QuantidadeML,
    int QuantidadeMinimaML,
    bool EstoqueBaixo,
    string? Alerta)
{
    public static EstoqueDto De(EstoqueSangue e) => new(
        e.Descricao,
        e.QuantidadeML,
        EstoqueSangue.QuantidadeMinimaML,
        e.EstaAbaixoDoMinimo,
        e.EstaAbaixoDoMinimo
            ? $"Estoque do tipo {e.Descricao} abaixo do mínimo de {EstoqueSangue.QuantidadeMinimaML}ml."
            : null);
}

public record RelatorioEstoqueDto(
    int TotalGeralML,
    int TiposComEstoqueBaixo,
    List<EstoqueDto> PorTipo);

public record ResultadoDoacaoDto(
    DoacaoDto Doacao,
    string NomeDoador,
    EstoqueDto EstoqueAtualizado,
    string? AlertaEstoque);
