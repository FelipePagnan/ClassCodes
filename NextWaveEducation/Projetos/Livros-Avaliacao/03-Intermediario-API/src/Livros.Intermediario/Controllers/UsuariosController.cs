using FluentValidation;
using Livros.Intermediario.Aplicacao.InputModels;
using Livros.Intermediario.Aplicacao.Servicos;
using Livros.Intermediario.Aplicacao.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Livros.Intermediario.Controllers;

[ApiController]
[Route("api/usuarios")]
[Produces("application/json")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioAppService _usuarioAppService;
    private readonly IValidator<CriarUsuarioInputModel> _validadorCriar;
    private readonly IValidator<AtualizarUsuarioInputModel> _validadorAtualizar;

    public UsuariosController(
        IUsuarioAppService usuarioAppService,
        IValidator<CriarUsuarioInputModel> validadorCriar,
        IValidator<AtualizarUsuarioInputModel> validadorAtualizar)
    {
        _usuarioAppService = usuarioAppService;
        _validadorCriar = validadorCriar;
        _validadorAtualizar = validadorAtualizar;
    }

    /// <summary>Lista os usuários cadastrados.</summary>
    [HttpGet]
    public async Task<ActionResult<List<UsuarioViewModel>>> Listar()
        => Ok(await _usuarioAppService.ListarAsync());

    /// <summary>Detalhes do usuário com as avaliações feitas por ele.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioDetalheViewModel>> ObterPorId(int id)
        => Ok(await _usuarioAppService.ObterPorIdAsync(id));

    /// <summary>Cadastra um novo usuário.</summary>
    [HttpPost]
    public async Task<ActionResult<UsuarioViewModel>> Cadastrar([FromBody] CriarUsuarioInputModel input)
    {
        await _validadorCriar.ValidateAndThrowAsync(input);

        var usuario = await _usuarioAppService.CadastrarAsync(input);
        return CreatedAtAction(nameof(ObterPorId), new { id = usuario.Id }, usuario);
    }

    /// <summary>Atualiza os dados de um usuário.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<UsuarioViewModel>> Atualizar(
        int id, [FromBody] AtualizarUsuarioInputModel input)
    {
        await _validadorAtualizar.ValidateAndThrowAsync(input);
        return Ok(await _usuarioAppService.AtualizarAsync(id, input));
    }

    /// <summary>Remove um usuário que ainda não possui avaliações.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        await _usuarioAppService.RemoverAsync(id);
        return NoContent();
    }
}
