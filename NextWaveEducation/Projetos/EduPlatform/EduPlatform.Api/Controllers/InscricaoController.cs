using EduPlatform.Api.Data;
using EduPlatform.Api.Models;
using EduPlatform.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EduPlatform.Api.Controllers;

public record InscreverAssinaturaDto(Guid IdUsuario, Guid IdAssinatura);

[ApiController]
[Route("api/[controller]")]
public class InscricaoController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAsaasService _asaas;
    private readonly ILogger<InscricaoController> _logger;

    public InscricaoController(AppDbContext db, IAsaasService asaas, ILogger<InscricaoController> logger)
    {
        _db = db;
        _asaas = asaas;
        _logger = logger;
    }

    /// <summary>
    /// Inscreve o usuário em uma assinatura: cria (ou reaproveita) o Cliente no Asaas,
    /// gera a Cobrança e salva o vínculo em UsuarioAssinatura + PagamentoAssinatura.
    /// </summary>
    [HttpPost("assinatura")]
    public async Task<IActionResult> InscreverEmAssinatura(InscreverAssinaturaDto dto, CancellationToken ct)
    {
        var usuario = await _db.Usuarios.FindAsync(new object?[] { dto.IdUsuario }, ct);
        if (usuario is null) return BadRequest("Usuário não encontrado.");

        var assinatura = await _db.Assinaturas.FindAsync(new object?[] { dto.IdAssinatura }, ct);
        if (assinatura is null) return BadRequest("Assinatura não encontrada.");

        // 1) Obtém ou cria o vínculo UsuarioAssinatura (pendente)
        var vinculo = await _db.UsuarioAssinaturas
            .FirstOrDefaultAsync(v => v.IdUsuario == dto.IdUsuario && v.IdAssinatura == dto.IdAssinatura
                && v.Status != StatusUsuarioAssinatura.Expirado, ct);

        var dataInicio = DateTime.UtcNow;
        if (vinculo is null)
        {
            vinculo = new UsuarioAssinatura
            {
                IdUsuario = dto.IdUsuario,
                IdAssinatura = dto.IdAssinatura,
                Status = StatusUsuarioAssinatura.Pendente,
                DataInicio = dataInicio,
                DataExpiracao = dataInicio.AddDays(assinatura.Duracao)
            };
            _db.UsuarioAssinaturas.Add(vinculo);
        }

        // 2) Cria o cliente no Asaas (se ainda não existir)
        if (string.IsNullOrEmpty(vinculo.AsaasCustomerId))
        {
            var cliente = await _asaas.CriarClienteAsync(
                usuario.NomeCompleto, usuario.Email, usuario.Documento ?? "", usuario.TelefoneCelular, ct);
            vinculo.AsaasCustomerId = cliente.Id;
        }

        // 3) Cria a cobrança no Asaas
        var vencimento = DateTime.UtcNow.AddDays(3);
        var cobranca = await _asaas.CriarCobrancaAsync(
            vinculo.AsaasCustomerId!, assinatura.Valor, vencimento, $"Assinatura: {assinatura.Nome}", ct);

        // 4) Salva o pagamento
        var pagamento = new PagamentoAssinatura
        {
            IdUsuarioAssinatura = vinculo.Id,
            Valor = assinatura.Valor,
            Status = StatusPagamento.Pendente,
            LinkPagamento = cobranca.InvoiceUrl,
            IdExternoPagamento = cobranca.Id,
            DataVencimento = vencimento,
            DataProcessamento = DateTime.UtcNow
        };

        _db.PagamentoAssinaturas.Add(pagamento);
        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            vinculo.Id,
            vinculo.Status,
            Pagamento = new { pagamento.Id, pagamento.LinkPagamento, pagamento.DataVencimento }
        });
    }
}

[ApiController]
[Route("api/webhooks/asaas")]
public class AsaasWebhookController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<AsaasWebhookController> _logger;

    public AsaasWebhookController(AppDbContext db, ILogger<AsaasWebhookController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Endpoint que recebe as notificações de cobrança do Asaas
    /// (configurar em Configurações > Integrações > Webhooks no painel do Asaas).
    /// Eventos relevantes: PAYMENT_CONFIRMED, PAYMENT_RECEIVED, PAYMENT_OVERDUE, PAYMENT_DELETED.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Receber([FromBody] JsonElement payload, CancellationToken ct)
    {
        _logger.LogInformation("Webhook Asaas recebido: {Payload}", payload.GetRawText());

        if (!payload.TryGetProperty("event", out var eventoEl)) return BadRequest();
        var evento = eventoEl.GetString();

        if (!payload.TryGetProperty("payment", out var paymentEl)) return BadRequest();
        var idExterno = paymentEl.GetProperty("id").GetString();

        var pagamento = await _db.PagamentoAssinaturas
            .Include(p => p.UsuarioAssinatura)
            .FirstOrDefaultAsync(p => p.IdExternoPagamento == idExterno, ct);

        if (pagamento is null)
        {
            _logger.LogWarning("Pagamento não encontrado para cobrança Asaas {Id}", idExterno);
            return Ok(); // Responde 200 para o Asaas não reenviar indefinidamente.
        }

        switch (evento)
        {
            case "PAYMENT_CONFIRMED":
            case "PAYMENT_RECEIVED":
                pagamento.Status = StatusPagamento.Sucesso;
                if (pagamento.UsuarioAssinatura is not null)
                    pagamento.UsuarioAssinatura.Status = StatusUsuarioAssinatura.Ativo;
                break;

            case "PAYMENT_OVERDUE":
                pagamento.Status = StatusPagamento.Atrasado;
                break;

            case "PAYMENT_DELETED":
            case "PAYMENT_REFUNDED":
                pagamento.Status = StatusPagamento.Falha;
                if (pagamento.UsuarioAssinatura is not null)
                    pagamento.UsuarioAssinatura.Status = StatusUsuarioAssinatura.Desativado;
                break;

            default:
                _logger.LogInformation("Evento Asaas não tratado: {Evento}", evento);
                break;
        }

        pagamento.DataProcessamento = DateTime.UtcNow;
        pagamento.Mensagem = evento;

        await _db.SaveChangesAsync(ct);
        return Ok();
    }
}
