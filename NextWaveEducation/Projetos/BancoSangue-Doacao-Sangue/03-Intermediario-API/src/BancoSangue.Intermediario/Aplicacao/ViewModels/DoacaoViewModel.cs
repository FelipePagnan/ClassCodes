using BancoSangue.Intermediario.Dominio.Entidades;

namespace BancoSangue.Intermediario.Aplicacao.ViewModels;

public class DoacaoViewModel
{
    public int Id { get; set; }
    public int DoadorId { get; set; }
    public DateTime DataDoacao { get; set; }
    public int QuantidadeML { get; set; }

    public static DoacaoViewModel De(Doacao doacao) => new()
    {
        Id = doacao.Id,
        DoadorId = doacao.DoadorId,
        DataDoacao = doacao.DataDoacao,
        QuantidadeML = doacao.QuantidadeML
    };
}

/// <summary>
/// Usada no relatório de doações dos últimos 30 dias, que exige os dados do doador.
/// </summary>
public class DoacaoComDoadorViewModel
{
    public int Id { get; set; }
    public DateTime DataDoacao { get; set; }
    public int QuantidadeML { get; set; }
    public int DoadorId { get; set; }
    public string NomeDoador { get; set; } = string.Empty;
    public string EmailDoador { get; set; } = string.Empty;
    public string TipoSanguineo { get; set; } = string.Empty;
    public string? CidadeDoador { get; set; }

    public static DoacaoComDoadorViewModel De(Doacao doacao) => new()
    {
        Id = doacao.Id,
        DataDoacao = doacao.DataDoacao,
        QuantidadeML = doacao.QuantidadeML,
        DoadorId = doacao.DoadorId,
        NomeDoador = doacao.Doador?.NomeCompleto ?? string.Empty,
        EmailDoador = doacao.Doador?.Email ?? string.Empty,
        TipoSanguineo = doacao.Doador?.TipoSanguineoCompleto ?? string.Empty,
        CidadeDoador = doacao.Doador?.Endereco?.Cidade
    };
}
