using Livros.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Livros.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nome)
            .IsRequired()
            .HasMaxLength(200);

        // O domínio expõe o e-mail como Value Object; a coluna persiste a string.
        builder.Property(u => u.EmailValor)
            .HasColumnName("Email")
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(u => u.EmailValor)
            .IsUnique()
            .HasDatabaseName("IX_Usuarios_Email_Unico");

        builder.Property(u => u.DataCriacao).IsRequired();

        builder.HasMany(u => u.Avaliacoes)
            .WithOne(a => a.Usuario)
            .HasForeignKey(a => a.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Usuario.Avaliacoes))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(u => u.Email);
        builder.Ignore(u => u.TotalAvaliacoes);
        builder.Ignore(u => u.NotaMediaAtribuida);
        builder.Ignore(u => u.TotalPaginasLidas);
        builder.Ignore(u => u.DomainEvents);
    }
}
