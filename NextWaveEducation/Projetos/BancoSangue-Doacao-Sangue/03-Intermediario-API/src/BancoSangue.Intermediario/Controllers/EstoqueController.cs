using BancoSangue.Intermediario.Aplicacao.Servicos;
using BancoSangue.Intermediario.Aplicacao.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BancoSangue.Intermediario.Controllers;

[ApiController]
[Route("api/estoque")]
[Produces("application/json")]
public class EstoqueController : ControllerBase
{
    private readonly IEstoqueAppService _estoqueAppService;

    public EstoqueController(IEstoqueAppService estoqueAppService)
        => _estoqueAppService = estoqueAppService;

    /// <summary>Lista o estoque atual de todos os tipos sanguíneos.</summary>
    [HttpGet]
    public async Task<ActionResult<List<EstoqueViewModel>>> Listar()
        => Ok(await _estoqueAppService.ListarAsync());

    /// <summary>Lista apenas os tipos sanguíneos com estoque abaixo do mínimo.</summary>
    [HttpGet("alertas")]
    public async Task<ActionResult<List<EstoqueViewModel>>> Alertas()
        => Ok(await _estoqueAppService.ListarEstoqueBaixoAsync());
}
