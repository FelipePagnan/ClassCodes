using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GerenciadordeBiblioteca.Models;

namespace GerenciadordeBiblioteca.Data
{
    public class GerenciadordeBibliotecaContext : DbContext
    {
        public GerenciadordeBibliotecaContext (DbContextOptions<GerenciadordeBibliotecaContext> options)
            : base(options)
        {
        }

        public DbSet<GerenciadordeBiblioteca.Models.Livro> Livro { get; set; } = default!;
    }
}
