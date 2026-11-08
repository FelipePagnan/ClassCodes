using CRMDev.Domain.Entities;
using CRMDev.Domain.Enums;

namespace CRMDev.Application.Interfaces;

public interface IOportunidadeRepository
{
    Task<List<Oportunidade>> ListarAsync(EtapaOportunidade? etapa, StatusOportunidade? status, Guid? contatoId, CancellationToken ct = default);
    Task<Oportunidade?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task AdicionarAsync(Oportunidade oportunidade, CancellationToken ct = default);
    void Remover(Oportunidade oportunidade);
    Task SalvarAsync(CancellationToken ct = default);
}
