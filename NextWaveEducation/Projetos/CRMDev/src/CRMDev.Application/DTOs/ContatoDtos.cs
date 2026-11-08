namespace CRMDev.Application.DTOs;

public record ContatoCriarDto(
    string Nome,
    string Email,
    string? TelefoneCelular,
    Guid? AreaAtuacaoId,
    string? Cargo,
    string? Documento,
    string? Endereco,
    string? Notas
);

public record ContatoAtualizarDto(
    string Nome,
    string Email,
    string? TelefoneCelular,
    Guid? AreaAtuacaoId,
    string? Cargo,
    string? Documento,
    string? Endereco,
    string? Notas
);

public record ContatoDto(
    Guid Id,
    string Nome,
    string Email,
    string? TelefoneCelular,
    Guid? AreaAtuacaoId,
    string? AreaAtuacaoNome,
    string? Cargo,
    string? Documento,
    string? Endereco,
    string? Notas,
    List<string> Tags,
    DateTime CriadoEm,
    DateTime? AtualizadoEm
);

public record TagAdicionarDto(string Nome);
