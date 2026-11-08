using BancoSangue.Application.Common;
using BancoSangue.Domain.Exceptions;
using BancoSangue.Domain.Repositories;
using MediatR;

namespace BancoSangue.Application.Doadores.Queries.ObterDoadorPorId;

/// <summary>Consulta de doador com seu histórico de doações.</summary>
public record ObterDoadorPorIdQuery(int Id) : IRequest<DoadorDetalheDto>;

public class ObterDoadorPorIdQueryHandler : IRequestHandler<ObterDoadorPorIdQuery, DoadorDetalheDto>
{
    private readonly IDoadorRepository _repository;

    public ObterDoadorPorIdQueryHandler(IDoadorRepository repository) => _repository = repository;

    public async Task<DoadorDetalheDto> Handle(ObterDoadorPorIdQuery request, CancellationToken cancellationToken)
    {
        var doador = await _repository.ObterPorIdComDoacoesAsync(request.Id, cancellationToken)
            ?? throw NaoEncontradoException.Para("Doador", request.Id);

        return DoadorDetalheDto.De(doador);
    }
}
