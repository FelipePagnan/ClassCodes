using Caixinhas.Intermediario.Aplicacao.InputModels;
using Caixinhas.Intermediario.Aplicacao.ViewModels;
using Caixinhas.Intermediario.Dominio.Entidades;
using Caixinhas.Intermediario.Dominio.Enums;
using Caixinhas.Intermediario.Dominio.Excecoes;
using Caixinhas.Intermediario.Dominio.Interfaces;
using Caixinhas.Intermediario.Infraestrutura.Servicos;

namespace Caixinhas.Intermediario.Aplicacao.Servicos;

public interface IObjetivoAppService
{
    Task<List<ObjetivoViewModel>> ListarAsync(StatusObjetivo? status);
    Task<ObjetivoDetalheViewModel> ObterPorIdAsync(int id);
    Task<ObjetivoViewModel> CadastrarAsync(CriarObjetivoInputModel input);
    Task<ObjetivoViewModel> AtualizarAsync(int id, AtualizarObjetivoInputModel input);
    Task RemoverAsync(int id);
    Task<ObjetivoViewModel> AlterarStatusAsync(int id, string acao);
    Task<ObjetivoViewModel> DefinirCapaAsync(int id, Stream conteudo, string nomeArquivo, long tamanho);
    Task<(byte[] Conteudo, string TipoConteudo)> ObterCapaAsync(int id);
    Task<SimulacaoViewModel> SimularAsync(int id, SimularEvolucaoInputModel input);
}

public class ObjetivoAppService : IObjetivoAppService
{
    private readonly IObjetivoRepository _repository;
    private readonly IValidadorDeImagem _validadorDeImagem;
    private readonly ILogger<ObjetivoAppService> _logger;

    public ObjetivoAppService(
        IObjetivoRepository repository,
        IValidadorDeImagem validadorDeImagem,
        ILogger<ObjetivoAppService> logger)
    {
        _repository = repository;
        _validadorDeImagem = validadorDeImagem;
        _logger = logger;
    }

    public async Task<List<ObjetivoViewModel>> ListarAsync(StatusObjetivo? status)
    {
        var objetivos = await _repository.ObterTodosAsync(status);
        return objetivos.Select(ObjetivoViewModel.De).ToList();
    }

    public async Task<ObjetivoDetalheViewModel> ObterPorIdAsync(int id)
    {
        var objetivo = await _repository.ObterPorIdComTransacoesAsync(id)
            ?? throw NaoEncontradoException.Para("Objetivo financeiro", id);

        return ObjetivoDetalheViewModel.DeDetalhe(objetivo);
    }

    public async Task<ObjetivoViewModel> CadastrarAsync(CriarObjetivoInputModel input)
    {
        if (await _repository.ExisteTituloAsync(input.Titulo))
            throw new RegraNegocioException($"Já existe um objetivo chamado '{input.Titulo.Trim()}'.");

        var objetivo = new ObjetivoFinanceiro(
            input.Titulo,
            input.QuantidadeAlvo,
            input.Prazo,
            input.QuantidadeIdealAporteMensal);

        await _repository.AdicionarAsync(objetivo);

        _logger.LogInformation(
            "Objetivo {ObjetivoId} '{Titulo}' criado com meta de R$ {Meta}.",
            objetivo.Id, objetivo.Titulo, objetivo.QuantidadeAlvo);

        return ObjetivoViewModel.De(objetivo);
    }

    public async Task<ObjetivoViewModel> AtualizarAsync(int id, AtualizarObjetivoInputModel input)
    {
        var objetivo = await _repository.ObterPorIdComTransacoesAsync(id)
            ?? throw NaoEncontradoException.Para("Objetivo financeiro", id);

        if (await _repository.ExisteTituloAsync(input.Titulo, ignorarId: id))
            throw new RegraNegocioException($"Já existe outro objetivo chamado '{input.Titulo.Trim()}'.");

        objetivo.Atualizar(
            input.Titulo,
            input.QuantidadeAlvo,
            input.Prazo,
            input.QuantidadeIdealAporteMensal);

        await _repository.SalvarAsync();

        return ObjetivoViewModel.De(objetivo);
    }

