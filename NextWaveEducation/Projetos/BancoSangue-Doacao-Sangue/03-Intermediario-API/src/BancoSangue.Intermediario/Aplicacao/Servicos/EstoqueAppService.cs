using BancoSangue.Intermediario.Aplicacao.ViewModels;
using BancoSangue.Intermediario.Dominio.Interfaces;

namespace BancoSangue.Intermediario.Aplicacao.Servicos;

public interface IEstoqueAppService
{
    Task<List<EstoqueViewModel>> ListarAsync();
    Task<List<EstoqueViewModel>> ListarEstoqueBaixoAsync();
    Task<RelatorioEstoqueViewModel> RelatorioPorTipoAsync();
}

public class EstoqueAppService : IEstoqueAppService
{
    private readonly IEstoqueSangueRepository _repository;

    public EstoqueAppService(IEstoqueSangueRepository repository) => _repository = repository;

    public async Task<List<EstoqueViewModel>> ListarAsync()
    {
        var estoques = await _repository.ObterTodosAsync();
        return estoques.Select(EstoqueViewModel.De).OrderBy(e => e.TipoSanguineo).ToList();
    }

    public async Task<List<EstoqueViewModel>> ListarEstoqueBaixoAsync()
    {
        var estoques = await _repository.ObterComEstoqueBaixoAsync();
        return estoques.Select(EstoqueViewModel.De).OrderBy(e => e.TipoSanguineo).ToList();
    }

    /// <summary>
    /// Relatório da quantidade total de sangue por tipo disponível (PLUS).
    /// </summary>
    public async Task<RelatorioEstoqueViewModel> RelatorioPorTipoAsync()
    {
        var estoques = await _repository.ObterTodosAsync();
        var porTipo = estoques.Select(EstoqueViewModel.De).OrderBy(e => e.TipoSanguineo).ToList();

        return new RelatorioEstoqueViewModel
        {
            TotalGeralML = porTipo.Sum(e => e.QuantidadeML),
            QuantidadeTiposComEstoqueBaixo = porTipo.Count(e => e.EstoqueBaixo),
            PorTipo = porTipo
        };
    }
}
