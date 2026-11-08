using BancoSangue.Domain.Entities;
using BancoSangue.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BancoSangue.Infrastructure.Persistence.Configurations;

public class EstoqueSangueConfiguration : IEntityTypeConfiguration<EstoqueSangue>
{
    public void Configure(EntityTypeBuilder<EstoqueSangue> builder)
    {
        builder.ToTable("EstoquesSangue");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TipoSanguineo)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(5);

        builder.Property(e => e.FatorRh)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(e => e.QuantidadeML).IsRequired();

        builder.HasIndex(e => new { e.TipoSanguineo, e.FatorRh })
            .IsUnique()
            .HasDatabaseName("IX_EstoquesSangue_Tipo_Fator");

        builder.Ignore(e => e.Descricao);
        builder.Ignore(e => e.EstaAbaixoDoMinimo);
        builder.Ignore(e => e.DomainEvents);

        // Semeia as 8 combinações de tipo sanguíneo + fator Rh, zeradas.
        var id = 1;
        var seed = new List<object>();
        foreach (TipoSanguineo tipo in Enum.GetValues<TipoSanguineo>())
            foreach (FatorRh fator in Enum.GetValues<FatorRh>())
                seed.Add(new
                {
                    Id = id++,
                    TipoSanguineo = tipo,
                    FatorRh = fator,
                    QuantidadeML = 0
                });

        builder.HasData(seed);
    }
}
