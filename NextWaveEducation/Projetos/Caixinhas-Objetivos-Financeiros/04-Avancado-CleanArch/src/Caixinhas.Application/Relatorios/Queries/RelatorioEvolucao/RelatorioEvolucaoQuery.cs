using Caixinhas.Application.Common;
using Caixinhas.Domain.Enums;
using Caixinhas.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Caixinhas.Application.Relatorios.Queries.RelatorioEvolucao;

/// <summary>
/// Relatório da evolução das caixinhas (requisito PLUS): situação de cada
/// objetivo e a movimentação consolidada mês a mês.
/// </summary>
public record RelatorioEvolucaoQuery(int MesesDeHistorico = 6) : IRequest<RelatorioEvolucaoDto>;

public class RelatorioEvolucaoQueryValidator : AbstractValidator<RelatorioEvolucaoQuery>
{
    public RelatorioEvolucaoQueryValidator()
    {
        RuleFor(r => r.MesesDeHistorico)
            .InclusiveBetween(1, 60)
            .WithMessage("O histórico deve ser de 1 a 60 meses.");
    }
}

public class RelatorioEvolucaoQueryHandler
    : IRequestHandler<RelatorioEvolucaoQuery, RelatorioEvolucaoDto>
{
    private readonly IObjetivoRepository _objetivoRepository;
    private readonly ITransacaoRepository _transacaoRepository;

    public RelatorioEvolucaoQueryHandler(
        IObjetivoRepository objetivoRepository,
        ITransacaoRepository transacaoRepository)
    {
        _objetivoRepository = objetivoRepository;
        _transacaoRepository = transacaoRepository;
    }

    public async Task<RelatorioEvolucaoDto> Handle(
        RelatorioEvolucaoQuery request,
        CancellationToken cancellationToken)
    {
        var objetivos = await _objetivoRepository.ObterTodosAsync(ct: cancellationToken);

        var itens = objetivos.Select(o =>
        {
            var transacoes = o.TransacoesAtivas.ToList();

            return new ItemRelatorioDto(
                o.Id,
                o.Titulo,
                o.Status.ToString(),
                o.ValorTotal,
                o.QuantidadeAlvo,
                o.PercentualConcluido,
                transacoes.Count(t => t.EhDeposito),
                transacoes.Count(t => !t.EhDeposito),
                transacoes.Where(t => t.EhDeposito).Sum(t => t.Quantidade),
                transacoes.Where(t => !t.EhDeposito).Sum(t => t.Quantidade),
                transacoes.Count == 0 ? null : transacoes.Max(t => t.DataTransacao),
                o.Prazo,
                o.DiasRestantes,
                o.PrazoVencido,
                o.AporteMensalNecessario);
        })
        .OrderByDescending(i => i.PercentualConcluido)
        .ThenBy(i => i.Titulo)
        .ToList();

        // Busca desde o começo para que o acumulado considere o que veio antes
        // da janela de histórico pedida.
        var todasAsTransacoes = await _transacaoRepository.ObterPorPeriodoAsync(
            new DateTime(1900, 1, 1), DateTime.Today, cancellationToken);

        var primeiroDiaDoMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var inicio = primeiroDiaDoMes.AddMonths(-(request.MesesDeHistorico - 1));

        var evolucao = new List<EvolucaoMensalDto>(request.MesesDeHistorico);

        for (var i = 0; i < request.MesesDeHistorico; i++)
        {
            var mes = inicio.AddMonths(i);
            var fimDoMes = mes.AddMonths(1).AddDays(-1);

            var doMes = todasAsTransacoes
                .Where(t => t.DataTransacao >= mes && t.DataTransacao <= fimDoMes)
                .ToList();

            var depositos = doMes.Where(t => t.EhDeposito).Sum(t => t.Quantidade);
            var saques = doMes.Where(t => !t.EhDeposito).Sum(t => t.Quantidade);

            // O acumulado é o saldo de tudo que aconteceu até o fim do mês.
            var acumulado = todasAsTransacoes
                .Where(t => t.DataTransacao <= fimDoMes)
                .Sum(t => t.Efeito);

            evolucao.Add(new EvolucaoMensalDto(
                $"{mes:MM/yyyy}", depositos, saques, depositos - saques, acumulado));
        }

        var patrimonio = objetivos.Sum(o => o.ValorTotal);
        var somaDasMetas = objetivos.Sum(o => o.QuantidadeAlvo);

        return new RelatorioEvolucaoDto(
            DateTime.Now,
            objetivos.Count,
            objetivos.Count(o => o.Status == StatusObjetivo.Completo),
            objetivos.Count(o => o.Status == StatusObjetivo.EmProgresso),
            objetivos.Count(o => o.Status == StatusObjetivo.Pausado),
            objetivos.Count(o => o.Status == StatusObjetivo.Cancelado),
            patrimonio,
            somaDasMetas,
            somaDasMetas == 0 ? 0 : Math.Min(100, Math.Round(patrimonio / somaDasMetas * 100, 2)),
            itens,
            evolucao);
    }
}
