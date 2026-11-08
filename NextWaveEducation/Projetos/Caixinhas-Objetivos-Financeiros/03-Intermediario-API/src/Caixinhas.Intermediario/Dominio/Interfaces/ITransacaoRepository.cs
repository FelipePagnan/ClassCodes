using Caixinhas.Intermediario.Dominio.Entidades;

namespace Caixinhas.Intermediario.Dominio.Interfaces;

public interface ITransacaoRepository
{
    Task<List<Transacao>> ObterPorObjetivoAsync(int idObjetivo);
    Task<Transacao?> ObterPorIdAsync(int idObjetivo, int idTransacao);

    /// <summary>Transações de todas as caixinhas num intervalo de datas.</summary>
    Task<List<Transacao>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim);
}
