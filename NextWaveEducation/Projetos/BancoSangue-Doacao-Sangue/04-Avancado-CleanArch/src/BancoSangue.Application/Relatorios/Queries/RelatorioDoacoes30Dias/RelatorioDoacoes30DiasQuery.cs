using BancoSangue.Application.Common;
using BancoSangue.Domain.Repositories;
using MediatR;

namespace BancoSangue.Application.Relatorios.Queries.RelatorioDoacoes30Dias;

/// <summary>
/// Relatório de doações nos últimos 30 dias com informações dos doadores (PLUS).
/// </summary>
public record RelatorioDoacoes30DiasQuery : IRequest<RelatorioDoacoes30DiasDto>;

public record RelatorioDoacoes30DiasDto(
    DateTime PeriodoInicio,
    DateTime PeriodoFim,
    int TotalDoacoes,
    int TotalColetadoML,
    int DoadoresDistintos,
    List<DoacaoComDoadorDto> Doacoes);

public class RelatorioDoacoes30DiasQueryHandler
    : IRequestHandler<RelatorioDoacoes30DiasQuery, RelatorioDoacoes30DiasDto>
{
    private readonly IDoacaoRepository _repository;

    public RelatorioDoacoes30DiasQueryHandler(IDoacaoRepository repository)
        => _repository = repository;

    public async Task<RelatorioDoacoes30DiasDto> Handle(
        RelatorioDoacoes30DiasQuery request,
        CancellationToken cancellationToken)
    {
        var inicio = DateTime.Now.AddDays(-30);
        var doacoes = await _repository.ObterApartirDeAsync(inicio, cancellationToken);
        var itens = doacoes.Select(DoacaoComDoadorDto.De).ToList();

        return new RelatorioDoacoes30DiasDto(
            inicio,
            DateTime.Now,
            itens.Count,
            itens.Sum(d => d.QuantidadeML),
            itens.Select(d => d.DoadorId).Distinct().Count(),
            itens);
    }
}
