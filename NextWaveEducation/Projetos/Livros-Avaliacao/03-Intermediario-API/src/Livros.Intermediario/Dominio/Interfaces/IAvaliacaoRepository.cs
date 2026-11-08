using Livros.Intermediario.Dominio.Entidades;

namespace Livros.Intermediario.Dominio.Interfaces;

public interface IAvaliacaoRepository
{
    Task<List<Avaliacao>> ObterPorLivroAsync(int idLivro);
    Task<List<Avaliacao>> ObterPorUsuarioAsync(int idUsuario);
    Task<List<Avaliacao>> ObterPorAnoDeLeituraAsync(int ano);
    Task AdicionarAsync(Avaliacao avaliacao);
}
