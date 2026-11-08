using Livros.Intermediario.Aplicacao.InputModels;
using Livros.Intermediario.Aplicacao.ViewModels;
using Livros.Intermediario.Dominio.Entidades;
using Livros.Intermediario.Dominio.Enums;
using Livros.Intermediario.Dominio.Excecoes;
using Livros.Intermediario.Dominio.Interfaces;

namespace Livros.Intermediario.Aplicacao.Servicos;

public interface ILivroAppService
{
    Task<List<LivroViewModel>> ListarAsync(string? busca);
    Task<LivroDetalheViewModel> ObterPorIdAsync(int id);
    Task<LivroViewModel> CadastrarAsync(CriarLivroInputModel input);
    Task<LivroViewModel> AtualizarAsync(int id, AtualizarLivroInputModel input);
    Task RemoverAsync(int id);
    Task<LivroViewModel> DefinirCapaAsync(int id, Stream conteudo, string nomeArquivo, long tamanho);
    Task<(byte[] Conteudo, string TipoConteudo)> ObterCapaAsync(int id);
    Task<List<LivroExternoViewModel>> ConsultarApiExternaAsync(string termo, bool porIsbn, CancellationToken ct);
    Task<LivroViewModel> ImportarDaApiExternaAsync(ImportarLivroInputModel input, CancellationToken ct);
}

public class LivroAppService : ILivroAppService
{
    private static readonly string[] _extensoesPermitidas = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"];

    private readonly ILivroRepository _livroRepository;
    private readonly IConsultaLivroExternoService _consultaExterna;
    private readonly ILogger<LivroAppService> _logger;

    public LivroAppService(
        ILivroRepository livroRepository,
        IConsultaLivroExternoService consultaExterna,
        ILogger<LivroAppService> logger)
    {
        _livroRepository = livroRepository;
        _consultaExterna = consultaExterna;
        _logger = logger;
    }

    public async Task<List<LivroViewModel>> ListarAsync(string? busca)
    {
        var livros = await _livroRepository.ObterTodosAsync(busca);
        return livros.Select(LivroViewModel.De).ToList();
    }

    public async Task<LivroDetalheViewModel> ObterPorIdAsync(int id)
    {
        var livro = await _livroRepository.ObterPorIdComAvaliacoesAsync(id)
            ?? throw NaoEncontradoException.Para("Livro", id);

        return LivroDetalheViewModel.DeDetalhe(livro);
    }

    public async Task<LivroViewModel> CadastrarAsync(CriarLivroInputModel input)
    {
        var isbnNormalizado = Livro.NormalizarIsbn(input.ISBN);

        // Regra de negócio: livros não podem ser cadastrados com o mesmo ISBN.
        if (await _livroRepository.ExisteIsbnAsync(isbnNormalizado))
            throw new RegraNegocioException($"Já existe um livro cadastrado com o ISBN '{input.ISBN}'.");

        var livro = new Livro(
            input.Titulo,
            input.Descricao,
            input.ISBN,
            input.Autor,
            input.Editora,
            input.Genero,
            input.AnoDePublicacao,
            input.QuantidadePaginas);

        await _livroRepository.AdicionarAsync(livro);
        _logger.LogInformation("Livro {LivroId} '{Titulo}' cadastrado.", livro.Id, livro.Titulo);

        return LivroViewModel.De(livro);
    }

    public async Task<LivroViewModel> AtualizarAsync(int id, AtualizarLivroInputModel input)
    {
        var livro = await _livroRepository.ObterPorIdComAvaliacoesAsync(id)
            ?? throw NaoEncontradoException.Para("Livro", id);

        var isbnNormalizado = Livro.NormalizarIsbn(input.ISBN);

        if (await _livroRepository.ExisteIsbnAsync(isbnNormalizado, ignorarId: id))
            throw new RegraNegocioException($"Já existe outro livro cadastrado com o ISBN '{input.ISBN}'.");

        livro.Atualizar(
            input.Titulo,
            input.Descricao,
            input.ISBN,
            input.Autor,
            input.Editora,
            input.Genero,
            input.AnoDePublicacao,
            input.QuantidadePaginas);

        await _livroRepository.AtualizarAsync(livro);

        return LivroViewModel.De(livro);
    }

