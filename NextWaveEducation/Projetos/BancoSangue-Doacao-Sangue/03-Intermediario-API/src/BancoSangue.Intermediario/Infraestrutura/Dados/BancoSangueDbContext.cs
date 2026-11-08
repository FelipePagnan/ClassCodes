using System.Reflection;
using BancoSangue.Intermediario.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace BancoSangue.Intermediario.Infraestrutura.Dados;

public class BancoSangueDbContext : DbContext
{
    public BancoSangueDbContext(DbContextOptions<BancoSangueDbContext> options) : base(options) { }

    public DbSet<Doador> Doadores => Set<Doador>();
    public DbSet<Doacao> Doacoes => Set<Doacao>();
    public DbSet<Endereco> Enderecos => Set<Endereco>();
    public DbSet<EstoqueSangue> EstoquesSangue => Set<EstoqueSangue>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica automaticamente todas as classes que implementam IEntityTypeConfiguration<T>
        // presentes neste assembly (mapeamento das entidades em arquivos separados).
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
