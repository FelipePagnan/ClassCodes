using BancoSangue.Application.Common;
using BancoSangue.Application.Relatorios.Queries.RelatorioDoacoes30Dias;
using BancoSangue.Application.Relatorios.Queries.RelatorioEstoquePorTipo;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BancoSangue.Api.Controllers;

[ApiController]
[Route("api/relatorios")]
[Produces("application/json")]
public class RelatoriosController : ControllerBase
{
    private readonly ISender _sender;

    public RelatoriosController(ISender sender) => _sender = sender;

    /// <summary>Relatório da quantidade total de sangue por tipo disponível.</summary>
    [HttpGet("estoque-por-tipo")]
    [ProducesResponseType(typeof(RelatorioEstoqueDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> EstoquePorTipo(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new RelatorioEstoquePorTipoQuery(), cancellationToken));

    /// <summary>Relatório das doações dos últimos 30 dias com informações dos doadores.</summary>
    [HttpGet("doacoes-ultimos-30-dias")]
    [ProducesResponseType(typeof(RelatorioDoacoes30DiasDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> DoacoesUltimos30Dias(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new RelatorioDoacoes30DiasQuery(), cancellationToken));
}