    public async Task RemoverAsync(int id)
    {
        var livro = await _livroRepository.ObterPorIdAsync(id)
            ?? throw NaoEncontradoException.Para("Livro", id);

        await _livroRepository.RemoverAsync(livro);
        _logger.LogInformation("Livro {LivroId} removido.", id);
    }

    /// <summary>
    /// Recebe o arquivo de imagem enviado e grava os bytes na entidade (PLUS).
    /// </summary>
    public async Task<LivroViewModel> DefinirCapaAsync(
        int id, Stream conteudo, string nomeArquivo, long tamanho)
    {
        var livro = await _livroRepository.ObterPorIdComAvaliacoesAsync(id)
            ?? throw NaoEncontradoException.Para("Livro", id);

        if (tamanho <= 0)
            throw new RegraNegocioException("O arquivo enviado está vazio.");

        if (tamanho > Livro.TamanhoMaximoCapaBytes)
            throw new RegraNegocioException(
                $"A imagem de capa deve ter no máximo {Livro.TamanhoMaximoCapaBytes / 1024 / 1024}MB.");

        var extensao = Path.GetExtension(nomeArquivo).ToLowerInvariant();
        if (!_extensoesPermitidas.Contains(extensao))
            throw new RegraNegocioException(
                $"Extensão '{extensao}' não permitida. Use: {string.Join(", ", _extensoesPermitidas)}");

        using var memoria = new MemoryStream();
        await conteudo.CopyToAsync(memoria);
        var bytes = memoria.ToArray();

        if (!PareceSerImagem(bytes))
            throw new RegraNegocioException("O arquivo enviado não parece ser uma imagem válida.");

        livro.DefinirCapa(bytes);
        await _livroRepository.AtualizarAsync(livro);

        _logger.LogInformation("Capa do livro {LivroId} atualizada ({Bytes} bytes).", id, bytes.Length);

        return LivroViewModel.De(livro);
    }

    public async Task<(byte[] Conteudo, string TipoConteudo)> ObterCapaAsync(int id)
    {
        var livro = await _livroRepository.ObterPorIdAsync(id)
            ?? throw NaoEncontradoException.Para("Livro", id);

        if (!livro.TemCapa)
            throw NaoEncontradoException.Para("Capa do livro", id);

        return (livro.CapaLivro!, DetectarTipoConteudo(livro.CapaLivro!));
    }

    /// <summary>
    /// Consulta a API externa e marca quais resultados já existem na base local.
    /// </summary>
    public async Task<List<LivroExternoViewModel>> ConsultarApiExternaAsync(
        string termo, bool porIsbn, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(termo))
            throw new RegraNegocioException("Informe um ISBN ou um termo de busca.");

        List<LivroExternoDto> resultados;

        if (porIsbn)
        {
            var unico = await _consultaExterna.ConsultarPorIsbnAsync(termo, ct);
            resultados = unico is null ? new List<LivroExternoDto>() : new List<LivroExternoDto> { unico };
        }
        else
        {
            resultados = await _consultaExterna.ConsultarPorTermoAsync(termo, ct);
        }

        var saida = new List<LivroExternoViewModel>(resultados.Count);

        foreach (var dto in resultados)
        {
            var jaCadastrado = !string.IsNullOrWhiteSpace(dto.Isbn)
                && await _livroRepository.ExisteIsbnAsync(Livro.NormalizarIsbn(dto.Isbn));

            saida.Add(LivroExternoViewModel.De(dto, jaCadastrado));
        }

