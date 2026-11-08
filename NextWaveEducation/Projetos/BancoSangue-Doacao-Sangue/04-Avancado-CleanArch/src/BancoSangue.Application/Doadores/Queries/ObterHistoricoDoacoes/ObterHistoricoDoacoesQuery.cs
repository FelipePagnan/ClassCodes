using BancoSangue.Application.Common;
using BancoSangue.Domain.Exceptions;
using BancoSangue.Domain.Repositories;
using MediatR;

namespace BancoSangue.Application.Doadores.Queries.ObterHistoricoDoacoes;

/// <summary>Consulta o histórico de doações de um doador específico.</summary>
public record ObterHistoricoDoacoesQuery(int DoadorId) : IRequest<List<DoacaoDto>>;

public class ObterHistoricoDoacoesQueryHandler
    : IRequestHandler<ObterHistoricoDoacoesQuery, List<DoacaoDto>>
{
    private readonly IDoadorRepository _doadorRepository;
    private readonly IDoacaoRepository _doacaoRepository;

    public ObterHistoricoDoacoesQueryHandler(
        IDoadorRepository doadorRepository,
        IDoacaoRepository doacaoRepository)
    {
        _doadorRepository = doadorRepository;
        _doacaoRepository = doacaoRepository;
    }

    public async Task<List<DoacaoDto>> Handle(
        ObterHistoricoDoacoesQuery request,
        CancellationToken cancellationToken)
    {
        var doador = await _doadorRepository.ObterPorIdAsync(request.DoadorId, cancellationToken)
            ?? throw NaoEncontradoException.Para("Doador", request.DoadorId);

        var doacoes = await _doacaoRepository.ObterPorDoadorAsync(doador.Id, cancellationToken);
        return doacoes.Select(DoacaoDto.De).ToList();
    }
}
