using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EduPlatform.Api.Services;

public record AsaasCustomerResult(string Id);
public record AsaasChargeResult(string Id, string? InvoiceUrl, string Status, DateTime DueDate);

public interface IAsaasService
{
    Task<AsaasCustomerResult> CriarClienteAsync(string nome, string email, string documento, string? telefone, CancellationToken ct = default);
    Task<AsaasChargeResult> CriarCobrancaAsync(string customerId, decimal valor, DateTime vencimento, string descricao, CancellationToken ct = default);
}

/// <summary>
/// Integração com a API do Asaas (https://docs.asaas.com) para criação de clientes
/// e cobranças referentes às assinaturas da plataforma.
/// Requer configuração em appsettings: "Asaas:ApiKey" e "Asaas:BaseUrl".
/// </summary>
public class AsaasService : IAsaasService
{
    private readonly HttpClient _http;
    private readonly ILogger<AsaasService> _logger;

    public AsaasService(HttpClient http, IConfiguration config, ILogger<AsaasService> logger)
    {
        _http = http;
        _logger = logger;

        var baseUrl = config["Asaas:BaseUrl"] ?? "https://sandbox.asaas.com/api/v3/";
        var apiKey = config["Asaas:ApiKey"] ?? throw new InvalidOperationException("Asaas:ApiKey não configurada.");

        _http.BaseAddress = new Uri(baseUrl);
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _http.DefaultRequestHeaders.Add("access_token", apiKey);
    }

    public async Task<AsaasCustomerResult> CriarClienteAsync(string nome, string email, string documento, string? telefone, CancellationToken ct = default)
    {
        var payload = new
        {
            name = nome,
            email,
            cpfCnpj = documento,
            mobilePhone = telefone
        };

        var response = await _http.PostAsync("customers",
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"), ct);

        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Falha ao criar cliente no Asaas: {Body}", body);
            throw new HttpRequestException($"Erro ao criar cliente no Asaas: {body}");
        }

        using var doc = JsonDocument.Parse(body);
        var id = doc.RootElement.GetProperty("id").GetString()!;
        return new AsaasCustomerResult(id);
    }

    public async Task<AsaasChargeResult> CriarCobrancaAsync(string customerId, decimal valor, DateTime vencimento, string descricao, CancellationToken ct = default)
    {
        var payload = new
        {
            customer = customerId,
            billingType = "UNDEFINED", // deixa o cliente escolher (boleto, cartão, pix)
            value = valor,
            dueDate = vencimento.ToString("yyyy-MM-dd"),
            description = descricao
        };

        var response = await _http.PostAsync("payments",
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"), ct);

        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Falha ao criar cobrança no Asaas: {Body}", body);
            throw new HttpRequestException($"Erro ao criar cobrança no Asaas: {body}");
        }

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        var id = root.GetProperty("id").GetString()!;
        var invoiceUrl = root.TryGetProperty("invoiceUrl", out var iu) ? iu.GetString() : null;
        var status = root.GetProperty("status").GetString() ?? "PENDING";

        return new AsaasChargeResult(id, invoiceUrl, status, vencimento);
    }
}
