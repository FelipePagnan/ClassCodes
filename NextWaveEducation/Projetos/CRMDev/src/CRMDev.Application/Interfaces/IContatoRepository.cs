using CRMDev.Domain.Entities;

namespace CRMDev.Application.Interfaces;

public interface IContatoRepository
{
    Task<List<Contato>> ListarAsync(string? busca, CancellationToken ct = default);
    Task<Contato?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task AdicionarAsync(Contato contato, CancellationToken ct = default);
    void Remover(Contato contato);
    Task<bool> ExisteEmailAsync(string email, Guid? ignorarId = null, CancellationToken ct = default);
    Task<Tag?> ObterOuCriarTagAsync(string nome, CancellationToken ct = default);
    Task SalvarAsync(CancellationToken ct = default);
}
