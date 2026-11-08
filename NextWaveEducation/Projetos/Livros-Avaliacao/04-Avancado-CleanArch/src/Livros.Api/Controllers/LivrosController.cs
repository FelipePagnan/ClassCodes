using Livros.Api.Middlewares;
using Livros.Application.Common;
using Livros.Application.Livros.Commands.AtualizarLivro;
using Livros.Application.Livros.Commands.CadastrarLivro;
using Livros.Application.Livros.Commands.EnviarCapa;
using Livros.Application.Livros.Commands.ImportarLivroExterno;
using Livros.Application.Livros.Commands.RemoverLivro;
using Livros.Application.Livros.Queries.ConsultarApiExterna;
using Livros.Application.Livros.Queries.ListarLivros;
using Livros.Application.Livros.Queries.ObterCapa;
using Livros.Application.Livros.Queries.ObterLivroPorId;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Livros.Api.Controllers;

[ApiController]
[Route("api/livros")]
[Produces("application/json")]
public class LivrosController : ControllerBase
{
    private readonly ISender _sender;

    public LivrosController(ISender sender) => _sender = sender;

    /// <summary>Lista os livros, opcionalmente filtrando por título, autor ou editora.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<LivroDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] string? busca, CancellationToken ct)
        => Ok(await _sender.Send(new ListarLivrosQuery(busca), ct));

    /// <summary>Detalhes do livro com avaliações e distribuição de notas.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(LivroDetalheDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
        => Ok(await _sender.Send(new ObterLivroPorIdQuery(id), ct));

    /// <summary>Cadastra um novo livro.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(LivroDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cadastrar(
        [FromBody] CadastrarLivroCommand command, CancellationToken ct)
    {
        var livro = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = livro.Id }, livro);
    }

    /// <summary>Atualiza os dados de um livro.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(LivroDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(
        int id, [FromBody] AtualizarLivroCommand command, CancellationToken ct)
        => Ok(await _sender.Send(command with { Id = id }, ct));

    /// <summary>Remove um livro e suas avaliações.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int id, CancellationToken ct)
    {
        await _sender.Send(new RemoverLivroCommand(id), ct);
        return NoContent();
    }

    /// <summary>
    /// Envia a imagem de capa do livro (multipart/form-data).
    /// Aceita jpg, jpeg, png, gif, webp e bmp, com até 5MB.
    /// </summary>
    [HttpPost("{id:int}/capa")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [ProducesResponseType(typeof(LivroDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnviarCapa(int id, IFormFile arquivo, CancellationToken ct)
    {
        if (arquivo is null || arquivo.Length == 0)
        {
            return BadRequest(new RespostaErro
            {
                Titulo = "Arquivo não enviado",
                Status = StatusCodes.Status400BadRequest,
                Caminho = Request.Path,
                Erros = new List<string> { "Informe o campo 'arquivo' com a imagem da capa." }
            });
        }

        using var memoria = new MemoryStream();
        await arquivo.CopyToAsync(memoria, ct);

        var livro = await _sender.Send(
            new EnviarCapaCommand(id, memoria.ToArray(), arquivo.FileName), ct);

        return Ok(livro);
    }

    /// <summary>Devolve a imagem de capa do livro.</summary>
    [HttpGet("{id:int}/capa")]
    [Produces("image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterCapa(int id, CancellationToken ct)
    {
        var capa = await _sender.Send(new ObterCapaQuery(id), ct);
        return File(capa.Conteudo, capa.TipoConteudo);
    }

    /// <summary>
    /// Consulta livros na API externa (Google Books). Informe 'isbn' ou 'termo'.
    /// </summary>
    [HttpGet("consulta-externa")]
    [ProducesResponseType(typeof(List<LivroExternoResultadoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConsultarApiExterna(
        [FromQuery] string? isbn,
        [FromQuery] string? termo,
        CancellationToken ct)
        => Ok(await _sender.Send(new ConsultarApiExternaQuery(isbn, termo), ct));

    /// <summary>
    /// Importa um livro da API externa pelo ISBN e cadastra na base local.
    /// </summary>
    [HttpPost("importar")]
    [ProducesResponseType(typeof(LivroDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Importar(
        [FromBody] ImportarLivroExternoCommand command, CancellationToken ct)
    {
        var livro = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = livro.Id }, livro);
    }
}
