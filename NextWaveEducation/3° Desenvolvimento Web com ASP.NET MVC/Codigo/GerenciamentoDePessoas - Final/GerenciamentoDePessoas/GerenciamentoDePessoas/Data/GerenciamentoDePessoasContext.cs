using GerenciamentoDePessoas.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GerenciamentoDePessoas.Data
{
    public class GerenciamentoDePessoasContext : IdentityDbContext<Usuario>
    {
        public GerenciamentoDePessoasContext(DbContextOptions<GerenciamentoDePessoasContext> options) : base(options)
        {
        }
        public DbSet<Pessoa> Pessoas { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            var administrador = new IdentityRole
            {
                Id = "1A7E2F64-9A1E-4A9E-9B57-2F3028E3A02D", // GUID fixo
                Name = "administrador",
                NormalizedName = "ADMINISTRADOR"
            };

            var operador = new IdentityRole
            {
                Id = "2B9E3F75-ABCD-4D5E-8C56-3D1238E4B03E", // GUID fixo
                Name = "operador",
                NormalizedName = "OPERADOR"
            };

            builder.Entity<IdentityRole>().HasData(administrador, operador);

            base.OnModelCreating(builder);
        }


    }
}
