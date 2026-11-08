using GerenciadordeBiblioteca.Models;

namespace GerenciadordeBiblioteca.Repository
{
    public interface IBibliotecaRepository
    {
        Task<List<Livro>> BuscarTodos();
        Task<bool> VerificarSeLivroFoiRegistrado(string titulo);
        Task<Livro> Registrar(Livro livro);
        Task<Livro> BuscarPorId(int id);
        Task<Livro> Editar(Livro livro);
        Task Apagar (Livro livro);
        public Task<int> BuscarTotal();

    }
}