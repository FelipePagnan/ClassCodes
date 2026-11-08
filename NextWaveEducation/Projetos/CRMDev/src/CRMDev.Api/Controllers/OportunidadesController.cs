using CRMDev.Application.DTOs;
using CRMDev.Application.Services;
using CRMDev.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMDev.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OportunidadesController : ControllerBase
{
    private readonly OportunidadeService _service;

    public OportunidadesController(OportunidadeService service)
    {
        _service = service;
    }

    /// <summary>Lista oportunidades, com filtros opcionais por etapa, status e contato.</summary>
    [HttpGet]
    public async Task<ActionResult<List<OportunidadeDto>>> Listar(
        [FromQuery] EtapaOportunidade? etapa,
        [FromQuery] StatusOportunidade? status,
        [FromQuery] Guid? contatoId,
        CancellationToken ct)
    {
        return Ok(await _service.ListarAsync(etapa, status, contatoId, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OportunidadeDto>> ObterPorId(Guid id, CancellationToken ct)
    {
        var op = await _service.ObterPorIdAsync(id, ct);
        return op is null ? NotFound() : Ok(op);
    }

    [HttpPost]
    public async Task<ActionResult<OportunidadeDto>> Criar(OportunidadeCriarDto dto, CancellationToken ct)
    {
        try
        {
            var op = await _service.CriarAsync(dto, ct);
            return CreatedAtAction(nameof(ObterPorId), new { id = op.Id }, op);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<OportunidadeDto>> Atualizar(Guid id, OportunidadeAtualizarDto dto, CancellationToken ct)
    {
        var op = await _service.AtualizarAsync(id, dto, ct);
        return op is null ? NotFound() : Ok(op);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id, CancellationToken ct)
    {
        var removido = await _service.RemoverAsync(id, ct);
        return removido ? NoContent() : NotFound();
    }

    /// <summary>Move a oportunidade para outra etapa do funil (não permitido se já estiver fechada).</summary>
    [HttpPatch("{id:guid}/etapa")]
    public async Task<ActionResult<OportunidadeDto>> AlterarEtapa(Guid id, AlterarEtapaDto dto, CancellationToken ct)
    {
        try
        {
            var op = await _service.AlterarEtapaAsync(id, dto.NovaEtapa, ct);
            return op is null ? NotFound() : Ok(op);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    /// <summary>Marca a oportunidade como ganha e a move para a etapa Fechado.</summary>
    [HttpPatch("{id:guid}/ganhar")]
    public async Task<ActionResult<OportunidadeDto>> MarcarGanho(Guid id, CancellationToken ct)
    {
        var op = await _service.MarcarGanhoAsync(id, ct);
        return op is null ? NotFound() : Ok(op);
    }

    /// <summary>Marca a oportunidade como perdida, informando a razão da perda.</summary>
    [HttpPatch("{id:guid}/perder")]
    public async Task<ActionResult<OportunidadeDto>> MarcarPerdido(Guid id, MarcarPerdidoDto dto, CancellationToken ct)
    {
        var op = await _service.MarcarPerdidoAsync(id, dto.Razao, ct);
        return op is null ? NotFound() : Ok(op);
    }
}
