using Caixinhas.Domain.Entities;
using Caixinhas.Domain.Enums;

namespace Caixinhas.Domain.Repositories;

public interface IObjetivoRepository
{
    Task<List<ObjetivoFinanceiro>> ObterTodosAsync(
        StatusObjetivo? status = null, CancellationToken ct = default);

    Task<ObjetivoFinanceiro?> ObterPorIdAsync(int id, CancellationToken ct = default);

    Task<ObjetivoFinanceiro?> ObterPorIdComTransacoesAsync(int id, CancellationToken ct = default);

    Task<bool> ExisteTituloAsync(string titulo, int? ignorarId = null, CancellationToken ct = default);

    Task AdicionarAsync(ObjetivoFinanceiro objetivo, CancellationToken ct = default);

    void Atualizar(ObjetivoFinanceiro objetivo);
}
