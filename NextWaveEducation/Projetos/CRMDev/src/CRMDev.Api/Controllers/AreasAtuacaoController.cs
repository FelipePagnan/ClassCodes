using CRMDev.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMDev.Api.Controllers;

public record AreaAtuacaoCriarDto(string Nome);

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AreasAtuacaoController : ControllerBase
{
    private readonly IAreaAtuacaoRepository _repo;

    public AreasAtuacaoController(IAreaAtuacaoRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        return Ok(await _repo.ListarAsync(ct));
    }

    [HttpPost]
    public async Task<IActionResult> Criar(AreaAtuacaoCriarDto dto, CancellationToken ct)
    {
        var area = await _repo.CriarAsync(dto.Nome, ct);
        return Ok(area);
    }
}
