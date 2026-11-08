using BancoSangue.Application.Common;
using BancoSangue.Application.Estoque.Queries.ObterEstoque;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BancoSangue.Api.Controllers;

[ApiController]
[Route("api/estoque")]
[Produces("application/json")]
public class EstoqueController : ControllerBase
{
    private readonly ISender _sender;

    public EstoqueController(ISender sender) => _sender = sender;

    /// <summary>Lista o estoque atual de todos os tipos sanguíneos.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<EstoqueDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new ObterEstoqueQuery(), cancellationToken));

    /// <summary>Lista apenas os tipos sanguíneos com estoque abaixo do mínimo definido.</summary>
    [HttpGet("alertas")]
    [ProducesResponseType(typeof(List<EstoqueDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Alertas(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new ObterEstoqueQuery(SomenteAbaixoDoMinimo: true), cancellationToken));
}
