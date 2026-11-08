using BancoSangue.Intermediario.Dominio.Entidades;
using BancoSangue.Intermediario.Dominio.Enums;

namespace BancoSangue.Intermediario.Dominio.Interfaces;

public interface IEstoqueSangueRepository
{
    Task<List<EstoqueSangue>> ObterTodosAsync();
    Task<EstoqueSangue?> ObterPorTipoAsync(TipoSanguineo tipo, FatorRh fator);
    Task<List<EstoqueSangue>> ObterComEstoqueBaixoAsync();
    Task AtualizarAsync(EstoqueSangue estoque);
}
