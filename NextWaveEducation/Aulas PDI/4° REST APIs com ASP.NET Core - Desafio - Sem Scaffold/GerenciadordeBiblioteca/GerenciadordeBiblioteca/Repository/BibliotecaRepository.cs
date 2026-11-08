using GerenciadordeBiblioteca.Data;
using GerenciadordeBiblioteca.Models;
using Microsoft.EntityFrameworkCore;

namespace GerenciadordeBiblioteca.Repository
{
    public class BibliotecaRepository : IBibliotecaRepository
    {
        private readonly GerenciadordeBibliotecaContext _context;
        public BibliotecaRepository(GerenciadordeBibliotecaContext context)
        {
            _context = context;
        }

        public async Task Apagar(Livro livro)
        {
            _context.Livros.Remove(livro);
            await _context.SaveChangesAsync();
        }

        public async Task<Livro> BuscarPorId(int id)
        {
            var livroDb = await _context.Livros
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id);

            if (livroDb == null)
            {
                throw new Exception("Livro não encontrado.");
            }
            return livroDb;
        }

        public async Task<List<Livro>> BuscarTodos()
        {
            var livrosBanco = await _context.Livros.ToListAsync();

            return livrosBanco;
        }

        public async Task<int> BuscarTotal()
        {
            return await _context.Livros.CountAsync();
        }

        public async Task<Livro> Editar(Livro livro)
        {
            _context.Livros.Update(livro);
            await _context.SaveChangesAsync();
            return livro;
        }

        public async Task<Livro> Registrar(Livro livro)
        {
            try
            {
                await _context.Livros.AddAsync(livro);
                await _context.SaveChangesAsync();

                return livro;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao registrar o livroDb: {ex.Message}");
            }

        }

        public async Task<bool> VerificarSeLivroFoiRegistrado(string isbn)
        {
            return await _context.Livros.AnyAsync(l => l.ISBN == isbn);
        }
    }
}
