using Livros.Api.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Livros.Api.Dados;

public class LivrosContext : DbContext
{
    public LivrosContext(DbContextOptions<LivrosContext> options) : base(options) { }

    public DbSet<Livro> Livros => Set<Livro>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Avaliacao> Avaliacoes => Set<Avaliacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Regra de negócio: não permitir dois livros com o mesmo ISBN.
        modelBuilder.Entity<Livro>()
            .HasIndex(l => l.ISBN)
            .IsUnique();

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Livro>()
            .HasMany(l => l.Avaliacoes)
            .WithOne(a => a.Livro)
            .HasForeignKey(a => a.IdLivro)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Usuario>()
            .HasMany(u => u.Avaliacoes)
            .WithOne(a => a.Usuario)
            .HasForeignKey(a => a.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);

        // Um usuário só avalia o mesmo livro uma vez.
        modelBuilder.Entity<Avaliacao>()
            .HasIndex(a => new { a.IdLivro, a.IdUsuario })
            .IsUnique();

        // Propriedades calculadas não têm coluna no banco.
        modelBuilder.Entity<Livro>().Ignore(l => l.TemCapa);
        modelBuilder.Entity<Usuario>().Ignore(u => u.TotalAvaliacoes);
        modelBuilder.Entity<Usuario>().Ignore(u => u.NotaMediaAtribuida);
        modelBuilder.Entity<Avaliacao>().Ignore(a => a.DiasDeLeitura);

        base.OnModelCreating(modelBuilder);
    }
}
