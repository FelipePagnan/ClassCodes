using BancoSangue.Application.Common;
using BancoSangue.Domain.Exceptions;
using BancoSangue.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BancoSangue.Application.Doacoes.Commands.RegistrarDoacao;

/// <summary>
/// Registra a doação. A atualização do estoque acontece no handler do
/// evento de domínio DoacaoRegistradaEvent, despachado no commit da Unit of Work.
/// </summary>
public class RegistrarDoacaoCommandHandler : IRequestHandler<RegistrarDoacaoCommand, ResultadoDoacaoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegistrarDoacaoCommandHandler> _logger;

    public RegistrarDoacaoCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<RegistrarDoacaoCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ResultadoDoacaoDto> Handle(
        RegistrarDoacaoCommand request,
        CancellationToken cancellationToken)
    {
        var doador = await _unitOfWork.Doadores.ObterPorIdComDoacoesAsync(request.DoadorId, cancellationToken)
            ?? throw NaoEncontradoException.Para("Doador", request.DoadorId);

        var dataDoacao = request.DataDoacao ?? DateTime.Now;

        // A entidade aplica as regras de idade, peso, intervalo entre doações
        // e quantidade em ml, e dispara o evento de domínio.
        var doacao = doador.RegistrarDoacao(request.QuantidadeML, dataDoacao);

        // O doador já está sendo rastreado pelo contexto, então não é preciso
        // chamar Atualizar: basta adicionar a doação e confirmar.
        await _unitOfWork.Doacoes.AdicionarAsync(doacao, cancellationToken);

        // O commit salva a doação e despacha DoacaoRegistradaEvent,
        // cujo handler credita a quantidade no estoque.
        await _unitOfWork.CommitAsync(cancellationToken);

        var estoque = await _unitOfWork.EstoquesSangue
            .ObterPorTipoAsync(doador.TipoSanguineo, doador.FatorRh, cancellationToken)
            ?? throw new RegraNegocioException(
                $"Estoque do tipo {doador.TipoSanguineoCompleto} não encontrado.");

        var estoqueDto = EstoqueDto.De(estoque);

        _logger.LogInformation(
            "Doação {DoacaoId} de {QuantidadeML}ml registrada para o doador {DoadorId}. Estoque {Tipo}: {Quantidade}ml",
            doacao.Id, doacao.QuantidadeML, doador.Id, estoque.Descricao, estoque.QuantidadeML);

        return new ResultadoDoacaoDto(
            DoacaoDto.De(doacao),
            doador.NomeCompleto,
            estoqueDto,
            estoqueDto.Alerta);
    }
}
