using BancoSangue.Intermediario.Aplicacao.InputModels;
using BancoSangue.Intermediario.Aplicacao.ViewModels;
using BancoSangue.Intermediario.Dominio.Entidades;
using BancoSangue.Intermediario.Dominio.Excecoes;
using BancoSangue.Intermediario.Dominio.Interfaces;

namespace BancoSangue.Intermediario.Aplicacao.Servicos;

public class DoacaoAppService : IDoacaoAppService
{
    private readonly IDoadorRepository _doadorRepository;
    private readonly IDoacaoRepository _doacaoRepository;
    private readonly IEstoqueSangueRepository _estoqueRepository;
    private readonly ILogger<DoacaoAppService> _logger;

    public DoacaoAppService(
        IDoadorRepository doadorRepository,
        IDoacaoRepository doacaoRepository,
        IEstoqueSangueRepository estoqueRepository,
        ILogger<DoacaoAppService> logger)
    {
        _doadorRepository = doadorRepository;
        _doacaoRepository = doacaoRepository;
        _estoqueRepository = estoqueRepository;
        _logger = logger;
    }

    public async Task<List<DoacaoComDoadorViewModel>> ListarAsync()
    {
        var doacoes = await _doacaoRepository.ObterTodasAsync();
        return doacoes.Select(DoacaoComDoadorViewModel.De).ToList();
    }

    public async Task<ResultadoDoacaoViewModel> RegistrarAsync(CriarDoacaoInputModel input)
    {
        var doador = await _doadorRepository.ObterPorIdComDoacoesAsync(input.DoadorId)
            ?? throw new NaoEncontradoException($"Doador com id {input.DoadorId} não encontrado.");

        var dataDoacao = input.DataDoacao ?? DateTime.Now;

        // A própria entidade aplica as regras de idade, peso e intervalo entre doações.
        var doacao = doador.RegistrarDoacao(input.QuantidadeML, dataDoacao);

        await _doacaoRepository.AdicionarAsync(doacao);

        // Atualiza o estoque sempre que uma doação é registrada.
        var estoque = await _estoqueRepository.ObterPorTipoAsync(doador.TipoSanguineo, doador.FatorRh)
            ?? throw new RegraNegocioException(
                $"Estoque do tipo {doador.TipoSanguineoCompleto} não encontrado.");

        estoque.Adicionar(input.QuantidadeML);
        await _estoqueRepository.AtualizarAsync(estoque);

        // Avisa quando o estoque atingir a quantidade mínima definida (PLUS).
        string? alerta = null;
        if (estoque.EstoqueBaixo)
        {
            alerta = $"ALERTA: o estoque do tipo {estoque.Descricao} está em {estoque.QuantidadeML}ml, " +
                     $"abaixo do mínimo de {EstoqueSangue.QuantidadeMinimaML}ml.";
            _logger.LogWarning("{Alerta}", alerta);
        }

        return new ResultadoDoacaoViewModel
        {
            Doacao = DoacaoViewModel.De(doacao),
            EstoqueAtualizado = EstoqueViewModel.De(estoque),
            AlertaEstoque = alerta
        };
    }

    public async Task<List<DoacaoComDoadorViewModel>> RelatorioUltimos30DiasAsync()
    {
        var doacoes = await _doacaoRepository.ObterUltimos30DiasAsync();
        return doacoes.Select(DoacaoComDoadorViewModel.De).ToList();
    }
}
