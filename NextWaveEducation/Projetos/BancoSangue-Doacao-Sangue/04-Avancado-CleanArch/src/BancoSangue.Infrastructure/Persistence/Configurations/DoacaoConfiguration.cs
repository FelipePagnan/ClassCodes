using BancoSangue.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BancoSangue.Infrastructure.Persistence.Configurations;

public class DoacaoConfiguration : IEntityTypeConfiguration<Doacao>
{
    public void Configure(EntityTypeBuilder<Doacao> builder)
    {
        builder.ToTable("Doacoes");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.DoadorId).IsRequired();
        builder.Property(d => d.DataDoacao).IsRequired();
        builder.Property(d => d.QuantidadeML).IsRequired();

        builder.HasIndex(d => d.DataDoacao)
            .HasDatabaseName("IX_Doacoes_DataDoacao");

        builder.HasIndex(d => d.DoadorId)
            .HasDatabaseName("IX_Doacoes_DoadorId");

        builder.Ignore(d => d.DomainEvents);
    }
}
