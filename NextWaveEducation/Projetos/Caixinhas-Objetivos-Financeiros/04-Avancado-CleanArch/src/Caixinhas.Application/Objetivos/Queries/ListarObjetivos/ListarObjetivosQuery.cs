using Caixinhas.Application.Common;
using Caixinhas.Domain.Enums;
using Caixinhas.Domain.Repositories;
using MediatR;

namespace Caixinhas.Application.Objetivos.Queries.ListarObjetivos;

/// <summary>Query (lado de leitura do CQRS) que lista as caixinhas ativas.</summary>
public record ListarObjetivosQuery(StatusObjetivo? Status = null) : IRequest<List<ObjetivoDto>>;

public class ListarObjetivosQueryHandler : IRequestHandler<ListarObjetivosQuery, List<ObjetivoDto>>
{
    private readonly IObjetivoRepository _repository;

    public ListarObjetivosQueryHandler(IObjetivoRepository repository) => _repository = repository;

    public async Task<List<ObjetivoDto>> Handle(
        ListarObjetivosQuery request,
        CancellationToken cancellationToken)
    {
        var objetivos = await _repository.ObterTodosAsync(request.Status, cancellationToken);
        return objetivos.Select(ObjetivoDto.De).ToList();
    }
}
