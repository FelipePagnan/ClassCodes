using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GerenciamentoDePessoas.Models;

namespace GerenciamentoDePessoas.Data
{
    public class GerenciamentoDePessoasContext : DbContext
    {
        public GerenciamentoDePessoasContext (DbContextOptions<GerenciamentoDePessoasContext> options)
            : base(options)
        {
        }

        public DbSet<GerenciamentoDePessoas.Models.Pessoa> Pessoa { get; set; } = default!;
    }
}
