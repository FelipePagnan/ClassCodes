using BancoSangue.Domain.Entities;
using BancoSangue.Domain.Enums;

namespace BancoSangue.Domain.Repositories;

public interface IEstoqueSangueRepository
{
    Task<List<EstoqueSangue>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<EstoqueSangue?> ObterPorTipoAsync(TipoSanguineo tipo, FatorRh fator, CancellationToken cancellationToken = default);
    Task<List<EstoqueSangue>> ObterAbaixoDoMinimoAsync(CancellationToken cancellationToken = default);
    void Atualizar(EstoqueSangue estoque);
}
