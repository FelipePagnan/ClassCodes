using System.Net.Http.Json;
using BancoSangue.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace BancoSangue.Infrastructure.Services;

/// <summary>
/// Implementação de ICepService usando a API pública ViaCEP (https://viacep.com.br).
/// </summary>
public class ViaCepService : ICepService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ViaCepService> _logger;

    public ViaCepService(HttpClient httpClient, ILogger<ViaCepService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<EnderecoCepDto?> ConsultarAsync(
        string cep,
        CancellationToken cancellationToken = default)
    {
        var digitos = new string(cep.Where(char.IsDigit).ToArray());

        if (digitos.Length != 8)
        {
            _logger.LogWarning("CEP com formato inválido: {Cep}", cep);
            return null;
        }

        try
        {
            var resposta = await _httpClient.GetFromJsonAsync<ViaCepResponse>(
                $"{digitos}/json/", cancellationToken);

            if (resposta is null || resposta.Erro)
            {
                _logger.LogWarning("CEP não encontrado no ViaCEP: {Cep}", digitos);
                return null;
            }

            return new EnderecoCepDto(
                resposta.Logradouro ?? string.Empty,
                resposta.Localidade ?? string.Empty,
                resposta.Uf ?? string.Empty,
                resposta.Cep ?? digitos);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "Falha ao consultar o ViaCEP para o CEP {Cep}", digitos);
            return null;
        }
    }

    private sealed class ViaCepResponse
    {
        public string? Cep { get; set; }
        public string? Logradouro { get; set; }
        public string? Localidade { get; set; }
        public string? Uf { get; set; }
        public bool Erro { get; set; }
    }
}
