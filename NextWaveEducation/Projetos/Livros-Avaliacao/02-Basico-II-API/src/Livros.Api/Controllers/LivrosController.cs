using Livros.Api.Dados;
using Livros.Api.Dtos;
using Livros.Api.Entidades;
using Livros.Api.Excecoes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Livros.Api.Controllers;

[ApiController]
[Route("api/livros")]
public class LivrosController : ControllerBase
{
    private readonly LivrosContext _context;

    public LivrosController(LivrosContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LivroDto>>> Listar([FromQuery] string? busca)
    {
        var query = _context.Livros.Include(l => l.Avaliacoes).AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            query = query.Where(l =>
                l.Titulo.Contains(busca) ||
                l.Autor.Contains(busca) ||
                l.Editora.Contains(busca));
        }

        var livros = await query.ToListAsync();
        return Ok(livros.Select(ParaDto).OrderByDescending(l => l.NotaMedia));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LivroDetalheDto>> ObterPorId(int id)
    {
        var livro = await _context.Livros
            .Include(l => l.Avaliacoes)
                .ThenInclude(a => a.Usuario)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (livro is null)
            throw new NaoEncontradoException($"Livro com id {id} não encontrado.");

        return Ok(new LivroDetalheDto(
            livro.Id, livro.Titulo, livro.Descricao, livro.Autor, livro.Editora, livro.ISBN,
            livro.Genero, livro.AnoDePublicacao, livro.QuantidadePaginas, livro.NotaMedia,
            livro.TemCapa, livro.DataCriacao,
            livro.Avaliacoes
                .OrderByDescending(a => a.DataCriacao)
                .Select(a => new AvaliacaoDto(
                    a.Id, a.Nota, a.Descricao, a.IdUsuario, a.Usuario?.Nome,
                    a.DataInicioLeitura, a.DataFimLeitura, a.DiasDeLeitura, a.DataCriacao))
                .ToList()));
    }

    [HttpPost]
    public async Task<ActionResult<LivroDto>> Cadastrar(CriarLivroDto dto)
    {
        ValidarDadosLivro(dto.Titulo, dto.ISBN, dto.Autor, dto.AnoDePublicacao, dto.QuantidadePaginas);

        var isbnNormalizado = NormalizarIsbn(dto.ISBN);

        // Regra de negócio: livros não podem ser cadastrados com o mesmo ISBN.
        var livrosExistentes = await _context.Livros.Select(l => new { l.Id, l.ISBN }).ToListAsync();
        if (livrosExistentes.Any(l => NormalizarIsbn(l.ISBN) == isbnNormalizado))
            throw new RegraNegocioException($"Já existe um livro cadastrado com o ISBN '{dto.ISBN}'.");

        var livro = new Livro
        {
            Titulo = dto.Titulo.Trim(),
            Descricao = dto.Descricao?.Trim() ?? string.Empty,
            ISBN = dto.ISBN.Trim(),
            Autor = dto.Autor.Trim(),
            Editora = dto.Editora?.Trim() ?? string.Empty,
            Genero = dto.Genero,
            AnoDePublicacao = dto.AnoDePublicacao,
            QuantidadePaginas = dto.QuantidadePaginas,
            DataCriacao = DateTime.Now,
            NotaMedia = 0
        };

        _context.Livros.Add(livro);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObterPorId), new { id = livro.Id }, ParaDto(livro));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<LivroDto>> Atualizar(int id, AtualizarLivroDto dto)
    {
        var livro = await _context.Livros.Include(l => l.Avaliacoes).FirstOrDefaultAsync(l => l.Id == id);
        if (livro is null)
            throw new NaoEncontradoException($"Livro com id {id} não encontrado.");

        ValidarDadosLivro(dto.Titulo, dto.ISBN, dto.Autor, dto.AnoDePublicacao, dto.QuantidadePaginas);

        var isbnNormalizado = NormalizarIsbn(dto.ISBN);
        var outros = await _context.Livros
            .Where(l => l.Id != id)
            .Select(l => new { l.ISBN })
            .ToListAsync();

        if (outros.Any(l => NormalizarIsbn(l.ISBN) == isbnNormalizado))
            throw new RegraNegocioException($"Já existe outro livro cadastrado com o ISBN '{dto.ISBN}'.");

        livro.Titulo = dto.Titulo.Trim();
        livro.Descricao = dto.Descricao?.Trim() ?? string.Empty;
        livro.ISBN = dto.ISBN.Trim();
        livro.Autor = dto.Autor.Trim();
        livro.Editora = dto.Editora?.Trim() ?? string.Empty;
        livro.Genero = dto.Genero;
        livro.AnoDePublicacao = dto.AnoDePublicacao;
        livro.QuantidadePaginas = dto.QuantidadePaginas;

        await _context.SaveChangesAsync();
        return Ok(ParaDto(livro));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        var livro = await _context.Livros.FindAsync(id);
        if (livro is null)
            throw new NaoEncontradoException($"Livro com id {id} não encontrado.");

        _context.Livros.Remove(livro);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static LivroDto ParaDto(Livro l) => new(
        l.Id, l.Titulo, l.Autor, l.Editora, l.ISBN, l.Genero, l.AnoDePublicacao,
        l.QuantidadePaginas, l.NotaMedia, l.Avaliacoes.Count, l.TemCapa, l.DataCriacao);

    private static void ValidarDadosLivro(
        string titulo, string isbn, string autor, int ano, int paginas)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new RegraNegocioException("O título é obrigatório.");

        if (string.IsNullOrWhiteSpace(isbn))
            throw new RegraNegocioException("O ISBN é obrigatório.");

        var digitos = NormalizarIsbn(isbn);
        if (digitos.Length is not (10 or 13))
            throw new RegraNegocioException($"O ISBN '{isbn}' deve ter 10 ou 13 dígitos.");

        if (string.IsNullOrWhiteSpace(autor))
            throw new RegraNegocioException("O autor é obrigatório.");

        if (ano < 1450 || ano > DateTime.Today.Year + 1)
            throw new RegraNegocioException($"O ano de publicação deve estar entre 1450 e {DateTime.Today.Year + 1}.");

        if (paginas <= 0)
            throw new RegraNegocioException("A quantidade de páginas deve ser maior que zero.");
    }

    private static string NormalizarIsbn(string isbn)
        => new(isbn.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());
}
