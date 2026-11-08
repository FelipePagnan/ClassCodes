using CRMDev.Application.DTOs;
using CRMDev.Application.Interfaces;
using CRMDev.Domain.Entities;
using CRMDev.Domain.Enums;

namespace CRMDev.Application.Services;

public class OportunidadeService
{
    private readonly IOportunidadeRepository _repo;
    private readonly IContatoRepository _contatoRepo;

    public OportunidadeService(IOportunidadeRepository repo, IContatoRepository contatoRepo)
    {
        _repo = repo;
        _contatoRepo = contatoRepo;
    }

    public async Task<List<OportunidadeDto>> ListarAsync(EtapaOportunidade? etapa, StatusOportunidade? status, Guid? contatoId, CancellationToken ct = default)
    {
        var lista = await _repo.ListarAsync(etapa, status, contatoId, ct);
        return lista.Select(MapearParaDto).ToList();
    }

    public async Task<OportunidadeDto?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var op = await _repo.ObterPorIdAsync(id, ct);
        return op is null ? null : MapearParaDto(op);
    }

    public async Task<OportunidadeDto> CriarAsync(OportunidadeCriarDto dto, CancellationToken ct = default)
    {
        var contato = await _contatoRepo.ObterPorIdAsync(dto.ContatoId, ct)
            ?? throw new InvalidOperationException("Contato informado não foi encontrado.");

        var oportunidade = new Oportunidade
        {
            Titulo = dto.Titulo,
            Descricao = dto.Descricao,
            Prazo = dto.Prazo,
            Orcamento = dto.Orcamento,
            Escopo = dto.Escopo,
            SuporteIncluso = dto.SuporteIncluso,
            ContatoId = contato.Id,
            Contato = contato,
            Status = StatusOportunidade.EmAberto,
            Etapa = EtapaOportunidade.ContatoFeito
        };

        await _repo.AdicionarAsync(oportunidade, ct);
        await _repo.SalvarAsync(ct);

        return MapearParaDto(oportunidade);
    }

    public async Task<OportunidadeDto?> AtualizarAsync(Guid id, OportunidadeAtualizarDto dto, CancellationToken ct = default)
    {
        var op = await _repo.ObterPorIdAsync(id, ct);
        if (op is null) return null;

        op.Titulo = dto.Titulo;
        op.Descricao = dto.Descricao;
        op.Prazo = dto.Prazo;
        op.Orcamento = dto.Orcamento;
        op.Escopo = dto.Escopo;
        op.SuporteIncluso = dto.SuporteIncluso;
        op.AtualizadoEm = DateTime.UtcNow;

        await _repo.SalvarAsync(ct);
        return MapearParaDto(op);
    }

    public async Task<bool> RemoverAsync(Guid id, CancellationToken ct = default)
    {
        var op = await _repo.ObterPorIdAsync(id, ct);
        if (op is null) return false;

        _repo.Remover(op);
        await _repo.SalvarAsync(ct);
        return true;
    }

    public async Task<OportunidadeDto?> AlterarEtapaAsync(Guid id, EtapaOportunidade novaEtapa, CancellationToken ct = default)
    {
        var op = await _repo.ObterPorIdAsync(id, ct);
        if (op is null) return null;

        op.AlterarEtapa(novaEtapa);
        await _repo.SalvarAsync(ct);
        return MapearParaDto(op);
    }

    public async Task<OportunidadeDto?> MarcarGanhoAsync(Guid id, CancellationToken ct = default)
    {
        var op = await _repo.ObterPorIdAsync(id, ct);
        if (op is null) return null;

        op.MarcarGanho();
        await _repo.SalvarAsync(ct);
        return MapearParaDto(op);
    }

    public async Task<OportunidadeDto?> MarcarPerdidoAsync(Guid id, RazaoPerda razao, CancellationToken ct = default)
    {
        var op = await _repo.ObterPorIdAsync(id, ct);
        if (op is null) return null;

        op.MarcarPerdido(razao);
        await _repo.SalvarAsync(ct);
        return MapearParaDto(op);
    }

    private static OportunidadeDto MapearParaDto(Oportunidade o) => new(
        o.Id,
        o.Titulo,
        o.Descricao,
        o.Prazo,
        o.Orcamento,
        o.Escopo,
        o.SuporteIncluso,
        o.Status,
        o.RazaoPerda,
        o.Etapa,
        o.ContatoId,
        o.Contato?.Nome ?? string.Empty,
        o.CriadoEm,
        o.AtualizadoEm
    );
}
