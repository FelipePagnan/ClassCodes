using GerenciadordeBiblioteca.Models;
using Microsoft.EntityFrameworkCore;

namespace GerenciadordeBiblioteca.Data
{
    public class GerenciadordeBibliotecaContext : DbContext
    {
        public GerenciadordeBibliotecaContext(DbContextOptions<GerenciadordeBibliotecaContext> options) : base(options)
        {
        }

        public DbSet<Livro> Livros { get; set; }

    }
}
