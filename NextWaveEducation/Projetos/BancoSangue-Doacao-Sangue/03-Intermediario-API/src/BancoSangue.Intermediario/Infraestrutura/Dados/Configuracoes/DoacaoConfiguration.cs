using BancoSangue.Intermediario.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BancoSangue.Intermediario.Infraestrutura.Dados.Configuracoes;

public class DoacaoConfiguration : IEntityTypeConfiguration<Doacao>
{
    public void Configure(EntityTypeBuilder<Doacao> builder)
    {
        builder.ToTable("Doacoes");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.DataDoacao).IsRequired();
        builder.Property(d => d.QuantidadeML).IsRequired();
        builder.Property(d => d.DoadorId).IsRequired();

        builder.HasIndex(d => d.DataDoacao)
            .HasDatabaseName("IX_Doacoes_DataDoacao");
    }
}
