using Caixinhas.Console.Entidades;
using Caixinhas.Console.Enums;
using Caixinhas.Console.Excecoes;

namespace Caixinhas.Console.Servicos;

/// <summary>
/// Gerenciamento de transações: cadastro, remoção (soft delete),
/// listagem e detalhes.
/// </summary>
public class TransacaoService
{
    private readonly ObjetivoService _objetivoService;

    public TransacaoService(ObjetivoService objetivoService)
        => _objetivoService = objetivoService;

    /// <summary>
    /// Cadastra a transação no objetivo. O ValorTotal do objetivo é
    /// atualizado dentro da própria entidade.
    /// </summary>
    public Transacao Cadastrar(
        int idObjetivo,
        decimal quantidade,
        TipoTransacao tipo,
        DateTime dataTransacao)
    {
        var objetivo = _objetivoService.ObterPorId(idObjetivo);

        return objetivo.RegistrarTransacao(
            quantidade, tipo, dataTransacao, _objetivoService.ProximoIdTransacao());
    }

    public IEnumerable<Transacao> ListarPorObjetivo(int idObjetivo)
    {
        var objetivo = _objetivoService.ObterPorId(idObjetivo);

        return objetivo.TransacoesAtivas
            .OrderByDescending(t => t.DataTransacao)
            .ThenByDescending(t => t.Id);
    }

    /// <summary>Lista as transações de todas as caixinhas ativas.</summary>
    public IEnumerable<Transacao> ListarTodas()
        => _objetivoService.ListarTodos()
            .SelectMany(o => o.TransacoesAtivas)
            .OrderByDescending(t => t.DataTransacao)
            .ThenByDescending(t => t.Id);

    public Transacao ObterPorId(int idTransacao)
        => ListarTodas().FirstOrDefault(t => t.Id == idTransacao)
           ?? throw new RegraNegocioException($"Transação {idTransacao} não encontrada.");

    /// <summary>
    /// Remoção lógica da transação. O ValorTotal do objetivo é ajustado.
    /// </summary>
    public void Remover(int idTransacao)
    {
        var transacao = ObterPorId(idTransacao);
        var objetivo = transacao.ObjetivoFinanceiro
            ?? _objetivoService.ObterPorId(transacao.ObjetivoFinanceiroId);

        objetivo.RemoverTransacao(idTransacao);
    }

    /// <summary>Extrato do objetivo com o saldo corrido após cada movimentação.</summary>
    public List<LinhaExtrato> GerarExtrato(int idObjetivo)
    {
        var objetivo = _objetivoService.ObterPorId(idObjetivo);
        var saldo = 0m;

        return objetivo.TransacoesAtivas
            .OrderBy(t => t.DataTransacao)
            .ThenBy(t => t.Id)
            .Select(t =>
            {
                saldo += t.Efeito;
                return new LinhaExtrato(
                    t.Id, t.DataTransacao, t.Tipo.ToString(), t.Quantidade, saldo);
            })
            .ToList();
    }
}

public record LinhaExtrato(
    int Id,
    DateTime Data,
    string Tipo,
    decimal Quantidade,
    decimal SaldoApos);
