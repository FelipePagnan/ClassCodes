using Livros.Intermediario.Aplicacao.Servicos;
using Livros.Intermediario.Aplicacao.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Livros.Intermediario.Controllers;

[ApiController]
[Route("api/relatorios")]
[Produces("application/json")]
public class RelatoriosController : ControllerBase
{
    private readonly IAvaliacaoAppService _avaliacaoAppService;

    public RelatoriosController(IAvaliacaoAppService avaliacaoAppService)
        => _avaliacaoAppService = avaliacaoAppService;

    /// <summary>
    /// Quantidade de livros lidos num determinado ano, com base na data
    /// de fim de leitura informada nas avaliações.
    /// </summary>
    [HttpGet("livros-lidos/{ano:int}")]
    public async Task<ActionResult<RelatorioAnualViewModel>> LivrosLidosNoAno(int ano)
        => Ok(await _avaliacaoAppService.RelatorioLivrosLidosAsync(ano));

    /// <summary>Relatório do ano corrente.</summary>
    [HttpGet("livros-lidos")]
    public async Task<ActionResult<RelatorioAnualViewModel>> LivrosLidosNoAnoCorrente()
        => Ok(await _avaliacaoAppService.RelatorioLivrosLidosAsync(DateTime.Today.Year));
}
