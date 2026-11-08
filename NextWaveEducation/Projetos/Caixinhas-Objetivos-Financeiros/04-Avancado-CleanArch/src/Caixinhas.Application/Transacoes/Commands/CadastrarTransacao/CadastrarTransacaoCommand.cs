using Caixinhas.Application.Common;
using Caixinhas.Domain.Enums;
using Caixinhas.Domain.Exceptions;
using Caixinhas.Domain.Repositories;
using Caixinhas.Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Caixinhas.Application.Transacoes.Commands.CadastrarTransacao;

public record CadastrarTransacaoCommand : IRequest<ResultadoTransacaoDto>
{
    public int IdObjetivo { get; init; }
    public decimal Quantidade { get; init; }
    public TipoTransacao Tipo { get; init; }

    /// <summary>Data da movimentação. Quando omitida, usa hoje.</summary>
    public DateTime? DataTransacao { get; init; }
}

public class CadastrarTransacaoCommandValidator : AbstractValidator<CadastrarTransacaoCommand>
{
    public CadastrarTransacaoCommandValidator()
    {
        RuleFor(t => t.IdObjetivo)
            .GreaterThan(0).WithMessage("O id do objetivo é obrigatório.");

        // Regra de negócio: a quantidade não pode ser negativa e deve ter
        // no máximo duas casas decimais de precisão.
        RuleFor(t => t.Quantidade)
            .GreaterThan(0).WithMessage("A quantidade da transação deve ser maior que zero.")
            .Must(Dinheiro.EhValido)
            .WithMessage("A quantidade deve ter no máximo 2 casas decimais e não pode ser negativa.");

        RuleFor(t => t.Tipo)
            .IsInEnum().WithMessage("O tipo de transação deve ser Deposito ou Saque.");

        When(t => t.DataTransacao.HasValue, () =>
        {
            RuleFor(t => t.DataTransacao!.Value)
                .LessThanOrEqualTo(_ => DateTime.Today.AddDays(1))
                .WithMessage("A data da transação não pode estar no futuro.");
        });
    }
}

/// <summary>
/// Cadastra a transação. A entidade atualiza o ValorTotal da caixinha e
/// dispara os eventos de domínio; a Unit of Work grava tudo dentro de uma
/// transação de banco, então o saldo nunca fica divergente do histórico.
/// </summary>
public class CadastrarTransacaoCommandHandler
    : IRequestHandler<CadastrarTransacaoCommand, ResultadoTransacaoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CadastrarTransacaoCommandHandler> _logger;

    public CadastrarTransacaoCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CadastrarTransacaoCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ResultadoTransacaoDto> Handle(
        CadastrarTransacaoCommand request,
        CancellationToken cancellationToken)
    {
        // Tudo dentro de uma transação de banco: a transação nova e o
        // ValorTotal atualizado entram juntos ou nenhum entra.
        return await _unitOfWork.ExecutarEmTransacaoAsync(async ct =>
        {
            var objetivo = await _unitOfWork.Objetivos
                .ObterPorIdComTransacoesAsync(request.IdObjetivo, ct)
                ?? throw NaoEncontradoException.Para("Objetivo financeiro", request.IdObjetivo);

            var saldoAnterior = objetivo.ValorTotal;
            var statusAnterior = objetivo.Status;

            // A entidade valida quantidade, tipo, data e saldo, atualiza o
            // ValorTotal e dispara TransacaoRegistradaEvent.
            var transacao = objetivo.RegistrarTransacao(
                request.Quantidade,
                request.Tipo,
                request.DataTransacao ?? DateTime.Today);

            await _unitOfWork.CommitAsync(ct);

            _logger.LogInformation(
                "{Tipo} de R$ {Quantidade} em '{Titulo}'. Saldo: R$ {Anterior} -> R$ {Atual}.",
                transacao.Tipo, transacao.Quantidade, objetivo.Titulo,
                saldoAnterior, objetivo.ValorTotal);

            string? mensagem = null;
            if (statusAnterior != StatusObjetivo.Completo
                && objetivo.Status == StatusObjetivo.Completo)
            {
                mensagem = $"Meta alcançada! '{objetivo.Titulo}' chegou a " +
                           $"R$ {objetivo.ValorTotal:N2} de R$ {objetivo.QuantidadeAlvo:N2}.";
            }

            return new ResultadoTransacaoDto(
                TransacaoDto.De(transacao),
                objetivo.Titulo,
                saldoAnterior,
                objetivo.ValorTotal,
                objetivo.ValorRestante,
                objetivo.PercentualConcluido,
                objetivo.Status.ToString(),
                objetivo.MetaAlcancada,
                mensagem);
        }, cancellationToken);
    }
}
