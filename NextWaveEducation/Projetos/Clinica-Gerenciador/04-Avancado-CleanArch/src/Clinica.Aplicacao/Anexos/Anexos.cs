using Clinica.Aplicacao.Dtos;
using Clinica.Dominio.Entidades;
using Clinica.Dominio.Enums;
using Clinica.Dominio.Excecoes;
using Clinica.Dominio.Repositorios;
using FluentValidation;
using MediatR;

namespace Clinica.Aplicacao.Anexos;

// ================================= Comandos =================================

/// <summary>Atestado ou receita digitados na consulta.</summary>
public record AnexarDocumentoDeTexto(
    int AtendimentoId,
    TipoAnexo Tipo,
    string NomeArquivo,
    string Conteudo,
    string? Descricao) : IRequest<AnexoDto>;

/// <summary>Arquivo enviado (PDF digitalizado, foto de exame).</summary>
public record AnexarArquivo(
    int AtendimentoId,
    TipoAnexo Tipo,
    string NomeArquivo,
    string TipoConteudo,
    byte[] Conteudo,
    string? Descricao) : IRequest<AnexoDto>;

public record RemoverAnexo(int AtendimentoId, int AnexoId) : IRequest<Unit>;

// ================================= Consultas =================================

public record ListarAnexosDoAtendimento(int AtendimentoId, TipoAnexo? Tipo)
    : IRequest<List<AnexoDto>>;

public record ListarAnexosDoPaciente(int PacienteId) : IRequest<List<AnexoDto>>;

/// <summary>Devolve a entidade, porque o endpoint de download precisa dos bytes.</summary>
public record ObterAnexoParaDownload(int AtendimentoId, int AnexoId) : IRequest<Anexo>;

// ================================ Validadores ================================

public class AnexarDocumentoDeTextoValidator : AbstractValidator<AnexarDocumentoDeTexto>
{
    public AnexarDocumentoDeTextoValidator()
    {
        RuleFor(c => c.AtendimentoId).GreaterThan(0).WithMessage("Informe o atendimento.");

        RuleFor(c => c.Tipo)
            .IsInEnum().WithMessage("O tipo deve ser Atestado, Receita ou Evolucao.");

        RuleFor(c => c.NomeArquivo)
            .NotEmpty().WithMessage("O nome do arquivo é obrigatório.")
            .MaximumLength(255).WithMessage("O nome do arquivo deve ter no máximo 255 caracteres.");

        RuleFor(c => c.Conteudo)
            .NotEmpty().WithMessage("O conteúdo do documento é obrigatório.")
            .Must(conteudo =>
                System.Text.Encoding.UTF8.GetByteCount(conteudo) <= Anexo.TamanhoMaximoBytes)
            .WithMessage($"O anexo deve ter no máximo {Anexo.TamanhoMaximoBytes / 1024 / 1024}MB.");

        RuleFor(c => c.Descricao)
            .MaximumLength(4000).WithMessage("A descrição deve ter no máximo 4000 caracteres.");
    }
}

public class AnexarArquivoValidator : AbstractValidator<AnexarArquivo>
{
    public AnexarArquivoValidator()
    {
        RuleFor(c => c.AtendimentoId).GreaterThan(0).WithMessage("Informe o atendimento.");

        RuleFor(c => c.Tipo)
            .IsInEnum().WithMessage("O tipo deve ser Atestado, Receita ou Evolucao.");

        RuleFor(c => c.NomeArquivo)
            .NotEmpty().WithMessage("O nome do arquivo é obrigatório.")
            .MaximumLength(255).WithMessage("O nome do arquivo deve ter no máximo 255 caracteres.");

        RuleFor(c => c.Conteudo)
            .Must(conteudo => conteudo.Length > 0)
            .WithMessage("O arquivo está vazio.")
            .Must(conteudo => conteudo.Length <= Anexo.TamanhoMaximoBytes)
            .WithMessage($"O anexo deve ter no máximo {Anexo.TamanhoMaximoBytes / 1024 / 1024}MB.");
    }
}

// ================================= Handlers =================================

public class AnexarDocumentoDeTextoHandler : IRequestHandler<AnexarDocumentoDeTexto, AnexoDto>
{
    private readonly IAtendimentoRepositorio _repositorio;
    private readonly IUnitOfWork _unitOfWork;

    public AnexarDocumentoDeTextoHandler(
        IAtendimentoRepositorio repositorio, IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _unitOfWork = unitOfWork;
    }

    public async Task<AnexoDto> Handle(
        AnexarDocumentoDeTexto comando, CancellationToken cancellationToken)
    {
        var atendimento = await _repositorio.ObterComAnexosAsync(
                              comando.AtendimentoId, cancellationToken)
                          ?? throw new RegraNegocioException(
                              $"Atendimento {comando.AtendimentoId} não encontrado.");

        // A regra de que só atendimento realizado aceita anexo está na
        // entidade, em AnexarDocumento — junto com o evento DocumentoAnexado.
        var anexo = atendimento.AnexarDocumento(
            comando.Tipo,
            comando.NomeArquivo,
            "text/plain; charset=utf-8",
            System.Text.Encoding.UTF8.GetBytes(comando.Conteudo),
            comando.Descricao ?? comando.Conteudo);

        await _unitOfWork.CommitAsync(cancellationToken);

        return AnexoDto.De(anexo);
    }
}

