using GerenciadordeBiblioteca.Models;

namespace GerenciadordeBiblioteca.Services
{
    public interface IBibliotecaService
    {
        Task<List<Livro>> ListarTodos();
        Task<int> ContarTotal();
        Task<Livro> Registrar(Livro livro);
        Task<Livro> BuscarPorId(int id);
        Task<Livro> Editar(Livro livro);
        Task Apagar(int id);
    }
}
