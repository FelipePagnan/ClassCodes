using CRMDev.Application.DTOs;
using CRMDev.Application.Interfaces;
using CRMDev.Domain.Entities;

namespace CRMDev.Application.Services;

public class ContatoService
{
    private readonly IContatoRepository _repo;

    public ContatoService(IContatoRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<ContatoDto>> ListarAsync(string? busca, CancellationToken ct = default)
    {
        var contatos = await _repo.ListarAsync(busca, ct);
        return contatos.Select(MapearParaDto).ToList();
    }

    public async Task<ContatoDto?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var contato = await _repo.ObterPorIdAsync(id, ct);
        return contato is null ? null : MapearParaDto(contato);
    }

    public async Task<ContatoDto> CriarAsync(ContatoCriarDto dto, CancellationToken ct = default)
    {
        if (await _repo.ExisteEmailAsync(dto.Email, ct: ct))
        {
            throw new InvalidOperationException("Já existe um contato cadastrado com este e-mail.");
        }

        var contato = new Contato
        {
            Nome = dto.Nome,
            Email = dto.Email,
            TelefoneCelular = dto.TelefoneCelular,
            AreaAtuacaoId = dto.AreaAtuacaoId,
            Cargo = dto.Cargo,
            Documento = dto.Documento,
            Endereco = dto.Endereco,
            Notas = dto.Notas
        };

        await _repo.AdicionarAsync(contato, ct);
        await _repo.SalvarAsync(ct);

        return MapearParaDto(contato);
    }

    public async Task<ContatoDto?> AtualizarAsync(Guid id, ContatoAtualizarDto dto, CancellationToken ct = default)
    {
        var contato = await _repo.ObterPorIdAsync(id, ct);
        if (contato is null) return null;

        if (await _repo.ExisteEmailAsync(dto.Email, ignorarId: id, ct: ct))
        {
            throw new InvalidOperationException("Já existe outro contato cadastrado com este e-mail.");
        }

        contato.Nome = dto.Nome;
        contato.Email = dto.Email;
        contato.TelefoneCelular = dto.TelefoneCelular;
        contato.AreaAtuacaoId = dto.AreaAtuacaoId;
        contato.Cargo = dto.Cargo;
        contato.Documento = dto.Documento;
        contato.Endereco = dto.Endereco;
        contato.Notas = dto.Notas;
        contato.AtualizadoEm = DateTime.UtcNow;

        await _repo.SalvarAsync(ct);
        return MapearParaDto(contato);
    }

    public async Task<bool> RemoverAsync(Guid id, CancellationToken ct = default)
    {
        var contato = await _repo.ObterPorIdAsync(id, ct);
        if (contato is null) return false;

        _repo.Remover(contato);
        await _repo.SalvarAsync(ct);
        return true;
    }

    public async Task<ContatoDto?> AdicionarTagAsync(Guid contatoId, string nomeTag, CancellationToken ct = default)
    {
        var contato = await _repo.ObterPorIdAsync(contatoId, ct);
        if (contato is null) return null;

        var tag = await _repo.ObterOuCriarTagAsync(nomeTag, ct);
        if (contato.ContatoTags.All(ct2 => ct2.TagId != tag!.Id))
        {
            contato.ContatoTags.Add(new ContatoTag { ContatoId = contato.Id, TagId = tag!.Id, Tag = tag });
        }

        await _repo.SalvarAsync(ct);
        return MapearParaDto(contato);
    }

    public async Task<ContatoDto?> RemoverTagAsync(Guid contatoId, string nomeTag, CancellationToken ct = default)
    {
        var contato = await _repo.ObterPorIdAsync(contatoId, ct);
        if (contato is null) return null;

        var vinculo = contato.ContatoTags.FirstOrDefault(c => string.Equals(c.Tag.Nome, nomeTag, StringComparison.OrdinalIgnoreCase));
        if (vinculo is not null)
        {
            contato.ContatoTags.Remove(vinculo);
        }

        await _repo.SalvarAsync(ct);
        return MapearParaDto(contato);
    }

    private static ContatoDto MapearParaDto(Contato c) => new(
        c.Id,
        c.Nome,
        c.Email,
        c.TelefoneCelular,
        c.AreaAtuacaoId,
        c.AreaAtuacao?.Nome,
        c.Cargo,
        c.Documento,
        c.Endereco,
        c.Notas,
        c.ContatoTags.Select(t => t.Tag.Nome).ToList(),
        c.CriadoEm,
        c.AtualizadoEm
    );
}
