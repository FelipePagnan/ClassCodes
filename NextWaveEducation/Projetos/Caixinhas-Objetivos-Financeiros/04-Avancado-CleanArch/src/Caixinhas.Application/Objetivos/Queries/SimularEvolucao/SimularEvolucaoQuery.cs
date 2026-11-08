using Caixinhas.Application.Common;
using Caixinhas.Domain.Exceptions;
using Caixinhas.Domain.Repositories;
using Caixinhas.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Caixinhas.Application.Objetivos.Queries.SimularEvolucao;

/// <summary>
/// Simula a evolução da caixinha com base em aportes mensais e rendimentos
/// (requisito PLUS). Sem aporte informado, usa o aporte ideal cadastrado
/// ou o necessário para bater a meta no prazo.
/// </summary>
public record SimularEvolucaoQuery : IRequest<SimulacaoDto>
{
    public int IdObjetivo { get; init; }
    public decimal AporteMensal { get; init; }
    public int QuantidadeMeses { get; init; } = 12;

    /// <summary>Taxa de juros mensal em percentual (ex: 0.80 para 0,80% ao mês).</summary>
    public decimal TaxaJurosMensalPercentual { get; init; }
}

public class SimularEvolucaoQueryValidator : AbstractValidator<SimularEvolucaoQuery>
{
    public SimularEvolucaoQueryValidator()
    {
        RuleFor(s => s.IdObjetivo)
            .GreaterThan(0).WithMessage("O id do objetivo é obrigatório.");

        RuleFor(s => s.AporteMensal)
            .GreaterThanOrEqualTo(0).WithMessage("O aporte mensal não pode ser negativo.")
            .Must(Dinheiro.EhValido)
            .WithMessage("O aporte mensal deve ter no máximo 2 casas decimais.");

        RuleFor(s => s.QuantidadeMeses)
            .InclusiveBetween(1, 600)
            .WithMessage("A quantidade de meses deve estar entre 1 e 600.");

        RuleFor(s => s.TaxaJurosMensalPercentual)
            .InclusiveBetween(0m, 100m)
            .WithMessage("A taxa de juros mensal deve estar entre 0% e 100%.");
    }
}

public class SimularEvolucaoQueryHandler : IRequestHandler<SimularEvolucaoQuery, SimulacaoDto>
{
    private readonly IObjetivoRepository _repository;

    public SimularEvolucaoQueryHandler(IObjetivoRepository repository) => _repository = repository;

    public async Task<SimulacaoDto> Handle(
        SimularEvolucaoQuery request,
        CancellationToken cancellationToken)
    {
        var objetivo = await _repository
            .ObterPorIdComTransacoesAsync(request.IdObjetivo, cancellationToken)
            ?? throw NaoEncontradoException.Para("Objetivo financeiro", request.IdObjetivo);

        var aporte = request.AporteMensal > 0
            ? request.AporteMensal
            : objetivo.QuantidadeIdealAporteMensal ?? objetivo.AporteMensalNecessario ?? 0m;

        var resultado = objetivo.Simular(
            aporte, request.QuantidadeMeses, request.TaxaJurosMensalPercentual);

        return SimulacaoDto.De(resultado);
    }
}
