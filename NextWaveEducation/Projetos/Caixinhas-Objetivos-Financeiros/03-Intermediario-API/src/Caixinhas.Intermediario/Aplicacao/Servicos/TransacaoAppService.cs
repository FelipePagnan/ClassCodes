using Caixinhas.Intermediario.Aplicacao.InputModels;
using Caixinhas.Intermediario.Aplicacao.ViewModels;
using Caixinhas.Intermediario.Dominio.Enums;
using Caixinhas.Intermediario.Dominio.Excecoes;
using Caixinhas.Intermediario.Dominio.Interfaces;

namespace Caixinhas.Intermediario.Aplicacao.Servicos;

public interface ITransacaoAppService
{
    Task<List<TransacaoViewModel>> ListarPorObjetivoAsync(int idObjetivo);
    Task<TransacaoViewModel> ObterPorIdAsync(int idObjetivo, int idTransacao);
    Task<ResultadoTransacaoViewModel> CadastrarAsync(int idObjetivo, CriarTransacaoInputModel input);
    Task RemoverAsync(int idObjetivo, int idTransacao);
    Task<List<LinhaExtratoViewModel>> GerarExtratoAsync(int idObjetivo);
}

public class TransacaoAppService : ITransacaoAppService
{
    private readonly IObjetivoRepository _objetivoRepository;
    private readonly ITransacaoRepository _transacaoRepository;
    private readonly ILogger<TransacaoAppService> _logger;

    public TransacaoAppService(
        IObjetivoRepository objetivoRepository,
        ITransacaoRepository transacaoRepository,
        ILogger<TransacaoAppService> logger)
    {
        _objetivoRepository = objetivoRepository;
        _transacaoRepository = transacaoRepository;
        _logger = logger;
    }

    public async Task<List<TransacaoViewModel>> ListarPorObjetivoAsync(int idObjetivo)
    {
        if (await _objetivoRepository.ObterPorIdAsync(idObjetivo) is null)
            throw NaoEncontradoException.Para("Objetivo financeiro", idObjetivo);

        var transacoes = await _transacaoRepository.ObterPorObjetivoAsync(idObjetivo);
        return transacoes.Select(TransacaoViewModel.De).ToList();
    }

    public async Task<TransacaoViewModel> ObterPorIdAsync(int idObjetivo, int idTransacao)
    {
        var transacao = await _transacaoRepository.ObterPorIdAsync(idObjetivo, idTransacao)
            ?? throw new NaoEncontradoException(
                $"Transação {idTransacao} não encontrada no objetivo {idObjetivo}.");

        return TransacaoViewModel.De(transacao);
    }

    /// <summary>
    /// Cadastra a transação. A entidade valida as regras e atualiza o
    /// ValorTotal; o repositório grava os dois numa transação de banco.
    /// </summary>
    public async Task<ResultadoTransacaoViewModel> CadastrarAsync(
        int idObjetivo, CriarTransacaoInputModel input)
    {
        var objetivo = await _objetivoRepository.ObterPorIdComTransacoesAsync(idObjetivo)
            ?? throw NaoEncontradoException.Para("Objetivo financeiro", idObjetivo);

        var statusAnterior = objetivo.Status;

        var transacao = objetivo.RegistrarTransacao(
            input.Quantidade,
            input.Tipo,
            input.DataTransacao ?? DateTime.Today);

        // A transação nova e o ValorTotal atualizado entram juntos no banco.
        await _objetivoRepository.SalvarComTransacaoAsync(objetivo);

        _logger.LogInformation(
            "{Tipo} de R$ {Quantidade} em '{Titulo}'. Novo saldo: R$ {Saldo}.",
            transacao.Tipo, transacao.Quantidade, objetivo.Titulo, objetivo.ValorTotal);

        string? mensagem = null;
        if (statusAnterior != StatusObjetivo.Completo && objetivo.Status == StatusObjetivo.Completo)
        {
            mensagem = $"Meta alcançada! '{objetivo.Titulo}' chegou a R$ {objetivo.ValorTotal:N2}.";
            _logger.LogInformation("{Mensagem}", mensagem);
        }

        return new ResultadoTransacaoViewModel
        {
            Transacao = TransacaoViewModel.De(transacao),
            TituloObjetivo = objetivo.Titulo,
            ValorTotalAtualizado = objetivo.ValorTotal,
            ValorRestante = objetivo.ValorRestante,
            PercentualConcluido = objetivo.PercentualConcluido,
            StatusObjetivo = objetivo.Status.ToString(),
            MetaAlcancada = objetivo.MetaAlcancada,
            Mensagem = mensagem
        };
    }

    /// <summary>Remoção lógica da transação, ajustando o ValorTotal.</summary>
    public async Task RemoverAsync(int idObjetivo, int idTransacao)
    {
        var objetivo = await _objetivoRepository.ObterPorIdComTransacoesAsync(idObjetivo)
            ?? throw NaoEncontradoException.Para("Objetivo financeiro", idObjetivo);

        objetivo.RemoverTransacao(idTransacao);

        // O soft delete e o ajuste do saldo entram juntos.
        await _objetivoRepository.SalvarComTransacaoAsync(objetivo);

        _logger.LogInformation(
            "Transação {TransacaoId} removida de '{Titulo}'. Novo saldo: R$ {Saldo}.",
            idTransacao, objetivo.Titulo, objetivo.ValorTotal);
    }

    /// <summary>Extrato do objetivo com o saldo corrido após cada movimentação.</summary>
    public async Task<List<LinhaExtratoViewModel>> GerarExtratoAsync(int idObjetivo)
    {
        var objetivo = await _objetivoRepository.ObterPorIdComTransacoesAsync(idObjetivo)
            ?? throw NaoEncontradoException.Para("Objetivo financeiro", idObjetivo);

        var saldo = 0m;

        return objetivo.Transacoes
            .Where(t => !t.EstaDeletado)
            .OrderBy(t => t.DataTransacao)
            .ThenBy(t => t.Id)
            .Select(t =>
            {
                saldo += t.Efeito;
                return new LinhaExtratoViewModel
                {
                    Id = t.Id,
                    Data = t.DataTransacao,
                    Tipo = t.Tipo.ToString(),
                    Quantidade = t.Quantidade,
                    SaldoApos = saldo
                };
            })
            .ToList();
    }
}
