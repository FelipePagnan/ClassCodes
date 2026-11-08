using Caixinhas.Intermediario.Aplicacao.InputModels;
using Caixinhas.Intermediario.Aplicacao.Servicos;
using Caixinhas.Intermediario.Aplicacao.ViewModels;
using Caixinhas.Intermediario.Dominio.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Caixinhas.Intermediario.Controllers;

[ApiController]
[Route("api/objetivos")]
[Produces("application/json")]
public class ObjetivosController : ControllerBase
{
    private readonly IObjetivoAppService _objetivoAppService;
    private readonly IValidator<CriarObjetivoInputModel> _validadorCriar;
    private readonly IValidator<AtualizarObjetivoInputModel> _validadorAtualizar;
    private readonly IValidator<SimularEvolucaoInputModel> _validadorSimular;

    public ObjetivosController(
        IObjetivoAppService objetivoAppService,
        IValidator<CriarObjetivoInputModel> validadorCriar,
        IValidator<AtualizarObjetivoInputModel> validadorAtualizar,
        IValidator<SimularEvolucaoInputModel> validadorSimular)
    {
        _objetivoAppService = objetivoAppService;
        _validadorCriar = validadorCriar;
        _validadorAtualizar = validadorAtualizar;
        _validadorSimular = validadorSimular;
    }

    /// <summary>Lista as caixinhas ativas, opcionalmente filtrando por status.</summary>
    [HttpGet]
    public async Task<ActionResult<List<ObjetivoViewModel>>> Listar(
        [FromQuery] StatusObjetivo? status)
        => Ok(await _objetivoAppService.ListarAsync(status));

    /// <summary>Detalhes da caixinha com as transações e o resumo de movimentações.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ObjetivoDetalheViewModel>> ObterPorId(int id)
        => Ok(await _objetivoAppService.ObterPorIdAsync(id));

    /// <summary>Cadastra uma nova caixinha.</summary>
    [HttpPost]
    public async Task<ActionResult<ObjetivoViewModel>> Cadastrar(
        [FromBody] CriarObjetivoInputModel input)
    {
        await _validadorCriar.ValidateAndThrowAsync(input);

        var objetivo = await _objetivoAppService.CadastrarAsync(input);
        return CreatedAtAction(nameof(ObterPorId), new { id = objetivo.Id }, objetivo);
    }

    /// <summary>Atualiza os dados da caixinha.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ObjetivoViewModel>> Atualizar(
        int id, [FromBody] AtualizarObjetivoInputModel input)
    {
        await _validadorAtualizar.ValidateAndThrowAsync(input);
        return Ok(await _objetivoAppService.AtualizarAsync(id, input));
    }

    /// <summary>
    /// Remoção lógica da caixinha. O registro continua no banco com
    /// EstaDeletado marcado e sai das listagens.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        await _objetivoAppService.RemoverAsync(id);
        return NoContent();
    }

    /// <summary>Pausa a caixinha: ela deixa de aceitar transações.</summary>
    [HttpPatch("{id:int}/pausar")]
    public async Task<ActionResult<ObjetivoViewModel>> Pausar(int id)
        => Ok(await _objetivoAppService.AlterarStatusAsync(id, "pausar"));

    /// <summary>Retoma uma caixinha pausada.</summary>
    [HttpPatch("{id:int}/retomar")]
    public async Task<ActionResult<ObjetivoViewModel>> Retomar(int id)
        => Ok(await _objetivoAppService.AlterarStatusAsync(id, "retomar"));

    /// <summary>Cancela a caixinha definitivamente.</summary>
    [HttpPatch("{id:int}/cancelar")]
    public async Task<ActionResult<ObjetivoViewModel>> Cancelar(int id)
        => Ok(await _objetivoAppService.AlterarStatusAsync(id, "cancelar"));

    /// <summary>
    /// Envia a imagem de capa do objetivo (multipart/form-data).
    /// Aceita jpg, jpeg, png, gif, webp e bmp, com até 5MB.
    /// </summary>
    [HttpPost("{id:int}/capa")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<ActionResult<ObjetivoViewModel>> EnviarCapa(int id, IFormFile arquivo)
    {
        if (arquivo is null || arquivo.Length == 0)
        {
            return BadRequest(new
            {
                titulo = "Arquivo não enviado",
                erros = new[] { "Informe o campo 'arquivo' com a imagem da capa." }
            });
        }

        await using var stream = arquivo.OpenReadStream();
        var objetivo = await _objetivoAppService.DefinirCapaAsync(
            id, stream, arquivo.FileName, arquivo.Length);

        return Ok(objetivo);
    }

    /// <summary>Devolve a imagem de capa do objetivo.</summary>
    [HttpGet("{id:int}/capa")]
    [Produces("image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp")]
    public async Task<IActionResult> ObterCapa(int id)
    {
        var (conteudo, tipoConteudo) = await _objetivoAppService.ObterCapaAsync(id);
        return File(conteudo, tipoConteudo);
    }

    /// <summary>
    /// Simula a evolução da caixinha com base em aportes mensais e
    /// rendimentos. Sem aporte informado, usa o aporte ideal cadastrado.
    /// </summary>
    [HttpPost("{id:int}/simular")]
    public async Task<ActionResult<SimulacaoViewModel>> Simular(
        int id, [FromBody] SimularEvolucaoInputModel input)
    {
        await _validadorSimular.ValidateAndThrowAsync(input);
        return Ok(await _objetivoAppService.SimularAsync(id, input));
    }
}
