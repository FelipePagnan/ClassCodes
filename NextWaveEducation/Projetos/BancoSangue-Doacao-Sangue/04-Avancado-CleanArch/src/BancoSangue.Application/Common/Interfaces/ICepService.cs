namespace BancoSangue.Application.Common.Interfaces;

/// <summary>
/// Abstração da API externa de consulta de CEP.
/// A implementação (ViaCEP) vive na camada de Infrastructure.
/// </summary>
public interface ICepService
{
    Task<EnderecoCepDto?> ConsultarAsync(string cep, CancellationToken cancellationToken = default);
}

public record EnderecoCepDto(string Logradouro, string Cidade, string Estado, string Cep);
