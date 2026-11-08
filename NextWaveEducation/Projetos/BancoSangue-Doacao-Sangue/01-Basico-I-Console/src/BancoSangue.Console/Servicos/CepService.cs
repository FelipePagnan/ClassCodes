using System.Net.Http.Json;
using BancoSangue.Console.Entidades;

namespace BancoSangue.Console.Servicos;

/// <summary>
/// Integração com a API pública ViaCEP para consulta de endereço por CEP (funcionalidade PLUS).
/// Requer conexão com a internet.
/// </summary>
public class CepService
{
    private static readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("https://viacep.com.br/ws/")
    };

    public async Task<Endereco?> ConsultarAsync(string cep)
    {
        var cepLimpo = new string(cep.Where(char.IsDigit).ToArray());
        if (cepLimpo.Length != 8)
            return null;

        try
        {
            var resultado = await _httpClient.GetFromJsonAsync<ViaCepResponse>($"{cepLimpo}/json/");
            if (resultado is null || resultado.erro)
                return null;

            return new Endereco
            {
                Logradouro = resultado.logradouro ?? string.Empty,
                Cidade = resultado.localidade ?? string.Empty,
                Estado = resultado.uf ?? string.Empty,
                CEP = resultado.cep ?? cep
            };
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    // DTO interno que reflete o retorno da API ViaCEP.
    private sealed class ViaCepResponse
    {
        public string? cep { get; set; }
        public string? logradouro { get; set; }
        public string? localidade { get; set; }
        public string? uf { get; set; }
        public bool erro { get; set; }
    }
}
