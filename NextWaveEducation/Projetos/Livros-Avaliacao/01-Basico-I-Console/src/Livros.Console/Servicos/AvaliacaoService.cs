using Livros.Console.Entidades;
using Livros.Console.Excecoes;

namespace Livros.Console.Servicos;

/// <summary>
/// Cadastro e listagem de avaliações. Aplica as regras de nota e de datas
/// de leitura, e mantém a nota média do livro atualizada.
/// </summary>
public class AvaliacaoService
{
    private readonly LivroService _livroService;
    private readonly UsuarioService _usuarioService;
    private readonly List<Avaliacao> _avaliacoes = new();
    private int _proximoId = 1;

    public AvaliacaoService(LivroService livroService, UsuarioService usuarioService)
    {
        _livroService = livroService;
        _usuarioService = usuarioService;
    }

    public IReadOnlyList<Avaliacao> ListarTodas() => _avaliacoes.AsReadOnly();

    public IEnumerable<Avaliacao> ListarPorLivro(int idLivro)
    {
        var livro = _livroService.ObterPorId(idLivro);
        return livro.Avaliacoes.OrderByDescending(a => a.DataCriacao);
    }

    public IEnumerable<Avaliacao> ListarPorUsuario(int idUsuario)
    {
        var usuario = _usuarioService.ObterPorId(idUsuario);
        return usuario.Avaliacoes.OrderByDescending(a => a.DataCriacao);
    }

    public Avaliacao Cadastrar(
        int idLivro,
        int idUsuario,
        int nota,
        string descricao,
        DateTime dataInicioLeitura,
        DateTime dataFimLeitura)
    {
        var livro = _livroService.ObterPorId(idLivro);
        var usuario = _usuarioService.ObterPorId(idUsuario);

        // Regra de negócio: a nota deve ser de 1 a 5.
        if (nota < Avaliacao.NotaMinima || nota > Avaliacao.NotaMaxima)
            throw new RegraNegocioException(
                $"A nota deve ser de {Avaliacao.NotaMinima} a {Avaliacao.NotaMaxima}. Valor informado: {nota}.");

        // Regra de negócio: a data de início não pode ser maior que a data de fim de leitura.
        if (dataInicioLeitura.Date > dataFimLeitura.Date)
            throw new RegraNegocioException(
                $"A data de início da leitura ({dataInicioLeitura:dd/MM/yyyy}) não pode ser " +
                $"maior que a data de fim ({dataFimLeitura:dd/MM/yyyy}).");

        if (dataFimLeitura.Date > DateTime.Today)
            throw new RegraNegocioException("A data de fim da leitura não pode estar no futuro.");

        // Um mesmo usuário não avalia o mesmo livro duas vezes.
        if (livro.Avaliacoes.Any(a => a.IdUsuario == idUsuario))
            throw new RegraNegocioException(
                $"'{usuario.Nome}' já avaliou o livro '{livro.Titulo}'.");

        var avaliacao = new Avaliacao
        {
            Id = _proximoId++,
            IdLivro = livro.Id,
            IdUsuario = usuario.Id,
            Nota = nota,
            Descricao = descricao?.Trim() ?? string.Empty,
            DataInicioLeitura = dataInicioLeitura.Date,
            DataFimLeitura = dataFimLeitura.Date,
            DataCriacao = DateTime.Now,
            Livro = livro,
            Usuario = usuario
        };

        livro.Avaliacoes.Add(avaliacao);
        usuario.Avaliacoes.Add(avaliacao);
        _avaliacoes.Add(avaliacao);

        // A cada avaliação cadastrada a média do livro é recalculada.
        livro.RecalcularNotaMedia();

        return avaliacao;
    }

    public void Remover(int id)
    {
        var avaliacao = _avaliacoes.FirstOrDefault(a => a.Id == id)
            ?? throw new RegraNegocioException($"Avaliação com id {id} não encontrada.");

        avaliacao.Livro?.Avaliacoes.Remove(avaliacao);
        avaliacao.Usuario?.Avaliacoes.Remove(avaliacao);
        _avaliacoes.Remove(avaliacao);

        avaliacao.Livro?.RecalcularNotaMedia();
    }
}
