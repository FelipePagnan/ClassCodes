using Caixinhas.Application.Common;
using Caixinhas.Domain.Exceptions;
using Caixinhas.Domain.Repositories;
using Caixinhas.Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Caixinhas.Application.Objetivos.Commands.CadastrarObjetivo;

/// <summary>Command (lado de escrita do CQRS) para criar uma caixinha.</summary>
public record CadastrarObjetivoCommand : IRequest<ObjetivoDto>
{
    public string Titulo { get; init; } = string.Empty;
    public decimal QuantidadeAlvo { get; init; }
    public DateTime? Prazo { get; init; }
    public decimal? QuantidadeIdealAporteMensal { get; init; }
}

public class CadastrarObjetivoCommandValidator : AbstractValidator<CadastrarObjetivoCommand>
{
    public CadastrarObjetivoCommandValidator()
    {
        RuleFor(o => o.Titulo)
            .NotEmpty().WithMessage("O título é obrigatório.")
            .MinimumLength(3).WithMessage("O título deve ter ao menos 3 caracteres.")
            .MaximumLength(120).WithMessage("O título deve ter no máximo 120 caracteres.");

        RuleFor(o => o.QuantidadeAlvo)
            .GreaterThan(0).WithMessage("A quantidade alvo deve ser maior que zero.")
            .Must(Dinheiro.EhValido)
            .WithMessage("A quantidade alvo deve ter no máximo 2 casas decimais e não pode ser negativa.");

        When(o => o.Prazo.HasValue, () =>
        {
            RuleFor(o => o.Prazo!.Value)
                .GreaterThan(_ => DateTime.Today)
                .WithMessage("O prazo, quando informado, deve ser uma data futura.");
        });

        When(o => o.QuantidadeIdealAporteMensal.HasValue, () =>
        {
            RuleFor(o => o.QuantidadeIdealAporteMensal!.Value)
                .GreaterThan(0).WithMessage("O aporte mensal ideal deve ser maior que zero.")
                .Must(Dinheiro.EhValido)
                .WithMessage("O aporte mensal ideal deve ter no máximo 2 casas decimais.");
        });
    }
}

public class CadastrarObjetivoCommandHandler : IRequestHandler<CadastrarObjetivoCommand, ObjetivoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CadastrarObjetivoCommandHandler> _logger;

    public CadastrarObjetivoCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CadastrarObjetivoCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ObjetivoDto> Handle(
        CadastrarObjetivoCommand request,
        CancellationToken cancellationToken)
    {
        if (await _unitOfWork.Objetivos.ExisteTituloAsync(request.Titulo, ct: cancellationToken))
            throw new RegraNegocioException($"Já existe um objetivo chamado '{request.Titulo.Trim()}'.");

        var objetivo = new Domain.Entities.ObjetivoFinanceiro(
            request.Titulo,
            request.QuantidadeAlvo,
            request.Prazo,
            request.QuantidadeIdealAporteMensal);

        await _unitOfWork.Objetivos.AdicionarAsync(objetivo, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Objetivo {ObjetivoId} '{Titulo}' criado com meta de R$ {Meta}.",
            objetivo.Id, objetivo.Titulo, objetivo.QuantidadeAlvo);

        return ObjetivoDto.De(objetivo);
    }
}
