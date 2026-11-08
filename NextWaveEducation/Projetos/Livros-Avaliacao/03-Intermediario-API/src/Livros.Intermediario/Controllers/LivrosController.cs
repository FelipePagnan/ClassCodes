using FluentValidation;
using Livros.Intermediario.Aplicacao.InputModels;
using Livros.Intermediario.Aplicacao.Servicos;
using Livros.Intermediario.Aplicacao.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Livros.Intermediario.Controllers;

[ApiController]
[Route("api/livros")]
[Produces("application/json")]
public class LivrosController : ControllerBase
{
    private readonly ILivroAppService _livroAppService;
    private readonly IValidator<CriarLivroInputModel> _validadorCriar;
    private readonly IValidator<AtualizarLivroInputModel> _validadorAtualizar;
    private readonly IValidator<ImportarLivroInputModel> _validadorImportar;

    public LivrosController(
        ILivroAppService livroAppService,
        IValidator<CriarLivroInputModel> validadorCriar,
        IValidator<AtualizarLivroInputModel> validadorAtualizar,
        IValidator<ImportarLivroInputModel> validadorImportar)
    {
        _livroAppService = livroAppService;
        _validadorCriar = validadorCriar;
        _validadorAtualizar = validadorAtualizar;
        _validadorImportar = validadorImportar;
    }

    /// <summary>Lista os livros cadastrados, opcionalmente filtrando por título, autor ou editora.</summary>
    [HttpGet]
    public async Task<ActionResult<List<LivroViewModel>>> Listar([FromQuery] string? busca)
        => Ok(await _livroAppService.ListarAsync(busca));

    /// <summary>Detalhes do livro, incluindo as avaliações e a distribuição de notas.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<LivroDetalheViewModel>> ObterPorId(int id)
        => Ok(await _livroAppService.ObterPorIdAsync(id));

    /// <summary>Cadastra um novo livro.</summary>
    [HttpPost]
    public async Task<ActionResult<LivroViewModel>> Cadastrar([FromBody] CriarLivroInputModel input)
    {
        await _validadorCriar.ValidateAndThrowAsync(input);

        var livro = await _livroAppService.CadastrarAsync(input);
        return CreatedAtAction(nameof(ObterPorId), new { id = livro.Id }, livro);
    }

    /// <summary>Atualiza os dados de um livro.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<LivroViewModel>> Atualizar(
        int id, [FromBody] AtualizarLivroInputModel input)
    {
        await _validadorAtualizar.ValidateAndThrowAsync(input);
        return Ok(await _livroAppService.AtualizarAsync(id, input));
    }

    /// <summary>Remove um livro e suas avaliações.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        await _livroAppService.RemoverAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Envia a imagem de capa do livro (multipart/form-data).
    /// Aceita jpg, jpeg, png, gif, webp e bmp, com até 5MB.
    /// </summary>
    [HttpPost("{id:int}/capa")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<ActionResult<LivroViewModel>> EnviarCapa(int id, IFormFile arquivo)
    {
        if (arquivo is null)
            return BadRequest(new { titulo = "Arquivo não enviado", erros = new[] { "Informe o campo 'arquivo'." } });

        await using var stream = arquivo.OpenReadStream();
        var livro = await _livroAppService.DefinirCapaAsync(id, stream, arquivo.FileName, arquivo.Length);

        return Ok(livro);
    }

    /// <summary>Devolve a imagem de capa do livro.</summary>
    [HttpGet("{id:int}/capa")]
    [Produces("image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp")]
    public async Task<IActionResult> ObterCapa(int id)
    {
        var (conteudo, tipoConteudo) = await _livroAppService.ObterCapaAsync(id);
        return File(conteudo, tipoConteudo);
    }

    /// <summary>
    /// Consulta livros na API externa (Google Books). Informe 'isbn' ou 'termo'.
    /// </summary>
    [HttpGet("consulta-externa")]
    public async Task<ActionResult<List<LivroExternoViewModel>>> ConsultarApiExterna(
        [FromQuery] string? isbn,
        [FromQuery] string? termo,
        CancellationToken cancellationToken)
    {
        var porIsbn = !string.IsNullOrWhiteSpace(isbn);
        var consulta = porIsbn ? isbn! : termo ?? string.Empty;

        return Ok(await _livroAppService.ConsultarApiExternaAsync(consulta, porIsbn, cancellationToken));
    }

    /// <summary>
    /// Importa um livro da API externa pelo ISBN e cadastra na base local.
    /// </summary>
    [HttpPost("importar")]
    public async Task<ActionResult<LivroViewModel>> ImportarDaApiExterna(
        [FromBody] ImportarLivroInputModel input,
        CancellationToken cancellationToken)
    {
        await _validadorImportar.ValidateAndThrowAsync(input);

        var livro = await _livroAppService.ImportarDaApiExternaAsync(input, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = livro.Id }, livro);
    }
}
