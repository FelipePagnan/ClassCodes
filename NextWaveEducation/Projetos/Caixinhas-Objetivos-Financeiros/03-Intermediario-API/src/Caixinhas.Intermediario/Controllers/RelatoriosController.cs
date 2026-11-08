using Caixinhas.Intermediario.Aplicacao.Servicos;
using Caixinhas.Intermediario.Aplicacao.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Caixinhas.Intermediario.Controllers;

[ApiController]
[Route("api/relatorios")]
[Produces("application/json")]
public class RelatoriosController : ControllerBase
{
    private readonly IRelatorioAppService _relatorioAppService;

    public RelatoriosController(IRelatorioAppService relatorioAppService)
        => _relatorioAppService = relatorioAppService;

    /// <summary>
    /// Relatório da evolução das caixinhas: situação de cada objetivo e a
    /// movimentação consolidada mês a mês.
    /// </summary>
    [HttpGet("evolucao")]
    public async Task<ActionResult<RelatorioEvolucaoViewModel>> Evolucao(
        [FromQuery] int mesesDeHistorico = 6)
        => Ok(await _relatorioAppService.GerarEvolucaoAsync(mesesDeHistorico));
}
