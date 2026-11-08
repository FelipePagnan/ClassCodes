using Caixinhas.Domain.Entities;

namespace Caixinhas.Domain.Repositories;

public interface ITransacaoRepository
{
    Task<List<Transacao>> ObterPorObjetivoAsync(int idObjetivo, CancellationToken ct = default);

    Task<Transacao?> ObterPorIdAsync(int idObjetivo, int idTransacao, CancellationToken ct = default);

    /// <summary>Transações de todas as caixinhas ativas num intervalo de datas.</summary>
    Task<List<Transacao>> ObterPorPeriodoAsync(
        DateTime inicio, DateTime fim, CancellationToken ct = default);
}
