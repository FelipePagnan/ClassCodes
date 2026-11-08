using Livros.Domain.Common;
using Livros.Domain.Events;
using Livros.Domain.Exceptions;
using Livros.Domain.ValueObjects;

namespace Livros.Domain.Entities;

public class Usuario : BaseEntity
{
    private readonly List<Avaliacao> _avaliacoes = new();

    // Construtor sem parâmetros para o EF Core.
    private Usuario() { }

    public Usuario(string nome, string email)
    {
        DefinirNome(nome);
        DefinirEmail(email);
        DataCriacao = DateTime.Now;

        AdicionarEvento(new UsuarioCadastradoEvent(Id, Nome, EmailValor));
    }

    public string Nome { get; private set; } = string.Empty;

    /// <summary>E-mail persistido. Exposto como Value Object através de <see cref="Email"/>.</summary>
    public string EmailValor { get; private set; } = string.Empty;

    public Email Email => ValueObjects.Email.Criar(EmailValor);

    public IReadOnlyCollection<Avaliacao> Avaliacoes => _avaliacoes.AsReadOnly();

    public int TotalAvaliacoes => _avaliacoes.Count;

    public double? NotaMediaAtribuida => _avaliacoes.Count == 0
        ? null
        : Math.Round(_avaliacoes.Average(a => a.Nota), 2);

    /// <summary>Total de páginas lidas somando todos os livros avaliados.</summary>
    public int TotalPaginasLidas => _avaliacoes.Sum(a => a.Livro?.QuantidadePaginas ?? 0);

    public void Atualizar(string nome, string email)
    {
        DefinirNome(nome);
        DefinirEmail(email);
    }

    /// <summary>
    /// Garante que o usuário pode ser removido. Um usuário com avaliações
    /// não é removido para não apagar o histórico de notas dos livros.
    /// </summary>
    public void GarantirQuePodeSerRemovido()
    {
        if (_avaliacoes.Count > 0)
            throw new RegraNegocioException(
                $"'{Nome}' possui {_avaliacoes.Count} avaliação(ões) e não pode ser removido.");
    }

    /// <summary>Quantos livros o usuário terminou de ler no ano informado.</summary>
    public int LivrosLidosNoAno(int ano)
        => _avaliacoes.Count(a => a.DataFimLeitura.Year == ano);

    private void DefinirNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length < 3)
            throw new RegraNegocioException("O nome é obrigatório e deve ter ao menos 3 caracteres.");

        if (nome.Trim().Length > 200)
            throw new RegraNegocioException("O nome deve ter no máximo 200 caracteres.");

        Nome = nome.Trim();
    }

    private void DefinirEmail(string email)
        => EmailValor = ValueObjects.Email.Criar(email).Valor;
}
