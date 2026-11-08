using BancoSangue.Intermediario.Dominio.Entidades;
using BancoSangue.Intermediario.Dominio.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BancoSangue.Intermediario.Infraestrutura.Dados.Configuracoes;

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

        builder.HasIndex(e => new { e.TipoSanguineo, e.FatorRh }).IsUnique();

        builder.Ignore(e => e.Descricao);
        builder.Ignore(e => e.EstoqueBaixo);

        // Semeia as 8 combinações de tipo sanguíneo + fator Rh.
        var id = 1;
        var dados = new List<object>();
        foreach (TipoSanguineo tipo in Enum.GetValues<TipoSanguineo>())
            foreach (FatorRh fator in Enum.GetValues<FatorRh>())
                dados.Add(new { Id = id++, TipoSanguineo = tipo, FatorRh = fator, QuantidadeML = 0 });

        builder.HasData(dados);
    }
}
