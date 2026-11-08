using AvaliacaoLivros.AppConsole.Models;

namespace AvaliacaoLivros.AppConsole.Services;

/// <summary>
/// Concentra toda a regra de negócio do sistema: livros, usuários e avaliações.
/// Os dados ficam em memória (listas), reiniciando a cada execução do programa.
/// </summary>
public class AvaliacaoLivrosService
{
    private readonly List<Livro> _livros = new();
    private readonly List<Usuario> _usuarios = new();
    private readonly List<Avaliacao> _avaliacoes = new();

    private int _proximoIdLivro = 1;
    private int _proximoIdUsuario = 1;
    private int _proximoIdAvaliacao = 1;

    // ===================== LIVROS =====================

    public Livro CadastrarLivro(
        string titulo, string descricao, string isbn, string autor, string editora,
        GeneroLivroEnum genero, int anoDePublicacao, int quantidadePaginas)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(titulo))
            erros.Add("O título não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(autor))
            erros.Add("O autor não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(isbn))
            erros.Add("O ISBN não pode ser vazio.");
        else if (_livros.Any(l => l.ISBN.Equals(isbn, StringComparison.OrdinalIgnoreCase)))
            erros.Add("Já existe um livro cadastrado com esse ISBN.");

        if (anoDePublicacao < 1440 || anoDePublicacao > DateTime.Now.Year)
            erros.Add($"Ano de publicação inválido. Informe um valor entre 1440 e {DateTime.Now.Year}.");

        if (quantidadePaginas <= 0)
            erros.Add("A quantidade de páginas deve ser maior que zero.");

        if (erros.Count > 0)
            throw new ArgumentException(string.Join(" ", erros));

        var livro = new Livro
        {
            Id = _proximoIdLivro++,
            Titulo = titulo.Trim(),
            Descricao = descricao.Trim(),
            ISBN = isbn.Trim(),
            Autor = autor.Trim(),
            Editora = editora.Trim(),
            Genero = genero,
            AnoDePublicacao = anoDePublicacao,
            QuantidadePaginas = quantidadePaginas,
            DataCriacao = DateTime.Now,
            NotaMedia = 0
        };

        _livros.Add(livro);
        return livro;
    }

    public Livro AtualizarLivro(
        int id, string titulo, string descricao, string autor, string editora,
        GeneroLivroEnum genero, int anoDePublicacao, int quantidadePaginas)
    {
        var livro = ConsultarLivroPorId(id)
            ?? throw new ArgumentException("Livro não encontrado.");

        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(titulo))
            erros.Add("O título não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(autor))
            erros.Add("O autor não pode ser vazio.");

        if (anoDePublicacao < 1440 || anoDePublicacao > DateTime.Now.Year)
            erros.Add($"Ano de publicação inválido. Informe um valor entre 1440 e {DateTime.Now.Year}.");

        if (quantidadePaginas <= 0)
            erros.Add("A quantidade de páginas deve ser maior que zero.");

        if (erros.Count > 0)
            throw new ArgumentException(string.Join(" ", erros));

        livro.Titulo = titulo.Trim();
        livro.Descricao = descricao.Trim();
        livro.Autor = autor.Trim();
        livro.Editora = editora.Trim();
        livro.Genero = genero;
        livro.AnoDePublicacao = anoDePublicacao;
        livro.QuantidadePaginas = quantidadePaginas;

        return livro;
    }

    public List<Livro> ConsultarTodosLivros() => _livros.ToList();

    public Livro? ConsultarLivroPorId(int id) => _livros.FirstOrDefault(l => l.Id == id);

    public bool RemoverLivro(int id)
    {
        var livro = ConsultarLivroPorId(id);
        if (livro is null)
            return false;

        _livros.Remove(livro);
        _avaliacoes.RemoveAll(a => a.IdLivro == id);
        return true;
    }

    /// <summary>Cadastra/atualiza a capa de um livro a partir de bytes de imagem (PLUS).</summary>
    public void CadastrarCapaLivro(int idLivro, byte[] conteudoImagem)
    {
        var livro = ConsultarLivroPorId(idLivro)
            ?? throw new ArgumentException("Livro não encontrado.");

        if (conteudoImagem is null || conteudoImagem.Length == 0)
            throw new ArgumentException("Arquivo de imagem vazio ou inválido.");

        livro.CapaLivro = conteudoImagem;
    }

    // ===================== USUÁRIOS =====================

    public Usuario CadastrarUsuario(string nome, string email)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(nome))
            erros.Add("O nome não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@') || !email.Contains('.'))
            erros.Add("Informe um e-mail válido.");
        else if (_usuarios.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            erros.Add("Já existe um usuário cadastrado com esse e-mail.");

        if (erros.Count > 0)
            throw new ArgumentException(string.Join(" ", erros));

        var usuario = new Usuario
        {
            Id = _proximoIdUsuario++,
            Nome = nome.Trim(),
            Email = email.Trim()
        };

        _usuarios.Add(usuario);
        return usuario;
    }

    public Usuario AtualizarUsuario(int id, string nome, string email)
    {
        var usuario = ConsultarUsuarioPorId(id)
            ?? throw new ArgumentException("Usuário não encontrado.");

        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(nome))
            erros.Add("O nome não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@') || !email.Contains('.'))
            erros.Add("Informe um e-mail válido.");
        else if (_usuarios.Any(u => u.Id != id && u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            erros.Add("Já existe outro usuário cadastrado com esse e-mail.");

        if (erros.Count > 0)
            throw new ArgumentException(string.Join(" ", erros));

        usuario.Nome = nome.Trim();
        usuario.Email = email.Trim();

        return usuario;
    }

    public List<Usuario> ConsultarTodosUsuarios() => _usuarios.ToList();

    public Usuario? ConsultarUsuarioPorId(int id) => _usuarios.FirstOrDefault(u => u.Id == id);

    public bool RemoverUsuario(int id)
    {
        var usuario = ConsultarUsuarioPorId(id);
        if (usuario is null)
            return false;

        _usuarios.Remove(usuario);
        _avaliacoes.RemoveAll(a => a.IdUsuario == id);
        return true;
    }

    // ===================== AVALIAÇÕES =====================

    public Avaliacao CadastrarAvaliacao(
        int idUsuario, int idLivro, int nota, string descricao,
        DateTime dataInicioLeitura, DateTime dataFimLeitura)
    {
        var usuario = ConsultarUsuarioPorId(idUsuario)
            ?? throw new ArgumentException("Usuário não encontrado.");

        var livro = ConsultarLivroPorId(idLivro)
            ?? throw new ArgumentException("Livro não encontrado.");

        var erros = new List<string>();

        if (nota < 1 || nota > 5)
            erros.Add("A nota deve ser entre 1 e 5.");

        if (dataInicioLeitura.Date > dataFimLeitura.Date)
            erros.Add("A data de início da leitura não pode ser maior que a data de fim da leitura.");

        if (erros.Count > 0)
            throw new ArgumentException(string.Join(" ", erros));

        var avaliacao = new Avaliacao
        {
            Id = _proximoIdAvaliacao++,
            Nota = nota,
            Descricao = descricao.Trim(),
            IdUsuario = idUsuario,
            IdLivro = idLivro,
            DataCriacao = DateTime.Now,
            DataInicioLeitura = dataInicioLeitura.Date,
            DataFimLeitura = dataFimLeitura.Date
        };

        _avaliacoes.Add(avaliacao);
        usuario.Avaliacoes.Add(avaliacao);
        livro.Avaliacoes.Add(avaliacao);

        AtualizarNotaMedia(livro);

        return avaliacao;
    }

    /// <summary>Recalcula a nota média do livro (PLUS) sempre que uma avaliação é cadastrada.</summary>
    private void AtualizarNotaMedia(Livro livro)
    {
        livro.NotaMedia = livro.Avaliacoes.Count > 0
            ? Math.Round((decimal)livro.Avaliacoes.Average(a => a.Nota), 1)
            : 0;
    }

    public List<Avaliacao> ConsultarAvaliacoesPorLivro(int idLivro)
    {
        ConsultarLivroPorId(idLivro) ?? throw new ArgumentException("Livro não encontrado.");
        return _avaliacoes.Where(a => a.IdLivro == idLivro).OrderByDescending(a => a.DataCriacao).ToList();
    }

    public List<Avaliacao> ConsultarAvaliacoesPorUsuario(int idUsuario)
    {
        ConsultarUsuarioPorId(idUsuario) ?? throw new ArgumentException("Usuário não encontrado.");
        return _avaliacoes.Where(a => a.IdUsuario == idUsuario).OrderByDescending(a => a.DataCriacao).ToList();
    }

    // ===================== RELATÓRIOS (PLUS) =====================

    /// <summary>
    /// Quantidade de livros "lidos" em um determinado ano — considerando que um livro
    /// é lido quando o usuário finaliza a leitura (DataFimLeitura) dentro do ano informado.
    /// Um mesmo livro pode ser contado mais de uma vez, caso avaliado por usuários diferentes.
    /// </summary>
    public List<(Livro Livro, Usuario Usuario, DateTime DataFimLeitura)> RelatorioLivrosLidosNoAno(int ano)
    {
        return _avaliacoes
            .Where(a => a.DataFimLeitura.Year == ano)
            .OrderBy(a => a.DataFimLeitura)
            .Select(a => (ConsultarLivroPorId(a.IdLivro)!, ConsultarUsuarioPorId(a.IdUsuario)!, a.DataFimLeitura))
            .ToList();
    }
}
