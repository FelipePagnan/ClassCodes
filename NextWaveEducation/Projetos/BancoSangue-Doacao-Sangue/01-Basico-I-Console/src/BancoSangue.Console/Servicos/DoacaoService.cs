using BancoSangue.Console.Entidades;
using BancoSangue.Console.Excecoes;

namespace BancoSangue.Console.Servicos;

/// <summary>
/// Responsável por registrar doações, aplicar as regras de negócio e atualizar o estoque.
/// </summary>
public class DoacaoService
{
    private const int QuantidadeMinimaML = 420;
    private const int QuantidadeMaximaML = 470;
    private const double PesoMinimoKg = 50;

    private readonly DoadorService _doadorService;
    private readonly EstoqueService _estoqueService;
    private readonly List<Doacao> _doacoes = new();
    private int _proximoId = 1;

    public DoacaoService(DoadorService doadorService, EstoqueService estoqueService)
    {
        _doadorService = doadorService;
        _estoqueService = estoqueService;
    }

    public IReadOnlyList<Doacao> ListarTodas() => _doacoes.AsReadOnly();

    /// <summary>
    /// Registra uma doação aplicando todas as regras de negócio.
    /// Lança RegraNegocioException caso alguma regra não seja atendida.
    /// </summary>
    public (Doacao Doacao, bool EstoqueBaixo) Registrar(int doadorId, int quantidadeML, DateTime? dataDoacao = null)
    {
        var doador = _doadorService.BuscarPorId(doadorId)
            ?? throw new RegraNegocioException($"Doador com id {doadorId} não encontrado.");

        // Regra: menor de idade não pode doar.
        if (!doador.EhMaiorDeIdade)
            throw new RegraNegocioException($"'{doador.NomeCompleto}' é menor de idade e não pode doar sangue.");

        // Regra: peso mínimo de 50kg.
        if (doador.Peso < PesoMinimoKg)
            throw new RegraNegocioException($"'{doador.NomeCompleto}' pesa {doador.Peso}kg. O peso mínimo para doar é {PesoMinimoKg}kg.");

        // Regra: quantidade entre 420ml e 470ml.
        if (quantidadeML < QuantidadeMinimaML || quantidadeML > QuantidadeMaximaML)
            throw new RegraNegocioException($"A quantidade doada deve estar entre {QuantidadeMinimaML}ml e {QuantidadeMaximaML}ml.");

        var data = dataDoacao ?? DateTime.Now;

        // Regra: intervalo mínimo entre doações, de acordo com o gênero (homens 60 dias, mulheres 90 dias).
        if (doador.UltimaDoacao is DateTime ultima)
        {
            var diasDesdeUltima = (data.Date - ultima.Date).TotalDays;
            if (diasDesdeUltima < doador.IntervaloMinimoDiasEntreDoacoes)
            {
                var diasRestantes = doador.IntervaloMinimoDiasEntreDoacoes - diasDesdeUltima;
                throw new RegraNegocioException(
                    $"'{doador.NomeCompleto}' deve aguardar mais {diasRestantes:0} dia(s) para doar novamente " +
                    $"(intervalo mínimo de {doador.IntervaloMinimoDiasEntreDoacoes} dias).");
            }
        }

        var doacao = new Doacao
        {
            Id = _proximoId++,
            DoadorId = doador.Id,
            DataDoacao = data,
            QuantidadeML = quantidadeML
        };

        doador.Doacoes.Add(doacao);
        _doacoes.Add(doacao);

        // Atualiza o estoque sempre que uma doação for registrada.
        var estoqueBaixo = _estoqueService.AtualizarEstoque(doador.TipoSanguineo, doador.FatorRh, quantidadeML);

        return (doacao, estoqueBaixo);
    }

    public IEnumerable<Doacao> RelatorioUltimos30Dias()
    {
        var limite = DateTime.Now.AddDays(-30);
        return _doacoes.Where(d => d.DataDoacao >= limite).OrderByDescending(d => d.DataDoacao);
    }
}
