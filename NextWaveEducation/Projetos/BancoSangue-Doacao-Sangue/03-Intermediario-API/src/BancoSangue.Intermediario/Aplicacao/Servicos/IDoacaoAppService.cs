using BancoSangue.Intermediario.Aplicacao.InputModels;
using BancoSangue.Intermediario.Aplicacao.ViewModels;

namespace BancoSangue.Intermediario.Aplicacao.Servicos;

public interface IDoacaoAppService
{
    Task<List<DoacaoComDoadorViewModel>> ListarAsync();
    Task<ResultadoDoacaoViewModel> RegistrarAsync(CriarDoacaoInputModel input);
    Task<List<DoacaoComDoadorViewModel>> RelatorioUltimos30DiasAsync();
}

public class ResultadoDoacaoViewModel
{
    public DoacaoViewModel Doacao { get; set; } = new();
    public EstoqueViewModel EstoqueAtualizado { get; set; } = new();
    public string? AlertaEstoque { get; set; }
}
