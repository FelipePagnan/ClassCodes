using BancoSangue.Domain.Common;
using BancoSangue.Domain.Exceptions;

namespace BancoSangue.Domain.Entities;

public class Endereco : BaseEntity
{
    private Endereco() { }

    public Endereco(string logradouro, string cidade, string estado, string cep)
    {
        if (string.IsNullOrWhiteSpace(cep))
            throw new RegraNegocioException("O CEP é obrigatório para o endereço.");

        var cepDigitos = new string(cep.Where(char.IsDigit).ToArray());
        if (cepDigitos.Length != 8)
            throw new RegraNegocioException($"O CEP '{cep}' é inválido. Informe 8 dígitos.");

        Logradouro = logradouro?.Trim() ?? string.Empty;
        Cidade = cidade?.Trim() ?? string.Empty;
        Estado = (estado ?? string.Empty).Trim().ToUpperInvariant();
        CEP = $"{cepDigitos[..5]}-{cepDigitos[5..]}";
    }

    public string Logradouro { get; private set; } = string.Empty;
    public string Cidade { get; private set; } = string.Empty;
    public string Estado { get; private set; } = string.Empty;
    public string CEP { get; private set; } = string.Empty;

    public int DoadorId { get; private set; }
    public Doador Doador { get; private set; } = null!;

    internal void VincularDoador(Doador doador)
    {
        Doador = doador;
        DoadorId = doador.Id;
    }

    public void AdicionarComplemento(string? complemento)
    {
        if (!string.IsNullOrWhiteSpace(complemento))
            Logradouro = $"{Logradouro}, {complemento.Trim()}";
    }

    public override string ToString() => $"{Logradouro} - {Cidade}/{Estado} - {CEP}";
}
