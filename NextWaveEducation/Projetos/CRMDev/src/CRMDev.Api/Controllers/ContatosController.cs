using CRMDev.Application.DTOs;
using CRMDev.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMDev.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ContatosController : ControllerBase
{
    private readonly ContatoService _service;

    public ContatosController(ContatoService service)
    {
        _service = service;
    }

    /// <summary>Lista contatos, opcionalmente filtrando por nome ou e-mail.</summary>
    [HttpGet]
    public async Task<ActionResult<List<ContatoDto>>> Listar([FromQuery] string? busca, CancellationToken ct)
    {
        return Ok(await _service.ListarAsync(busca, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ContatoDto>> ObterPorId(Guid id, CancellationToken ct)
    {
        var contato = await _service.ObterPorIdAsync(id, ct);
        return contato is null ? NotFound() : Ok(contato);
    }

    [HttpPost]
    public async Task<ActionResult<ContatoDto>> Criar(ContatoCriarDto dto, CancellationToken ct)
    {
        try
        {
            var contato = await _service.CriarAsync(dto, ct);
            return CreatedAtAction(nameof(ObterPorId), new { id = contato.Id }, contato);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ContatoDto>> Atualizar(Guid id, ContatoAtualizarDto dto, CancellationToken ct)
    {
        try
        {
            var contato = await _service.AtualizarAsync(id, dto, ct);
            return contato is null ? NotFound() : Ok(contato);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id, CancellationToken ct)
    {
        var removido = await _service.RemoverAsync(id, ct);
        return removido ? NoContent() : NotFound();
    }

    /// <summary>Adiciona (ou cria, se não existir) uma tag ao contato.</summary>
    [HttpPost("{id:guid}/tags")]
    public async Task<ActionResult<ContatoDto>> AdicionarTag(Guid id, TagAdicionarDto dto, CancellationToken ct)
    {
        var contato = await _service.AdicionarTagAsync(id, dto.Nome, ct);
        return contato is null ? NotFound() : Ok(contato);
    }

    /// <summary>Remove uma tag do contato.</summary>
    [HttpDelete("{id:guid}/tags/{nomeTag}")]
    public async Task<ActionResult<ContatoDto>> RemoverTag(Guid id, string nomeTag, CancellationToken ct)
    {
        var contato = await _service.RemoverTagAsync(id, nomeTag, ct);
        return contato is null ? NotFound() : Ok(contato);
    }
}
