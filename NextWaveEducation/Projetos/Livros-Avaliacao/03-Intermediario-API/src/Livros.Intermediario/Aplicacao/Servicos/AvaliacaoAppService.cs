using Livros.Intermediario.Aplicacao.InputModels;
using Livros.Intermediario.Aplicacao.ViewModels;
using Livros.Intermediario.Dominio.Excecoes;
using Livros.Intermediario.Dominio.Interfaces;

namespace Livros.Intermediario.Aplicacao.Servicos;

public interface IAvaliacaoAppService
{
    Task<List<AvaliacaoViewModel>> ListarPorLivroAsync(int idLivro);
    Task<ResultadoAvaliacaoViewModel> CadastrarAsync(int idLivro, CriarAvaliacaoInputModel input);
    Task<RelatorioAnualViewModel> RelatorioLivrosLidosAsync(int ano);
}

public class AvaliacaoAppService : IAvaliacaoAppService
{
    private readonly ILivroRepository _livroRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IAvaliacaoRepository _avaliacaoRepository;
    private readonly ILogger<AvaliacaoAppService> _logger;

    public AvaliacaoAppService(
        ILivroRepository livroRepository,
        IUsuarioRepository usuarioRepository,
        IAvaliacaoRepository avaliacaoRepository,
        ILogger<AvaliacaoAppService> logger)
    {
        _livroRepository = livroRepository;
        _usuarioRepository = usuarioRepository;
        _avaliacaoRepository = avaliacaoRepository;
        _logger = logger;
    }

    public async Task<List<AvaliacaoViewModel>> ListarPorLivroAsync(int idLivro)
    {
        if (await _livroRepository.ObterPorIdAsync(idLivro) is null)
            throw NaoEncontradoException.Para("Livro", idLivro);

        var avaliacoes = await _avaliacaoRepository.ObterPorLivroAsync(idLivro);
        return avaliacoes.Select(AvaliacaoViewModel.De).ToList();
    }

    /// <summary>
    /// Cadastra a avaliação e devolve a nota média do livro já recalculada.
    /// </summary>
    public async Task<ResultadoAvaliacaoViewModel> CadastrarAsync(
        int idLivro, CriarAvaliacaoInputModel input)
    {
        var livro = await _livroRepository.ObterPorIdComAvaliacoesAsync(idLivro)
            ?? throw NaoEncontradoException.Para("Livro", idLivro);

        var usuario = await _usuarioRepository.ObterPorIdAsync(input.IdUsuario)
            ?? throw NaoEncontradoException.Para("Usuário", input.IdUsuario);

        // A entidade aplica as regras de nota (1 a 5) e de datas de leitura,
        // e recalcula a nota média do livro.
        var avaliacao = livro.AdicionarAvaliacao(
            usuario.Id,
            input.Nota,
            input.Descricao,
            input.DataInicioLeitura,
            input.DataFimLeitura);

        await _livroRepository.AtualizarAsync(livro);

        _logger.LogInformation(
            "Avaliação de {Nota} registrada para o livro {LivroId}. Nova média: {Media}",
            avaliacao.Nota, livro.Id, livro.NotaMedia);

        var vm = AvaliacaoViewModel.De(avaliacao);
        vm.NomeUsuario = usuario.Nome;

        return new ResultadoAvaliacaoViewModel
        {
            Avaliacao = vm,
            TituloLivro = livro.Titulo,
            NotaMediaAtualizada = livro.NotaMedia,
            TotalAvaliacoesDoLivro = livro.Avaliacoes.Count
        };
    }

    /// <summary>
    /// Relatório da quantidade de livros lidos num ano (PLUS), com base
    /// na data de fim de leitura das avaliações.
    /// </summary>
    public async Task<RelatorioAnualViewModel> RelatorioLivrosLidosAsync(int ano)
    {
        if (ano < 1900 || ano > DateTime.Today.Year + 1)
            throw new RegraNegocioException($"Ano inválido: {ano}.");

        var avaliacoes = await _avaliacaoRepository.ObterPorAnoDeLeituraAsync(ano);

        var porLivro = avaliacoes
            .Where(a => a.Livro is not null)
            .GroupBy(a => a.IdLivro)
            .Select(g => new ItemRelatorioAnualViewModel
            {
                IdLivro = g.Key,
                Titulo = g.First().Livro!.Titulo,
                Autor = g.First().Livro!.Autor,
                QuantidadePaginas = g.First().Livro!.QuantidadePaginas,
                QuantidadeLeituras = g.Count(),
                NotaMedia = Math.Round(g.Average(a => (double)a.Nota), 2)
            })
            .OrderByDescending(i => i.NotaMedia)
            .ThenBy(i => i.Titulo)
            .ToList();

        return new RelatorioAnualViewModel
        {
            Ano = ano,
            LivrosDistintosLidos = porLivro.Count,
            TotalDeLeituras = avaliacoes.Count,
            PaginasLidas = porLivro.Sum(i => i.QuantidadePaginas),
            LeitoresDistintos = avaliacoes.Select(a => a.IdUsuario).Distinct().Count(),
            NotaMediaGeral = avaliacoes.Count == 0
                ? null
                : Math.Round(avaliacoes.Average(a => (double)a.Nota), 2),
            Livros = porLivro
        };
    }
}
