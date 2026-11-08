using Caixinhas.Application.Common;
using Caixinhas.Application.Relatorios.Queries.RelatorioEvolucao;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Caixinhas.Api.Controllers;

[ApiController]
[Route("api/relatorios")]
[Produces("application/json")]
public class RelatoriosController : ControllerBase
{
    private readonly ISender _sender;

    public RelatoriosController(ISender sender) => _sender = sender;

    /// <summary>
    /// Relatório da evolução das caixinhas: situação de cada objetivo e a
    /// movimentação consolidada mês a mês.
    /// </summary>
    [HttpGet("evolucao")]
    [ProducesResponseType(typeof(RelatorioEvolucaoDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Evolucao(
        [FromQuery] int mesesDeHistorico = 6, CancellationToken ct = default)
        => Ok(await _sender.Send(new RelatorioEvolucaoQuery(mesesDeHistorico), ct));
}