public class AnexarArquivoHandler : IRequestHandler<AnexarArquivo, AnexoDto>
{
    private readonly IAtendimentoRepositorio _repositorio;
    private readonly IUnitOfWork _unitOfWork;

    public AnexarArquivoHandler(IAtendimentoRepositorio repositorio, IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _unitOfWork = unitOfWork;
    }

    public async Task<AnexoDto> Handle(
        AnexarArquivo comando, CancellationToken cancellationToken)
    {
        var atendimento = await _repositorio.ObterComAnexosAsync(
                              comando.AtendimentoId, cancellationToken)
                          ?? throw new RegraNegocioException(
                              $"Atendimento {comando.AtendimentoId} não encontrado.");

        var anexo = atendimento.AnexarDocumento(
            comando.Tipo,
            comando.NomeArquivo,
            comando.TipoConteudo,
            comando.Conteudo,
            comando.Descricao);

        await _unitOfWork.CommitAsync(cancellationToken);

        return AnexoDto.De(anexo);
    }
}

public class RemoverAnexoHandler : IRequestHandler<RemoverAnexo, Unit>
{
    private readonly IAtendimentoRepositorio _repositorio;
    private readonly IUnitOfWork _unitOfWork;

    public RemoverAnexoHandler(IAtendimentoRepositorio repositorio, IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RemoverAnexo comando, CancellationToken cancellationToken)
    {
        var atendimento = await _repositorio.ObterComAnexosAsync(
                              comando.AtendimentoId, cancellationToken)
                          ?? throw new RegraNegocioException(
                              $"Atendimento {comando.AtendimentoId} não encontrado.");

        var anexo = atendimento.Anexos.FirstOrDefault(x => x.Id == comando.AnexoId)
                    ?? throw new RegraNegocioException(
                        $"Anexo {comando.AnexoId} não encontrado no atendimento "
                        + $"{comando.AtendimentoId}.");

        // Remoção lógica: documento clínico não se apaga.
        anexo.Deletar();
        await _unitOfWork.CommitAsync(cancellationToken);

        return Unit.Value;
    }
}

public class ListarAnexosDoAtendimentoHandler
    : IRequestHandler<ListarAnexosDoAtendimento, List<AnexoDto>>
{
    private readonly IAtendimentoRepositorio _repositorio;

    public ListarAnexosDoAtendimentoHandler(IAtendimentoRepositorio repositorio)
        => _repositorio = repositorio;

    public async Task<List<AnexoDto>> Handle(
        ListarAnexosDoAtendimento consulta, CancellationToken cancellationToken)
    {
        var atendimento = await _repositorio.ObterComAnexosAsync(
                              consulta.AtendimentoId, cancellationToken)
                          ?? throw new RegraNegocioException(
                              $"Atendimento {consulta.AtendimentoId} não encontrado.");

        var anexos = atendimento.Anexos.AsEnumerable();

        if (consulta.Tipo is not null)
            anexos = anexos.Where(x => x.Tipo == consulta.Tipo);

        return anexos
            .OrderByDescending(x => x.DataCriacao)
            .Select(AnexoDto.De)
            .ToList();
    }
}

public class ListarAnexosDoPacienteHandler
    : IRequestHandler<ListarAnexosDoPaciente, List<AnexoDto>>
{
    private readonly IAtendimentoRepositorio _repositorio;

    public ListarAnexosDoPacienteHandler(IAtendimentoRepositorio repositorio)
        => _repositorio = repositorio;

    public async Task<List<AnexoDto>> Handle(
        ListarAnexosDoPaciente consulta, CancellationToken cancellationToken)
    {
        var anexos = await _repositorio.ListarAnexosDoPacienteAsync(
            consulta.PacienteId, cancellationToken);

        return anexos.Select(AnexoDto.De).ToList();
    }
}

public class ObterAnexoParaDownloadHandler : IRequestHandler<ObterAnexoParaDownload, Anexo>
{
    private readonly IAtendimentoRepositorio _repositorio;

    public ObterAnexoParaDownloadHandler(IAtendimentoRepositorio repositorio)
        => _repositorio = repositorio;

    public async Task<Anexo> Handle(
        ObterAnexoParaDownload consulta, CancellationToken cancellationToken)
    {
        var atendimento = await _repositorio.ObterComAnexosAsync(
                              consulta.AtendimentoId, cancellationToken)
                          ?? throw new RegraNegocioException(
                              $"Atendimento {consulta.AtendimentoId} não encontrado.");

        return atendimento.Anexos.FirstOrDefault(x => x.Id == consulta.AnexoId)
               ?? throw new RegraNegocioException(
                   $"Anexo {consulta.AnexoId} não encontrado no atendimento "
                   + $"{consulta.AtendimentoId}.");
    }
}
