using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace AvaliacaoLivros.AppConsole.Services;

/// <summary>
/// Integração externa (PLUS) para consulta de dados de um livro por ISBN,
/// usando a API pública e gratuita Open Library (https://openlibrary.org/dev/docs/api/books).
/// Não exige chave de API, por isso é uma boa opção para o treinamento.
/// </summary>
public class LivroExternoService
{
    private static readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("https://openlibrary.org/")
    };

    public async Task<LivroExternoResultado?> ConsultarPorIsbnAsync(string isbn)
    {
        string isbnLimpo = isbn.Trim();

        try
        {
            var resposta = await _httpClient.GetFromJsonAsync<Dictionary<string, OpenLibraryLivro>>(
                $"api/books?bibkeys=ISBN:{isbnLimpo}&format=json&jscmd=data");

            if (resposta is null || !resposta.TryGetValue($"ISBN:{isbnLimpo}", out var livro))
                return null;

            return new LivroExternoResultado
            {
                Titulo = livro.Title ?? string.Empty,
                Autor = livro.Authors?.FirstOrDefault()?.Name ?? string.Empty,
                Editora = livro.Publishers?.FirstOrDefault()?.Name ?? string.Empty,
                AnoDePublicacao = ExtrairAno(livro.PublishDate),
                QuantidadePaginas = livro.NumberOfPages ?? 0
            };
        }
        catch
        {
            // Sem conexão, timeout, API fora do ar, ISBN não encontrado, etc.
            return null;
        }
    }

    private static int ExtrairAno(string? publishDate)
    {
        if (string.IsNullOrWhiteSpace(publishDate))
            return 0;

        var digitos = new string(publishDate.Where(char.IsDigit).ToArray());
        if (digitos.Length >= 4 && int.TryParse(digitos[^4..], out int ano))
            return ano;

        return 0;
    }

    private class OpenLibraryLivro
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("authors")]
        public List<OpenLibraryAutor>? Authors { get; set; }

        [JsonPropertyName("publishers")]
        public List<OpenLibraryEditora>? Publishers { get; set; }

        [JsonPropertyName("publish_date")]
        public string? PublishDate { get; set; }

        [JsonPropertyName("number_of_pages")]
        public int? NumberOfPages { get; set; }
    }

    private class OpenLibraryAutor
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    private class OpenLibraryEditora
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}

public class LivroExternoResultado
{
    public string Titulo { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public string Editora { get; set; } = string.Empty;
    public int AnoDePublicacao { get; set; }
    public int QuantidadePaginas { get; set; }
}
