using Clinica.Dominio.Entidades;
using Clinica.Dominio.ObjetosDeValor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinica.Infraestrutura.Persistencia.Mapeamentos;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nome).IsRequired().HasMaxLength(200);
        builder.Property(u => u.SenhaHash).IsRequired().HasMaxLength(200);
        builder.Property(u => u.SenhaSalt).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Perfil).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(u => u.Ativo).IsRequired();

        builder.Property(u => u.Email)
            .HasConversion(email => email.Valor, valor => Email.Reconstruir(valor))
            .HasColumnName("Email")
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(u => u.Email).IsUnique().HasDatabaseName("IX_Usuarios_Email");

        builder.HasOne(u => u.Medico)
            .WithMany()
            .HasForeignKey(u => u.MedicoId)
            .OnDelete(DeleteBehavior.Restrict);

        // A lista de eventos de domínio é estado em memória, não coluna:
        // o Unit of Work a consome e descarta depois de publicar.
        builder.Ignore(u => u.Eventos);

        builder.HasQueryFilter(u => !u.EstaDeletado);
    }
}
