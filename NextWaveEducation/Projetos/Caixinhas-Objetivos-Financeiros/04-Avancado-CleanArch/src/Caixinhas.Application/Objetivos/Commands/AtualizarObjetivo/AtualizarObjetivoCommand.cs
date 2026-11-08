using Caixinhas.Application.Common;
using Caixinhas.Domain.Exceptions;
using Caixinhas.Domain.Repositories;
using Caixinhas.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Caixinhas.Application.Objetivos.Commands.AtualizarObjetivo;

public record AtualizarObjetivoCommand : IRequest<ObjetivoDto>
{
    public int Id { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public decimal QuantidadeAlvo { get; init; }
    public DateTime? Prazo { get; init; }
    public decimal? QuantidadeIdealAporteMensal { get; init; }
}

public class AtualizarObjetivoCommandValidator : AbstractValidator<AtualizarObjetivoCommand>
{
    public AtualizarObjetivoCommandValidator()
    {
        RuleFor(o => o.Id).GreaterThan(0).WithMessage("O id do objetivo é obrigatório.");

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

public class AtualizarObjetivoCommandHandler : IRequestHandler<AtualizarObjetivoCommand, ObjetivoDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public AtualizarObjetivoCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<ObjetivoDto> Handle(
        AtualizarObjetivoCommand request,
        CancellationToken cancellationToken)
    {
        var objetivo = await _unitOfWork.Objetivos
            .ObterPorIdComTransacoesAsync(request.Id, cancellationToken)
            ?? throw NaoEncontradoException.Para("Objetivo financeiro", request.Id);

        if (await _unitOfWork.Objetivos.ExisteTituloAsync(
                request.Titulo, request.Id, cancellationToken))
            throw new RegraNegocioException($"Já existe outro objetivo chamado '{request.Titulo.Trim()}'.");

        objetivo.Atualizar(
            request.Titulo,
            request.QuantidadeAlvo,
            request.Prazo,
            request.QuantidadeIdealAporteMensal);

        await _unitOfWork.CommitAsync(cancellationToken);

        return ObjetivoDto.De(objetivo);
    }
}
