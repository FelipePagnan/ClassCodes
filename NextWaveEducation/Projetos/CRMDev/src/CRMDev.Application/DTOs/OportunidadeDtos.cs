using CRMDev.Domain.Enums;

namespace CRMDev.Application.DTOs;

public record OportunidadeCriarDto(
    string Titulo,
    string? Descricao,
    DateTime? Prazo,
    decimal? Orcamento,
    string? Escopo,
    bool SuporteIncluso,
    Guid ContatoId
);

public record OportunidadeAtualizarDto(
    string Titulo,
    string? Descricao,
    DateTime? Prazo,
    decimal? Orcamento,
    string? Escopo,
    bool SuporteIncluso
);

public record OportunidadeDto(
    Guid Id,
    string Titulo,
    string? Descricao,
    DateTime? Prazo,
    decimal? Orcamento,
    string? Escopo,
    bool SuporteIncluso,
    StatusOportunidade Status,
    RazaoPerda? RazaoPerda,
    EtapaOportunidade Etapa,
    Guid ContatoId,
    string ContatoNome,
    DateTime CriadoEm,
    DateTime? AtualizadoEm
);

public record AlterarEtapaDto(EtapaOportunidade NovaEtapa);

public record MarcarGanhoDto();

public record MarcarPerdidoDto(RazaoPerda Razao);
