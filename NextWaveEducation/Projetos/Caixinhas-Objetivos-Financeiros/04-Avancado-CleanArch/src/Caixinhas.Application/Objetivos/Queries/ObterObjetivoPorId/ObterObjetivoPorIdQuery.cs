using Caixinhas.Application.Common;
using Caixinhas.Domain.Exceptions;
using Caixinhas.Domain.Repositories;
using MediatR;

namespace Caixinhas.Application.Objetivos.Queries.ObterObjetivoPorId;

/// <summary>
/// Detalhes da caixinha. O ValorTotal vem direto da coluna persistida, sem
/// somar as transações na hora da consulta.
/// </summary>
public record ObterObjetivoPorIdQuery(int Id) : IRequest<ObjetivoDetalheDto>;

public class ObterObjetivoPorIdQueryHandler
    : IRequestHandler<ObterObjetivoPorIdQuery, ObjetivoDetalheDto>
{
    private readonly IObjetivoRepository _repository;

    public ObterObjetivoPorIdQueryHandler(IObjetivoRepository repository)
        => _repository = repository;

    public async Task<ObjetivoDetalheDto> Handle(
        ObterObjetivoPorIdQuery request,
        CancellationToken cancellationToken)
    {
        var objetivo = await _repository.ObterPorIdComTransacoesAsync(request.Id, cancellationToken)
            ?? throw NaoEncontradoException.Para("Objetivo financeiro", request.Id);

        return ObjetivoDetalheDto.De(objetivo);
    }
}
