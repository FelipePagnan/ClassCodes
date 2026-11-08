using Clinica.Api.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinica.Api.Infraestrutura.Mapeamentos;

public class AnexoConfiguration : IEntityTypeConfiguration<Anexo>
{
    public void Configure(EntityTypeBuilder<Anexo> builder)
    {
        builder.ToTable("Anexos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.NomeArquivo).IsRequired().HasMaxLength(255);
        builder.Property(x => x.TipoConteudo).IsRequired().HasMaxLength(120);
        builder.Property(x => x.Conteudo).IsRequired();
        builder.Property(x => x.Descricao).HasMaxLength(4000);
        builder.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasOne(x => x.Atendimento)
            .WithMany(a => a.Anexos)
            .HasForeignKey(x => x.AtendimentoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.AtendimentoId, x.Tipo })
            .HasDatabaseName("IX_Anexos_AtendimentoId_Tipo");

        builder.Ignore(x => x.TamanhoBytes);
        builder.Ignore(x => x.TamanhoLegivel);

        builder.HasQueryFilter(x => !x.EstaDeletado);
    }
}
