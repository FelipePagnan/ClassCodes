using BancoSangue.Intermediario.Aplicacao.Servicos;
using BancoSangue.Intermediario.Aplicacao.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BancoSangue.Intermediario.Controllers;

[ApiController]
[Route("api/relatorios")]
[Produces("application/json")]
public class RelatoriosController : ControllerBase
{
    private readonly IEstoqueAppService _estoqueAppService;
    private readonly IDoacaoAppService _doacaoAppService;

    public RelatoriosController(
        IEstoqueAppService estoqueAppService,
        IDoacaoAppService doacaoAppService)
    {
        _estoqueAppService = estoqueAppService;
        _doacaoAppService = doacaoAppService;
    }

    /// <summary>Relatório da quantidade total de sangue por tipo disponível.</summary>
    [HttpGet("estoque-por-tipo")]
    public async Task<ActionResult<RelatorioEstoqueViewModel>> EstoquePorTipo()
        => Ok(await _estoqueAppService.RelatorioPorTipoAsync());

    /// <summary>Relatório das doações realizadas nos últimos 30 dias, com dados dos doadores.</summary>
    [HttpGet("doacoes-ultimos-30-dias")]
    public async Task<ActionResult<List<DoacaoComDoadorViewModel>>> DoacoesUltimos30Dias()
        => Ok(await _doacaoAppService.RelatorioUltimos30DiasAsync());
}
