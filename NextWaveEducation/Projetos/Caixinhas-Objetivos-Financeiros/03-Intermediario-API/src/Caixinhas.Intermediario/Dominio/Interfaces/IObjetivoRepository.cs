using Caixinhas.Intermediario.Dominio.Entidades;
using Caixinhas.Intermediario.Dominio.Enums;

namespace Caixinhas.Intermediario.Dominio.Interfaces;

public interface IObjetivoRepository
{
    Task<List<ObjetivoFinanceiro>> ObterTodosAsync(StatusObjetivo? status = null);
    Task<ObjetivoFinanceiro?> ObterPorIdAsync(int id);
    Task<ObjetivoFinanceiro?> ObterPorIdComTransacoesAsync(int id);
    Task<bool> ExisteTituloAsync(string titulo, int? ignorarId = null);
    Task AdicionarAsync(ObjetivoFinanceiro objetivo);

    /// <summary>
    /// Persiste as alterações do objetivo e das transações dele numa única
    /// transação de banco, para que o ValorTotal nunca fique inconsistente
    /// com as transações gravadas.
    /// </summary>
    Task SalvarComTransacaoAsync(ObjetivoFinanceiro objetivo);

    Task SalvarAsync();
}
