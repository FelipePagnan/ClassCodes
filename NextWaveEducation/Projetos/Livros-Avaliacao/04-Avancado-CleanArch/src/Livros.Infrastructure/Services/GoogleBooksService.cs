using System.Net.Http.Json;
using Livros.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Livros.Infrastructure.Services;

/// <summary>
/// Implementação de IConsultaLivroExternoService usando a API pública
/// do Google Books (https://www.googleapis.com/books/v1).
/// </summary>
public class GoogleBooksService : IConsultaLivroExternoService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleBooksService> _logger;

    public GoogleBooksService(HttpClient httpClient, ILogger<GoogleBooksService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<LivroExternoDto?> ConsultarPorIsbnAsync(
        string isbn, CancellationToken cancellationToken = default)
    {
        var digitos = new string(isbn.Where(char.IsLetterOrDigit).ToArray());

        if (digitos.Length is not (10 or 13))
        {
            _logger.LogWarning("ISBN com formato inválido na consulta externa: {Isbn}", isbn);
            return null;
        }

        var resultados = await BuscarAsync($"isbn:{digitos}", cancellationToken);
        return resultados.FirstOrDefault();
    }

    public async Task<List<LivroExternoDto>> ConsultarPorTermoAsync(
        string termo, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(termo))
            return new List<LivroExternoDto>();

        return await BuscarAsync(termo.Trim(), cancellationToken);
    }

    private async Task<List<LivroExternoDto>> BuscarAsync(
        string query, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"volumes?q={Uri.EscapeDataString(query)}&maxResults=10";
            var payload = await _httpClient.GetFromJsonAsync<GoogleBooksResposta>(url, cancellationToken);

            if (payload?.Items is null)
            {
                _logger.LogInformation("Nenhum resultado no Google Books para '{Query}'", query);
                return new List<LivroExternoDto>();
            }

            return payload.Items
                .Where(i => i.VolumeInfo is not null)
                .Select(i => Mapear(i.VolumeInfo!))
                .ToList();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "Falha ao consultar o Google Books para '{Query}'", query);
            return new List<LivroExternoDto>();
        }
    }

    private static LivroExternoDto Mapear(GoogleVolumeInfo info) => new(
        info.Title ?? string.Empty,
        Truncar(info.Description ?? string.Empty, 2000),
        ExtrairIsbn(info.IndustryIdentifiers),
        info.Authors is { Length: > 0 } ? string.Join(", ", info.Authors) : "Autor desconhecido",
        info.Publisher ?? "Editora desconhecida",
        info.Categories is { Length: > 0 } ? info.Categories[0] : null,
        ExtrairAno(info.PublishedDate),
        info.PageCount > 0 ? info.PageCount : 1);

    private static string ExtrairIsbn(GoogleIdentificador[]? identificadores)
    {
        if (identificadores is null || identificadores.Length == 0) return string.Empty;

        return identificadores.FirstOrDefault(i => i.Type == "ISBN_13")?.Identifier
            ?? identificadores.FirstOrDefault(i => i.Type == "ISBN_10")?.Identifier
            ?? string.Empty;
    }

    private static int ExtrairAno(string? publishedDate)
    {
        if (string.IsNullOrWhiteSpace(publishedDate)) return DateTime.Today.Year;

        var parte = publishedDate.Split('-')[0];
        return int.TryParse(parte, out var ano) ? ano : DateTime.Today.Year;
    }

    private static string Truncar(string texto, int tamanhoMaximo)
        => texto.Length <= tamanhoMaximo ? texto : texto[..tamanhoMaximo];

    private sealed class GoogleBooksResposta
    {
        public GoogleItem[]? Items { get; set; }
    }

    private sealed class GoogleItem
    {
        public GoogleVolumeInfo? VolumeInfo { get; set; }
    }

    private sealed class GoogleVolumeInfo
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Publisher { get; set; }
        public string? PublishedDate { get; set; }
        public string[]? Authors { get; set; }
        public string[]? Categories { get; set; }
        public int PageCount { get; set; }
        public GoogleIdentificador[]? IndustryIdentifiers { get; set; }
    }

    private sealed class GoogleIdentificador
    {
        public string? Type { get; set; }
        public string? Identifier { get; set; }
    }
}
