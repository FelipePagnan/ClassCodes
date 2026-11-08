using FluentValidation;
using Livros.Intermediario.Aplicacao.InputModels;
using Livros.Intermediario.Aplicacao.Servicos;
using Livros.Intermediario.Aplicacao.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Livros.Intermediario.Controllers;

/// <summary>Cadastro e listagem de avaliações de um livro.</summary>
[ApiController]
[Route("api/livros/{idLivro:int}/avaliacoes")]
[Produces("application/json")]
public class AvaliacoesController : ControllerBase
{
    private readonly IAvaliacaoAppService _avaliacaoAppService;
    private readonly IValidator<CriarAvaliacaoInputModel> _validador;

    public AvaliacoesController(
        IAvaliacaoAppService avaliacaoAppService,
        IValidator<CriarAvaliacaoInputModel> validador)
    {
        _avaliacaoAppService = avaliacaoAppService;
        _validador = validador;
    }

    /// <summary>Lista as avaliações de um livro.</summary>
    [HttpGet]
    public async Task<ActionResult<List<AvaliacaoViewModel>>> Listar(int idLivro)
        => Ok(await _avaliacaoAppService.ListarPorLivroAsync(idLivro));

    /// <summary>
    /// Cadastra uma avaliação para o livro. A nota média do livro é
    /// recalculada e devolvida na resposta.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ResultadoAvaliacaoViewModel>> Cadastrar(
        int idLivro, [FromBody] CriarAvaliacaoInputModel input)
    {
        await _validador.ValidateAndThrowAsync(input);

        var resultado = await _avaliacaoAppService.CadastrarAsync(idLivro, input);
        return CreatedAtAction(nameof(Listar), new { idLivro }, resultado);
    }
}
