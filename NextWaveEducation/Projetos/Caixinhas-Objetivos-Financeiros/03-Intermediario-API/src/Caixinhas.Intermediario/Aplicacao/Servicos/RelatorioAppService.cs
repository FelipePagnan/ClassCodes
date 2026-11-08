using Caixinhas.Intermediario.Aplicacao.ViewModels;
using Caixinhas.Intermediario.Dominio.Enums;
using Caixinhas.Intermediario.Dominio.Excecoes;
using Caixinhas.Intermediario.Dominio.Interfaces;

namespace Caixinhas.Intermediario.Aplicacao.Servicos;

public interface IRelatorioAppService
{
    Task<RelatorioEvolucaoViewModel> GerarEvolucaoAsync(int mesesDeHistorico);
}

public class RelatorioAppService : IRelatorioAppService
{
    private readonly IObjetivoRepository _objetivoRepository;
    private readonly ITransacaoRepository _transacaoRepository;

    public RelatorioAppService(
        IObjetivoRepository objetivoRepository,
        ITransacaoRepository transacaoRepository)
    {
        _objetivoRepository = objetivoRepository;
        _transacaoRepository = transacaoRepository;
    }

    /// <summary>
    /// Relatório da evolução das caixinhas (requisito PLUS): situação de cada
    /// objetivo e a movimentação consolidada mês a mês.
    /// </summary>
    public async Task<RelatorioEvolucaoViewModel> GerarEvolucaoAsync(int mesesDeHistorico)
    {
        if (mesesDeHistorico is < 1 or > 60)
            throw new RegraNegocioException("O histórico deve ser de 1 a 60 meses.");

        var objetivos = await _objetivoRepository.ObterTodosAsync();

        var itens = objetivos.Select(o =>
        {
            var transacoes = o.Transacoes.Where(t => !t.EstaDeletado).ToList();

            return new ItemRelatorioViewModel
            {
                Id = o.Id,
                Titulo = o.Titulo,
                Status = o.Status.ToString(),
                ValorTotal = o.ValorTotal,
                QuantidadeAlvo = o.QuantidadeAlvo,
                PercentualConcluido = o.PercentualConcluido,
                QuantidadeDepositos = transacoes.Count(t => t.EhDeposito),
                QuantidadeSaques = transacoes.Count(t => !t.EhDeposito),
                TotalDepositado = transacoes.Where(t => t.EhDeposito).Sum(t => t.Quantidade),
                TotalSacado = transacoes.Where(t => !t.EhDeposito).Sum(t => t.Quantidade),
                UltimaMovimentacao = transacoes.Count == 0
                    ? null
                    : transacoes.Max(t => t.DataTransacao),
                Prazo = o.Prazo,
                DiasRestantes = o.DiasRestantes,
                PrazoVencido = o.PrazoVencido,
                AporteMensalNecessario = o.AporteMensalNecessario
            };
        })
        .OrderByDescending(i => i.PercentualConcluido)
        .ThenBy(i => i.Titulo)
        .ToList();

        // Movimentação consolidada mês a mês, somando todas as caixinhas.
        var primeiroDiaDoMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var inicioDoHistorico = primeiroDiaDoMes.AddMonths(-(mesesDeHistorico - 1));

        // Busca desde o começo para que o acumulado considere o que veio antes.
        var todasAsTransacoes = await _transacaoRepository.ObterPorPeriodoAsync(
            new DateTime(1900, 1, 1), DateTime.Today);

        var evolucao = new List<EvolucaoMensalViewModel>(mesesDeHistorico);

        for (var i = 0; i < mesesDeHistorico; i++)
        {
            var mes = inicioDoHistorico.AddMonths(i);
            var fimDoMes = mes.AddMonths(1).AddDays(-1);

            var doMes = todasAsTransacoes
                .Where(t => t.DataTransacao >= mes && t.DataTransacao <= fimDoMes)
                .ToList();

            var depositos = doMes.Where(t => t.EhDeposito).Sum(t => t.Quantidade);
            var saques = doMes.Where(t => !t.EhDeposito).Sum(t => t.Quantidade);

            // O acumulado é o saldo de tudo que aconteceu até o fim daquele mês.
            var acumulado = todasAsTransacoes
                .Where(t => t.DataTransacao <= fimDoMes)
                .Sum(t => t.Efeito);

            evolucao.Add(new EvolucaoMensalViewModel
            {
                Mes = $"{mes:MM/yyyy}",
                Depositos = depositos,
                Saques = saques,
                Liquido = depositos - saques,
                SaldoAcumulado = acumulado
            });
        }

        var patrimonio = objetivos.Sum(o => o.ValorTotal);
        var somaDasMetas = objetivos.Sum(o => o.QuantidadeAlvo);

        return new RelatorioEvolucaoViewModel
        {
            GeradoEm = DateTime.Now,
            TotalObjetivos = objetivos.Count,
            ObjetivosCompletos = objetivos.Count(o => o.Status == StatusObjetivo.Completo),
            ObjetivosEmProgresso = objetivos.Count(o => o.Status == StatusObjetivo.EmProgresso),
            ObjetivosPausados = objetivos.Count(o => o.Status == StatusObjetivo.Pausado),
            ObjetivosCancelados = objetivos.Count(o => o.Status == StatusObjetivo.Cancelado),
            PatrimonioTotal = patrimonio,
            SomaDasMetas = somaDasMetas,
            PercentualGeralConcluido = somaDasMetas == 0
                ? 0
                : Math.Min(100, Math.Round(patrimonio / somaDasMetas * 100, 2)),
            Objetivos = itens,
            EvolucaoMensal = evolucao
        };
    }
}
