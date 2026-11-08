using Caixinhas.Application.Common;
using Caixinhas.Domain.Exceptions;
using Caixinhas.Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Caixinhas.Application.Objetivos.Commands.AlterarStatus;

public enum AcaoStatus
{
    Pausar = 1,
    Retomar = 2,
    Cancelar = 3
}

public record AlterarStatusCommand(int Id, AcaoStatus Acao) : IRequest<ObjetivoDto>;

public class AlterarStatusCommandValidator : AbstractValidator<AlterarStatusCommand>
{
    public AlterarStatusCommandValidator()
    {
        RuleFor(c => c.Id).GreaterThan(0).WithMessage("O id do objetivo é obrigatório.");
        RuleFor(c => c.Acao).IsInEnum().WithMessage("A ação deve ser Pausar, Retomar ou Cancelar.");
    }
}

public class AlterarStatusCommandHandler : IRequestHandler<AlterarStatusCommand, ObjetivoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AlterarStatusCommandHandler> _logger;

    public AlterarStatusCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<AlterarStatusCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ObjetivoDto> Handle(
        AlterarStatusCommand request,
        CancellationToken cancellationToken)
    {
        var objetivo = await _unitOfWork.Objetivos
            .ObterPorIdComTransacoesAsync(request.Id, cancellationToken)
            ?? throw NaoEncontradoException.Para("Objetivo financeiro", request.Id);

        switch (request.Acao)
        {
            case AcaoStatus.Pausar: objetivo.Pausar(); break;
            case AcaoStatus.Retomar: objetivo.Retomar(); break;
            case AcaoStatus.Cancelar: objetivo.Cancelar(); break;
            default:
                throw new RegraNegocioException($"Ação '{request.Acao}' inválida.");
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Objetivo {ObjetivoId} agora está {Status}.", objetivo.Id, objetivo.Status);

        return ObjetivoDto.De(objetivo);
    }
}
