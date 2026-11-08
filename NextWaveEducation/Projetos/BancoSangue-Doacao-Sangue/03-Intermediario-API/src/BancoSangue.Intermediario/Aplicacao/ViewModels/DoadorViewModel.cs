using BancoSangue.Intermediario.Dominio.Entidades;

namespace BancoSangue.Intermediario.Aplicacao.ViewModels;

public class DoadorViewModel
{
    public int Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public int Idade { get; set; }
    public string Genero { get; set; } = string.Empty;
    public double Peso { get; set; }
    public string TipoSanguineo { get; set; } = string.Empty;
    public bool AptoParaDoar { get; set; }
    public EnderecoViewModel? Endereco { get; set; }

    public static DoadorViewModel De(Doador doador) => new()
    {
        Id = doador.Id,
        NomeCompleto = doador.NomeCompleto,
        Email = doador.Email,
        DataNascimento = doador.DataNascimento,
        Idade = doador.Idade,
        Genero = doador.Genero.ToString(),
        Peso = doador.Peso,
        TipoSanguineo = doador.TipoSanguineoCompleto,
        AptoParaDoar = doador.EhMaiorDeIdade && doador.Peso >= Doador.PesoMinimoKg,
        Endereco = doador.Endereco is null ? null : EnderecoViewModel.De(doador.Endereco)
    };
}

public class EnderecoViewModel
{
    public string Logradouro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string CEP { get; set; } = string.Empty;

    public static EnderecoViewModel De(Endereco endereco) => new()
    {
        Logradouro = endereco.Logradouro,
        Cidade = endereco.Cidade,
        Estado = endereco.Estado,
        CEP = endereco.CEP
    };
}

public class DoadorDetalheViewModel : DoadorViewModel
{
    public List<DoacaoViewModel> Doacoes { get; set; } = new();
    public DateTime? UltimaDoacao { get; set; }
    public int IntervaloMinimoDias { get; set; }
    public DateTime? ProximaDoacaoPermitida { get; set; }

    public static DoadorDetalheViewModel DeDetalhe(Doador doador)
    {
        var baseVm = De(doador);
        return new DoadorDetalheViewModel
        {
            Id = baseVm.Id,
            NomeCompleto = baseVm.NomeCompleto,
            Email = baseVm.Email,
            DataNascimento = baseVm.DataNascimento,
            Idade = baseVm.Idade,
            Genero = baseVm.Genero,
            Peso = baseVm.Peso,
            TipoSanguineo = baseVm.TipoSanguineo,
            AptoParaDoar = baseVm.AptoParaDoar,
            Endereco = baseVm.Endereco,
            Doacoes = doador.Doacoes
                .OrderByDescending(d => d.DataDoacao)
                .Select(DoacaoViewModel.De)
                .ToList(),
            UltimaDoacao = doador.UltimaDoacao,
            IntervaloMinimoDias = doador.IntervaloMinimoDias,
            ProximaDoacaoPermitida = doador.UltimaDoacao?.AddDays(doador.IntervaloMinimoDias)
        };
    }
}
