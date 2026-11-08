using EduPlatform.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EduPlatform.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Assinatura> Assinaturas => Set<Assinatura>();
    public DbSet<UsuarioAssinatura> UsuarioAssinaturas => Set<UsuarioAssinatura>();
    public DbSet<PagamentoAssinatura> PagamentoAssinaturas => Set<PagamentoAssinatura>();
    public DbSet<Curso> Cursos => Set<Curso>();
    public DbSet<Modulo> Modulos => Set<Modulo>();
    public DbSet<Aula> Aulas => Set<Aula>();
    public DbSet<UsuarioAulaConcluida> UsuarioAulasConcluidas => Set<UsuarioAulaConcluida>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role).HasConversion<string>();
        });

        modelBuilder.Entity<Modulo>()
            .HasOne(m => m.Curso)
            .WithMany(c => c.Modulos)
            .HasForeignKey(m => m.IdCurso)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Aula>()
            .HasOne(a => a.Modulo)
            .WithMany(m => m.Aulas)
            .HasForeignKey(a => a.IdModulo)
            .OnDelete(DeleteBehavior.Cascade);

        // Assinatura <-> Curso (N:N)
        modelBuilder.Entity<Assinatura>()
            .HasMany(a => a.Cursos)
            .WithMany(c => c.Assinaturas)
            .UsingEntity(j => j.ToTable("AssinaturaCurso"));

        modelBuilder.Entity<UsuarioAssinatura>(e =>
        {
            e.Property(u => u.Status).HasConversion<string>();
            e.HasOne(u => u.Usuario)
                .WithMany(u => u.Assinaturas)
                .HasForeignKey(u => u.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(u => u.Assinatura)
                .WithMany(a => a.UsuarioAssinaturas)
                .HasForeignKey(u => u.IdAssinatura)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PagamentoAssinatura>(e =>
        {
            e.Property(p => p.Status).HasConversion<string>();
            e.Property(p => p.Valor).HasColumnType("decimal(18,2)");
            e.HasOne(p => p.UsuarioAssinatura)
                .WithMany(u => u.Pagamentos)
                .HasForeignKey(p => p.IdUsuarioAssinatura)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UsuarioAulaConcluida>(e =>
        {
            e.HasIndex(x => new { x.IdUsuario, x.IdAula }).IsUnique();
            e.HasOne(x => x.Usuario)
                .WithMany(u => u.AulasConcluidas)
                .HasForeignKey(x => x.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Aula)
                .WithMany(a => a.Conclusoes)
                .HasForeignKey(x => x.IdAula)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Assinatura>()
            .Property(a => a.Valor).HasColumnType("decimal(18,2)");
    }
}
