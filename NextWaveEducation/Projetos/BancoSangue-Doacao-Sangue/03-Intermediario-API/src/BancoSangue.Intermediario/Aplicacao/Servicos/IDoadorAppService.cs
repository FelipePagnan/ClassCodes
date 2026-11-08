using BancoSangue.Intermediario.Aplicacao.InputModels;
using BancoSangue.Intermediario.Aplicacao.ViewModels;

namespace BancoSangue.Intermediario.Aplicacao.Servicos;

public interface IDoadorAppService
{
    Task<List<DoadorViewModel>> ListarAsync();
    Task<DoadorDetalheViewModel> ObterPorIdAsync(int id);
    Task<List<DoacaoViewModel>> ObterHistoricoDoacoesAsync(int doadorId);
    Task<DoadorViewModel> CadastrarAsync(CriarDoadorInputModel input);
}
