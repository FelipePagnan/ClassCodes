using BancoSangue.Intermediario.Aplicacao.InputModels;
using BancoSangue.Intermediario.Aplicacao.Servicos;
using BancoSangue.Intermediario.Aplicacao.ViewModels;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace BancoSangue.Intermediario.Controllers;

[ApiController]
[Route("api/doadores")]
[Produces("application/json")]
public class DoadoresController : ControllerBase
{
    private readonly IDoadorAppService _doadorAppService;
    private readonly IValidator<CriarDoadorInputModel> _validator;

    public DoadoresController(
        IDoadorAppService doadorAppService,
        IValidator<CriarDoadorInputModel> validator)
    {
        _doadorAppService = doadorAppService;
        _validator = validator;
    }

    /// <summary>Lista todos os doadores cadastrados.</summary>
    [HttpGet]
    public async Task<ActionResult<List<DoadorViewModel>>> Listar()
        => Ok(await _doadorAppService.ListarAsync());

    /// <summary>Consulta um doador pelo id, incluindo seu histórico de doações.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DoadorDetalheViewModel>> ObterPorId(int id)
        => Ok(await _doadorAppService.ObterPorIdAsync(id));

    /// <summary>Consulta o histórico de doações de um doador.</summary>
    [HttpGet("{id:int}/doacoes")]
    public async Task<ActionResult<List<DoacaoViewModel>>> HistoricoDoacoes(int id)
        => Ok(await _doadorAppService.ObterHistoricoDoacoesAsync(id));

    /// <summary>
    /// Cadastra um novo doador. Informe o CEP para que o endereço seja preenchido
    /// automaticamente a partir da API ViaCEP.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DoadorViewModel>> Cadastrar([FromBody] CriarDoadorInputModel input)
    {
        // A exceção de validação é capturada pelo middleware de tratamento de exceções.
        await _validator.ValidateAndThrowAsync(input);

        var doador = await _doadorAppService.CadastrarAsync(input);
        return CreatedAtAction(nameof(ObterPorId), new { id = doador.Id }, doador);
    }
}
