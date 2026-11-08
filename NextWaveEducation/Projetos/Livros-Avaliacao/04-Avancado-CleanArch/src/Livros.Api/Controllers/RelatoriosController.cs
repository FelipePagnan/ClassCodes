using Livros.Application.Common;
using Livros.Application.Relatorios.Queries.RelatorioLivrosLidos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Livros.Api.Controllers;

[ApiController]
[Route("api/relatorios")]
[Produces("application/json")]
public class RelatoriosController : ControllerBase
{
    private readonly ISender _sender;

    public RelatoriosController(ISender sender) => _sender = sender;

    /// <summary>
    /// Quantidade de livros lidos num determinado ano, com base na data de
    /// fim de leitura informada nas avaliações.
    /// </summary>
    [HttpGet("livros-lidos/{ano:int}")]
    [ProducesResponseType(typeof(RelatorioLivrosLidosDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> LivrosLidosNoAno(int ano, CancellationToken ct)
        => Ok(await _sender.Send(new RelatorioLivrosLidosQuery(ano), ct));

    /// <summary>Relatório de livros lidos no ano corrente.</summary>
    [HttpGet("livros-lidos")]
    [ProducesResponseType(typeof(RelatorioLivrosLidosDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> LivrosLidosNoAnoCorrente(CancellationToken ct)
        => Ok(await _sender.Send(new RelatorioLivrosLidosQuery(DateTime.Today.Year), ct));
}
