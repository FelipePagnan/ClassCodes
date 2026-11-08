using BancoSangue.Domain.Entities;

namespace BancoSangue.Domain.Repositories;

public interface IDoacaoRepository
{
    Task<List<Doacao>> ObterTodasAsync(CancellationToken cancellationToken = default);
    Task<List<Doacao>> ObterPorDoadorAsync(int doadorId, CancellationToken cancellationToken = default);
    Task<List<Doacao>> ObterApartirDeAsync(DateTime dataInicial, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Doacao doacao, CancellationToken cancellationToken = default);
}
