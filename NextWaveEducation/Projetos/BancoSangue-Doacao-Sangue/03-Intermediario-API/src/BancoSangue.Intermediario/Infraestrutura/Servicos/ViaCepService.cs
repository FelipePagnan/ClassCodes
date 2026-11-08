using System.Net.Http.Json;
using BancoSangue.Intermediario.Dominio.Entidades;
using BancoSangue.Intermediario.Dominio.Interfaces;

namespace BancoSangue.Intermediario.Infraestrutura.Servicos;

/// <summary>
/// Integração com a API externa ViaCEP (https://viacep.com.br) para consulta de endereço por CEP.
/// Registrado via IHttpClientFactory no Program.cs.
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

    public async Task<Endereco?> ConsultarAsync(string cep, CancellationToken cancellationToken = default)
    {
        var cepLimpo = new string(cep.Where(char.IsDigit).ToArray());
        if (cepLimpo.Length != 8)
        {
            _logger.LogWarning("CEP com formato inválido: {Cep}", cep);
            return null;
        }

        try
        {
            var resposta = await _httpClient.GetFromJsonAsync<ViaCepResponse>(
                $"{cepLimpo}/json/", cancellationToken);

            if (resposta is null || resposta.Erro)
            {
                _logger.LogWarning("CEP não encontrado na API ViaCEP: {Cep}", cepLimpo);
                return null;
            }

            return new Endereco(
                resposta.Logradouro ?? string.Empty,
                resposta.Localidade ?? string.Empty,
                resposta.Uf ?? string.Empty,
                resposta.Cep ?? cepLimpo);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "Falha ao consultar a API ViaCEP para o CEP {Cep}", cepLimpo);
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
