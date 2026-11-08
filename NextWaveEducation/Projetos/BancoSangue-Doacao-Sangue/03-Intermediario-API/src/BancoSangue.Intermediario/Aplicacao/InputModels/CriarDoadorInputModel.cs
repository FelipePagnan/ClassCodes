using BancoSangue.Intermediario.Dominio.Enums;

namespace BancoSangue.Intermediario.Aplicacao.InputModels;

/// <summary>
/// Modelo de entrada para cadastro de doador.
/// Se o CEP for informado, o endereço é buscado automaticamente na API ViaCEP.
/// </summary>
public class CriarDoadorInputModel
{
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public Genero Genero { get; set; }
    public double Peso { get; set; }
    public TipoSanguineo TipoSanguineo { get; set; }
    public FatorRh FatorRh { get; set; }

    /// <summary>CEP para consulta automática do endereço (opcional).</summary>
    public string? Cep { get; set; }

    /// <summary>Número/complemento informado manualmente, concatenado ao logradouro retornado pelo ViaCEP.</summary>
    public string? Complemento { get; set; }
}