    /// <summary>Remoção lógica do objetivo e das transações dele.</summary>
    public async Task RemoverAsync(int id)
    {
        var objetivo = await _repository.ObterPorIdComTransacoesAsync(id)
            ?? throw NaoEncontradoException.Para("Objetivo financeiro", id);

        objetivo.Remover();
        await _repository.SalvarAsync();

        _logger.LogInformation("Objetivo {ObjetivoId} removido (soft delete).", id);
    }

    public async Task<ObjetivoViewModel> AlterarStatusAsync(int id, string acao)
    {
        var objetivo = await _repository.ObterPorIdComTransacoesAsync(id)
            ?? throw NaoEncontradoException.Para("Objetivo financeiro", id);

        switch (acao.ToLowerInvariant())
        {
            case "pausar": objetivo.Pausar(); break;
            case "retomar": objetivo.Retomar(); break;
            case "cancelar": objetivo.Cancelar(); break;
            default:
                throw new RegraNegocioException(
                    $"Ação '{acao}' inválida. Use pausar, retomar ou cancelar.");
        }

        await _repository.SalvarAsync();

        _logger.LogInformation(
            "Objetivo {ObjetivoId} agora está {Status}.", objetivo.Id, objetivo.Status);

        return ObjetivoViewModel.De(objetivo);
    }

    /// <summary>
    /// Recebe o arquivo de imagem enviado e grava os bytes na entidade (PLUS).
    /// </summary>
    public async Task<ObjetivoViewModel> DefinirCapaAsync(
        int id, Stream conteudo, string nomeArquivo, long tamanho)
    {
        var objetivo = await _repository.ObterPorIdComTransacoesAsync(id)
            ?? throw NaoEncontradoException.Para("Objetivo financeiro", id);

        if (tamanho <= 0)
            throw new RegraNegocioException("O arquivo enviado está vazio.");

        if (tamanho > ObjetivoFinanceiro.TamanhoMaximoCapaBytes)
            throw new RegraNegocioException(
                $"A imagem de capa deve ter no máximo " +
                $"{ObjetivoFinanceiro.TamanhoMaximoCapaBytes / 1024 / 1024}MB.");

        if (!_validadorDeImagem.ExtensaoPermitida(nomeArquivo))
            throw new RegraNegocioException(
                $"Extensão não permitida. Use: {string.Join(", ", _validadorDeImagem.ExtensoesPermitidas)}");

        using var memoria = new MemoryStream();
        await conteudo.CopyToAsync(memoria);
        var bytes = memoria.ToArray();

        // Confere a assinatura binária, não apenas a extensão do arquivo.
        if (!_validadorDeImagem.EhImagemValida(bytes))
            throw new RegraNegocioException("O arquivo enviado não parece ser uma imagem válida.");

        objetivo.DefinirCapa(bytes);
        await _repository.SalvarAsync();

        _logger.LogInformation(
            "Capa do objetivo {ObjetivoId} atualizada ({Bytes} bytes).", id, bytes.Length);

        return ObjetivoViewModel.De(objetivo);
    }

    public async Task<(byte[] Conteudo, string TipoConteudo)> ObterCapaAsync(int id)
    {
        var objetivo = await _repository.ObterPorIdAsync(id)
            ?? throw NaoEncontradoException.Para("Objetivo financeiro", id);

        if (!objetivo.TemCapa)
            throw new NaoEncontradoException($"O objetivo '{objetivo.Titulo}' não possui capa.");

        var conteudo = objetivo.CapaObjetivo!;
        return (conteudo, _validadorDeImagem.DetectarTipoConteudo(conteudo));
    }

    /// <summary>
    /// Simula a evolução da caixinha com aportes mensais e rendimentos (PLUS).
    /// </summary>
    public async Task<SimulacaoViewModel> SimularAsync(int id, SimularEvolucaoInputModel input)
    {
        var objetivo = await _repository.ObterPorIdComTransacoesAsync(id)
            ?? throw NaoEncontradoException.Para("Objetivo financeiro", id);

        // Sem aporte informado, usa o aporte ideal cadastrado ou o necessário
        // para bater a meta no prazo.
        var aporte = input.AporteMensal > 0
            ? input.AporteMensal
            : objetivo.QuantidadeIdealAporteMensal ?? objetivo.AporteMensalNecessario ?? 0m;

        var resultado = objetivo.Simular(
            aporte, input.QuantidadeMeses, input.TaxaJurosMensalPercentual);

        return SimulacaoViewModel.De(resultado);
    }
}
