using GerenciadordeBiblioteca.Models;
using GerenciadordeBiblioteca.Repository;

namespace GerenciadordeBiblioteca.Services
{
    public class BibliotecaService : IBibliotecaService
    {
        private readonly IBibliotecaRepository _bibliotecaRepository;

        public BibliotecaService(IBibliotecaRepository bibliotecaRepository)
        {
            _bibliotecaRepository = bibliotecaRepository;
        }

        public async Task Apagar(int id)
        {
            var livroDb = await _bibliotecaRepository.BuscarPorId(id);
            await _bibliotecaRepository.Apagar(livroDb);

        }

        public async Task<Livro> BuscarPorId(int id)
        {
            return await _bibliotecaRepository.BuscarPorId(id);
        }

        public async Task<int> ContarTotal()
        {
            return await _bibliotecaRepository.BuscarTotal();
        }

        public async Task<Livro> Editar(Livro livro)
        {
            var livroDb = await _bibliotecaRepository.BuscarPorId(livro.Id);
            if (livroDb == null)
            {
                throw new Exception("Livro não encontrado.");
            }

            // Atualize apenas as propriedades necessárias
            livroDb.Titulo = livro.Titulo;
            livroDb.Autor = livro.Autor;
            livroDb.ISBN = livro.ISBN;
            livroDb.AnoPublicacao = livro.AnoPublicacao;
            livroDb.NotaDoLivro = livro.NotaDoLivro;

            return await _bibliotecaRepository.Editar(livroDb);
        }

        public async Task<List<Livro>> ListarTodos()
        {
            var livrosBanco = await _bibliotecaRepository.BuscarTodos();

            return livrosBanco;
        }

        public async Task<Livro> Registrar(Livro livro)
        {
            var livroRegistrado = await _bibliotecaRepository.VerificarSeLivroFoiRegistrado(livro.ISBN);
            if (livroRegistrado)
            {
                throw new Exception("Livro já registrado pelo ISBN.");
            }
            return await _bibliotecaRepository.Registrar(livro);
        }
    }
}
