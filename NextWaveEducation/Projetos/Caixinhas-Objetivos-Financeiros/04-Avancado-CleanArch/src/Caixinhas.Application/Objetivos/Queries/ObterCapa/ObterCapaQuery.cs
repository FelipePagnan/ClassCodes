using Caixinhas.Application.Common;
using Caixinhas.Application.Common.Interfaces;
using Caixinhas.Domain.Exceptions;
using Caixinhas.Domain.Repositories;
using MediatR;

namespace Caixinhas.Application.Objetivos.Queries.ObterCapa;

/// <summary>Devolve os bytes da capa e o tipo de conteúdo detectado.</summary>
public record ObterCapaQuery(int IdObjetivo) : IRequest<CapaObjetivoDto>;

public class ObterCapaQueryHandler : IRequestHandler<ObterCapaQuery, CapaObjetivoDto>
{
    private readonly IObjetivoRepository _repository;
    private readonly IValidadorDeImagem _validadorDeImagem;

    public ObterCapaQueryHandler(
        IObjetivoRepository repository,
        IValidadorDeImagem validadorDeImagem)
    {
        _repository = repository;
        _validadorDeImagem = validadorDeImagem;
    }

    public async Task<CapaObjetivoDto> Handle(
        ObterCapaQuery request,
        CancellationToken cancellationToken)
    {
        var objetivo = await _repository.ObterPorIdAsync(request.IdObjetivo, cancellationToken)
            ?? throw NaoEncontradoException.Para("Objetivo financeiro", request.IdObjetivo);

        if (!objetivo.TemCapa)
            throw new NaoEncontradoException($"O objetivo '{objetivo.Titulo}' não possui capa.");

        var conteudo = objetivo.CapaObjetivo!;
        return new CapaObjetivoDto(conteudo, _validadorDeImagem.DetectarTipoConteudo(conteudo));
    }
}
