using CRMDev.Domain.Entities;
using CRMDev.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CRMDev.Infrastructure.Data;

public class CRMDevDbContext : IdentityDbContext<ApplicationUser>
{
    public CRMDevDbContext(DbContextOptions<CRMDevDbContext> options) : base(options)
    {
    }

    public DbSet<Contato> Contatos => Set<Contato>();
    public DbSet<Oportunidade> Oportunidades => Set<Oportunidade>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ContatoTag> ContatoTags => Set<ContatoTag>();
    public DbSet<AreaAtuacao> AreasAtuacao => Set<AreaAtuacao>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ContatoTag>(entity =>
        {
            entity.HasKey(ct => new { ct.ContatoId, ct.TagId });

            entity.HasOne(ct => ct.Contato)
                .WithMany(c => c.ContatoTags)
                .HasForeignKey(ct => ct.ContatoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ct => ct.Tag)
                .WithMany(t => t.ContatoTags)
                .HasForeignKey(ct => ct.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Contato>(entity =>
        {
            entity.Property(c => c.Nome).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(c => c.Email).IsUnique();

            entity.HasOne(c => c.AreaAtuacao)
                .WithMany(a => a.Contatos)
                .HasForeignKey(c => c.AreaAtuacaoId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Oportunidade>(entity =>
        {
            entity.Property(o => o.Titulo).IsRequired().HasMaxLength(200);
            entity.Property(o => o.Orcamento).HasColumnType("decimal(18,2)");

            entity.HasOne(o => o.Contato)
                .WithMany(c => c.Oportunidades)
                .HasForeignKey(o => o.ContatoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Tag>(entity =>
        {
            entity.Property(t => t.Nome).IsRequired().HasMaxLength(100);
            entity.HasIndex(t => t.Nome).IsUnique();
        });

        builder.Entity<AreaAtuacao>(entity =>
        {
            entity.Property(a => a.Nome).IsRequired().HasMaxLength(150);
            entity.HasIndex(a => a.Nome).IsUnique();
        });
    }
}
