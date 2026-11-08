using BancoSangue.Intermediario.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BancoSangue.Intermediario.Infraestrutura.Dados.Configuracoes;

public class EnderecoConfiguration : IEntityTypeConfiguration<Endereco>
{
    public void Configure(EntityTypeBuilder<Endereco> builder)
    {
        builder.ToTable("Enderecos");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Logradouro)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(e => e.Cidade)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(e => e.Estado)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(e => e.CEP)
            .IsRequired()
            .HasMaxLength(9);

        builder.HasIndex(e => e.DoadorId).IsUnique();
    }
}
