using BancoSangue.Intermediario.Aplicacao.InputModels;
using BancoSangue.Intermediario.Aplicacao.Servicos;
using BancoSangue.Intermediario.Aplicacao.ViewModels;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace BancoSangue.Intermediario.Controllers;

[ApiController]
[Route("api/doacoes")]
[Produces("application/json")]
public class DoacoesController : ControllerBase
{
    private readonly IDoacaoAppService _doacaoAppService;
    private readonly IValidator<CriarDoacaoInputModel> _validator;

    public DoacoesController(
        IDoacaoAppService doacaoAppService,
        IValidator<CriarDoacaoInputModel> validator)
    {
        _doacaoAppService = doacaoAppService;
        _validator = validator;
    }

    /// <summary>Lista todas as doações registradas.</summary>
    [HttpGet]
    public async Task<ActionResult<List<DoacaoComDoadorViewModel>>> Listar()
        => Ok(await _doacaoAppService.ListarAsync());

    /// <summary>
    /// Registra uma doação e atualiza o estoque de sangue correspondente.
    /// Retorna um alerta quando o estoque fica abaixo do mínimo.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ResultadoDoacaoViewModel>> Registrar([FromBody] CriarDoacaoInputModel input)
    {
        await _validator.ValidateAndThrowAsync(input);

        var resultado = await _doacaoAppService.RegistrarAsync(input);
        return CreatedAtAction(nameof(Listar), resultado);
    }
}
