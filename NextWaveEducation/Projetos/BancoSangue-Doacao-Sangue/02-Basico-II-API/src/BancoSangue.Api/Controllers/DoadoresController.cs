using BancoSangue.Api.Dados;
using BancoSangue.Api.Dtos;
using BancoSangue.Api.Entidades;
using BancoSangue.Api.Enums;
using BancoSangue.Api.Excecoes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BancoSangue.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoadoresController : ControllerBase
{
    private readonly BancoSangueContext _context;

    public DoadoresController(BancoSangueContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DoadorDto>>> Listar()
    {
        var doadores = await _context.Doadores.Include(d => d.Endereco).ToListAsync();
        return Ok(doadores.Select(ParaDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DoadorDto>> ObterPorId(int id)
    {
        var doador = await _context.Doadores.Include(d => d.Endereco).FirstOrDefaultAsync(d => d.Id == id);
        if (doador is null) return NotFound(new ErroDto($"Doador {id} não encontrado."));
        return Ok(ParaDto(doador));
    }

    [HttpGet("{id:int}/doacoes")]
    public async Task<ActionResult<IEnumerable<DoacaoDto>>> HistoricoDoacoes(int id)
    {
        var doador = await _context.Doadores.Include(d => d.Doacoes).FirstOrDefaultAsync(d => d.Id == id);
        if (doador is null) return NotFound(new ErroDto($"Doador {id} não encontrado."));

        var doacoes = doador.Doacoes
            .OrderByDescending(d => d.DataDoacao)
            .Select(d => new DoacaoDto(d.Id, d.DoadorId, doador.NomeCompleto, d.DataDoacao, d.QuantidadeML));

        return Ok(doacoes);
    }

    [HttpPost]
    public async Task<ActionResult<DoadorDto>> Cadastrar(CriarDoadorDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NomeCompleto))
            throw new RegraNegocioException("O nome completo é obrigatório.");

        if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains('@'))
            throw new RegraNegocioException("E-mail inválido.");

        if (await _context.Doadores.AnyAsync(d => d.Email == dto.Email))
            throw new RegraNegocioException($"Já existe um doador cadastrado com o e-mail '{dto.Email}'.");

        if (dto.Peso <= 0)
            throw new RegraNegocioException("O peso deve ser maior que zero.");

        var doador = new Doador
        {
            NomeCompleto = dto.NomeCompleto,
            Email = dto.Email,
            DataNascimento = dto.DataNascimento,
            Genero = dto.Genero,
            Peso = dto.Peso,
            TipoSanguineo = dto.TipoSanguineo,
            FatorRh = dto.FatorRh,
            Endereco = dto.Endereco is null
                ? null
                : new Endereco
                {
                    Logradouro = dto.Endereco.Logradouro,
                    Cidade = dto.Endereco.Cidade,
                    Estado = dto.Endereco.Estado,
                    CEP = dto.Endereco.CEP
                }
        };

        _context.Doadores.Add(doador);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObterPorId), new { id = doador.Id }, ParaDto(doador));
    }

    private static DoadorDto ParaDto(Doador d) => new(
        d.Id, d.NomeCompleto, d.Email, d.DataNascimento, d.Idade, d.Genero, d.Peso, d.TipoSanguineo, d.FatorRh,
        d.Endereco is null ? null : new EnderecoDto(d.Endereco.Logradouro, d.Endereco.Cidade, d.Endereco.Estado, d.Endereco.CEP));
}
