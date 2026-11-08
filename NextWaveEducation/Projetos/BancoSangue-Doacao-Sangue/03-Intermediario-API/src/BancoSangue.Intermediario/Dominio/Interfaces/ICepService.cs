using BancoSangue.Intermediario.Dominio.Entidades;

namespace BancoSangue.Intermediario.Dominio.Interfaces;

/// <summary>
/// Abstração da integração com API externa de consulta de CEP (ViaCEP).
/// </summary>
public interface ICepService
{
    Task<Endereco?> ConsultarAsync(string cep, CancellationToken cancellationToken = default);
}
