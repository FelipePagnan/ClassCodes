namespace EduPlatform.Api.DTOs;

public record CursoCreateDto(string Nome, string? Descricao, string? Cover);
public record CursoUpdateDto(string Nome, string? Descricao, string? Cover);

public record ModuloCreateDto(string Nome, string? Descricao, Guid IdCurso);
public record ModuloUpdateDto(string Nome, string? Descricao);

public record AulaCreateDto(string Nome, string? Descricao, string? LinkVideo, int Duracao, Guid IdModulo);
public record AulaUpdateDto(string Nome, string? Descricao, string? LinkVideo, int Duracao);

public record AssinaturaCreateDto(string Nome, int Duracao, decimal Valor, List<Guid> IdsCursos);
public record AssinaturaUpdateDto(string Nome, int Duracao, decimal Valor, List<Guid> IdsCursos);

public record ConectarUsuarioAssinaturaDto(Guid IdUsuario, Guid IdAssinatura);

public record ConcluirAulaDto(Guid IdUsuario, Guid IdAula, int Nota);

public record StatusCursoUsuarioDto(
    Guid IdCurso,
    string NomeCurso,
    int TempoTotalSegundos,
    int TempoConcluidoSegundos,
    int TempoRestanteSegundos,
    double PercentualConcluido,
    int TotalAulas,
    int AulasConcluidas);
