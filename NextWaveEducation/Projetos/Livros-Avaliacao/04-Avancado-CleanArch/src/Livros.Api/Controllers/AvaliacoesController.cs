using Livros.Api.Middlewares;
using Livros.Application.Avaliacoes.Commands.CadastrarAvaliacao;
using Livros.Application.Avaliacoes.Queries.ListarAvaliacoesDoLivro;
using Livros.Application.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Livros.Api.Controllers;

/// <summary>Cadastro e listagem de avaliações de um livro.</summary>
[ApiController]
[Route("api/livros/{idLivro:int}/avaliacoes")]
[Produces("application/json")]
public class AvaliacoesController : ControllerBase
{
    private readonly ISender _sender;

    public AvaliacoesController(ISender sender) => _sender = sender;

    /// <summary>Lista as avaliações de um livro.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<AvaliacaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Listar(int idLivro, CancellationToken ct)
        => Ok(await _sender.Send(new ListarAvaliacoesDoLivroQuery(idLivro), ct));

    /// <summary>
    /// Cadastra uma avaliação. A nota média do livro é recalculada por um
    /// evento de domínio e devolvida já atualizada na resposta.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ResultadoAvaliacaoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cadastrar(
        int idLivro, [FromBody] CadastrarAvaliacaoCommand command, CancellationToken ct)
    {
        var resultado = await _sender.Send(command with { IdLivro = idLivro }, ct);
        return CreatedAtAction(nameof(Listar), new { idLivro }, resultado);
    }
}
