using BancoSangue.Intermediario.Dominio.Entidades;

namespace BancoSangue.Intermediario.Dominio.Interfaces;

public interface IDoacaoRepository
{
    Task<List<Doacao>> ObterTodasAsync();
    Task<List<Doacao>> ObterPorDoadorAsync(int doadorId);
    Task<List<Doacao>> ObterUltimos30DiasAsync();
    Task AdicionarAsync(Doacao doacao);
}