        return saida;
    }

    /// <summary>
    /// Busca o livro na API externa pelo ISBN e cadastra na base local.
    /// </summary>
    public async Task<LivroViewModel> ImportarDaApiExternaAsync(
        ImportarLivroInputModel input, CancellationToken ct)
    {
        var isbnNormalizado = Livro.NormalizarIsbn(input.Isbn);

        if (await _livroRepository.ExisteIsbnAsync(isbnNormalizado))
            throw new RegraNegocioException($"Já existe um livro cadastrado com o ISBN '{input.Isbn}'.");

        var externo = await _consultaExterna.ConsultarPorIsbnAsync(input.Isbn, ct)
            ?? throw new RegraNegocioException(
                $"Nenhum livro encontrado na API externa para o ISBN '{input.Isbn}'.");

        var isbnFinal = string.IsNullOrWhiteSpace(externo.Isbn) ? input.Isbn : externo.Isbn;

        var livro = new Livro(
            externo.Titulo,
            externo.Descricao,
            isbnFinal,
            externo.Autor,
            externo.Editora,
            input.Genero ?? MapearGenero(externo.Categoria),
            externo.AnoDePublicacao,
            externo.QuantidadePaginas);

        await _livroRepository.AdicionarAsync(livro);

        _logger.LogInformation(
            "Livro {LivroId} '{Titulo}' importado da API externa.", livro.Id, livro.Titulo);

        return LivroViewModel.De(livro);
    }

    /// <summary>
    /// Traduz a categoria devolvida pela API externa para o enum do sistema.
    /// </summary>
    private static GeneroLivroEnum MapearGenero(string? categoria)
    {
        if (string.IsNullOrWhiteSpace(categoria)) return GeneroLivroEnum.Outro;

        var texto = categoria.ToLowerInvariant();

        if (texto.Contains("juvenile")) return GeneroLivroEnum.Infantil;
        if (texto.Contains("science fiction")) return GeneroLivroEnum.FiccaoCientifica;
        if (texto.Contains("fantasy")) return GeneroLivroEnum.Fantasia;
        if (texto.Contains("thriller") || texto.Contains("mystery")) return GeneroLivroEnum.Suspense;
        if (texto.Contains("horror")) return GeneroLivroEnum.Terror;
        if (texto.Contains("biography")) return GeneroLivroEnum.Biografia;
        if (texto.Contains("history")) return GeneroLivroEnum.Historia;
        if (texto.Contains("computer") || texto.Contains("technology")) return GeneroLivroEnum.Tecnico;
        if (texto.Contains("self-help")) return GeneroLivroEnum.Autoajuda;
        if (texto.Contains("poetry")) return GeneroLivroEnum.Poesia;
        if (texto.Contains("romance")) return GeneroLivroEnum.Romance;
        if (texto.Contains("fiction")) return GeneroLivroEnum.Ficcao;

        return GeneroLivroEnum.Outro;
    }

    /// <summary>
    /// Confere a assinatura binária do arquivo (magic number) para garantir
    /// que a extensão não está apenas disfarçando outro tipo de arquivo.
    /// </summary>
    private static bool PareceSerImagem(byte[] bytes)
    {
        if (bytes.Length < 12) return false;

        // JPEG: FF D8 FF
        if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF) return true;

        // PNG: 89 50 4E 47
        if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47) return true;

        // GIF: "GIF8"
        if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x38) return true;

        // BMP: "BM"
        if (bytes[0] == 0x42 && bytes[1] == 0x4D) return true;

        // WEBP: "RIFF" .... "WEBP"
        if (bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46 &&
            bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50) return true;

        return false;
    }

    private static string DetectarTipoConteudo(byte[] bytes)
    {
        if (bytes.Length >= 4)
        {
            if (bytes[0] == 0xFF && bytes[1] == 0xD8) return "image/jpeg";
            if (bytes[0] == 0x89 && bytes[1] == 0x50) return "image/png";
            if (bytes[0] == 0x47 && bytes[1] == 0x49) return "image/gif";
            if (bytes[0] == 0x42 && bytes[1] == 0x4D) return "image/bmp";
            if (bytes[0] == 0x52 && bytes[1] == 0x49) return "image/webp";
        }

        return "application/octet-stream";
    }
}
