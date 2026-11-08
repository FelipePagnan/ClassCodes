using System.Text.RegularExpressions;
using Livros.Api.Dados;
using Livros.Api.Dtos;
using Livros.Api.Entidades;
using Livros.Api.Excecoes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Livros.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly LivrosContext _context;

    public UsuariosController(LivrosContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioDto>>> Listar()
    {
        var usuarios = await _context.Usuarios.Include(u => u.Avaliacoes).ToListAsync();
        return Ok(usuarios.Select(ParaDto));
    }

    /// <summary>Detalhes do usuário com as avaliações feitas por ele.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioDetalheDto>> ObterPorId(int id)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Avaliacoes)
                .ThenInclude(a => a.Livro)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (usuario is null)
            throw new NaoEncontradoException($"Usuário com id {id} não encontrado.");

        return Ok(new UsuarioDetalheDto(
            usuario.Id, usuario.Nome, usuario.Email, usuario.TotalAvaliacoes,
            usuario.NotaMediaAtribuida, usuario.DataCriacao,
            usuario.Avaliacoes
                .OrderByDescending(a => a.DataFimLeitura)
                .Select(a => new AvaliacaoDoUsuarioDto(
                    a.Id, a.Nota, a.Descricao, a.IdLivro, a.Livro?.Titulo,
                    a.DataInicioLeitura, a.DataFimLeitura, a.DataCriacao))
                .ToList()));
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Cadastrar(CriarUsuarioDto dto)
    {
        ValidarDados(dto.Nome, dto.Email);

        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _context.Usuarios.AnyAsync(u => u.Email == email))
            throw new RegraNegocioException($"Já existe um usuário cadastrado com o e-mail '{email}'.");

        var usuario = new Usuario
        {
            Nome = dto.Nome.Trim(),
            Email = email,
            DataCriacao = DateTime.Now
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObterPorId), new { id = usuario.Id }, ParaDto(usuario));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UsuarioDto>> Atualizar(int id, AtualizarUsuarioDto dto)
    {
        var usuario = await _context.Usuarios.Include(u => u.Avaliacoes).FirstOrDefaultAsync(u => u.Id == id);
        if (usuario is null)
            throw new NaoEncontradoException($"Usuário com id {id} não encontrado.");

        ValidarDados(dto.Nome, dto.Email);

        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _context.Usuarios.AnyAsync(u => u.Email == email && u.Id != id))
            throw new RegraNegocioException($"Já existe outro usuário com o e-mail '{email}'.");

        usuario.Nome = dto.Nome.Trim();
        usuario.Email = email;

        await _context.SaveChangesAsync();
        return Ok(ParaDto(usuario));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        var usuario = await _context.Usuarios.Include(u => u.Avaliacoes).FirstOrDefaultAsync(u => u.Id == id);
        if (usuario is null)
            throw new NaoEncontradoException($"Usuário com id {id} não encontrado.");

        if (usuario.Avaliacoes.Count > 0)
            throw new RegraNegocioException(
                $"'{usuario.Nome}' possui {usuario.Avaliacoes.Count} avaliação(ões) e não pode ser removido.");

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static UsuarioDto ParaDto(Usuario u) => new(
        u.Id, u.Nome, u.Email, u.TotalAvaliacoes, u.NotaMediaAtribuida, u.DataCriacao);

    private static void ValidarDados(string nome, string email)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length < 3)
            throw new RegraNegocioException("O nome é obrigatório e deve ter ao menos 3 caracteres.");

        if (string.IsNullOrWhiteSpace(email) ||
            !Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$"))
            throw new RegraNegocioException($"O e-mail '{email}' é inválido.");
    }
}
