using System.Text.Json;
using Livros.Console.Entidades;
using Livros.Console.Enums;

namespace Livros.Console.Servicos;

/// <summary>
/// Consulta dados de livros na API pública do Google Books (funcionalidade PLUS).
/// Requer conexão com a internet.
/// </summary>
public class ConsultaLivroExternoService
{
    private static readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("https://www.googleapis.com/books/v1/"),
        Timeout = TimeSpan.FromSeconds(15)
    };

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Busca um livro pelo ISBN e devolve os dados já no formato da entidade Livro.
    /// Retorna null quando não encontra ou quando a API está inacessível.
    /// </summary>
    public async Task<Livro?> ConsultarPorIsbnAsync(string isbn)
    {
        var digitos = new string(isbn.Where(char.IsLetterOrDigit).ToArray());
        if (digitos.Length is not (10 or 13))
            return null;

        var resultados = await BuscarAsync($"isbn:{digitos}");
        return resultados.FirstOrDefault();
    }

    /// <summary>Busca livros por título, autor ou qualquer termo livre.</summary>
    public async Task<List<Livro>> ConsultarPorTermoAsync(string termo)
    {
        if (string.IsNullOrWhiteSpace(termo)) return new List<Livro>();
        return await BuscarAsync(termo);
    }

    private async Task<List<Livro>> BuscarAsync(string query)
    {
        try
        {
            var url = $"volumes?q={Uri.EscapeDataString(query)}&maxResults=10";
            using var resposta = await _httpClient.GetAsync(url);

            if (!resposta.IsSuccessStatusCode)
                return new List<Livro>();

            var json = await resposta.Content.ReadAsStringAsync();
            var payload = JsonSerializer.Deserialize<GoogleBooksResposta>(json, _jsonOptions);

            if (payload?.Items is null)
                return new List<Livro>();

            return payload.Items
                .Where(i => i.VolumeInfo is not null)
                .Select(i => MapearParaLivro(i.VolumeInfo!))
                .ToList();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            return new List<Livro>();
        }
    }

    private static Livro MapearParaLivro(GoogleVolumeInfo info) => new()
    {
        Titulo = info.Title ?? string.Empty,
        Descricao = Truncar(info.Description ?? string.Empty, 2000),
        ISBN = ExtrairIsbn(info.IndustryIdentifiers),
        Autor = info.Authors is { Length: > 0 } ? string.Join(", ", info.Authors) : "Autor desconhecido",
        Editora = info.Publisher ?? "Editora desconhecida",
        Genero = MapearGenero(info.Categories),
        AnoDePublicacao = ExtrairAno(info.PublishedDate),
        QuantidadePaginas = info.PageCount > 0 ? info.PageCount : 1
    };

    private static string ExtrairIsbn(GoogleIdentificador[]? identificadores)
    {
        if (identificadores is null || identificadores.Length == 0)
            return string.Empty;

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

    /// <summary>
    /// Traduz as categorias do Google Books para o enum de gênero do sistema.
    /// </summary>
    private static GeneroLivroEnum MapearGenero(string[]? categorias)
    {
        if (categorias is null || categorias.Length == 0)
            return GeneroLivroEnum.Outro;

        var texto = string.Join(" ", categorias).ToLowerInvariant();

        if (texto.Contains("juvenile")) return GeneroLivroEnum.Infantil;
        if (texto.Contains("science fiction")) return GeneroLivroEnum.FiccaoCientifica;
        if (texto.Contains("fantasy")) return GeneroLivroEnum.Fantasia;
        if (texto.Contains("thriller") || texto.Contains("mystery")) return GeneroLivroEnum.Suspense;
        if (texto.Contains("horror")) return GeneroLivroEnum.Terror;
        if (texto.Contains("biography")) return GeneroLivroEnum.Biografia;
        if (texto.Contains("history")) return GeneroLivroEnum.Historia;
        if (texto.Contains("computer") || texto.Contains("technology")) return GeneroLivroEnum.Tecnico;
        if (texto.Contains("self-help")) return GeneroLivroEnum.Autoajuda;
        if (texto.Contains("poetry")) return GeneroLivroEnum.Poesia;
        if (texto.Contains("romance")) return GeneroLivroEnum.Romance;
        if (texto.Contains("fiction")) return GeneroLivroEnum.Ficcao;

        return GeneroLivroEnum.Outro;
    }

    private static string Truncar(string texto, int tamanhoMaximo)
        => texto.Length <= tamanhoMaximo ? texto : texto[..tamanhoMaximo];

    // DTOs internos que refletem o retorno da API do Google Books.
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
