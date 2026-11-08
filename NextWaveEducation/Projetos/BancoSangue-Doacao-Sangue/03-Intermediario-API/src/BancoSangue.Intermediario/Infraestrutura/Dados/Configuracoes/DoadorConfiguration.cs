using BancoSangue.Intermediario.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BancoSangue.Intermediario.Infraestrutura.Dados.Configuracoes;

public class DoadorConfiguration : IEntityTypeConfiguration<Doador>
{
    public void Configure(EntityTypeBuilder<Doador> builder)
    {
        builder.ToTable("Doadores");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.NomeCompleto)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Email)
            .IsRequired()
            .HasMaxLength(150);

        // Regra de negócio: não permitir dois doadores com o mesmo e-mail.
        builder.HasIndex(d => d.Email)
            .IsUnique()
            .HasDatabaseName("IX_Doadores_Email_Unico");

        builder.Property(d => d.DataNascimento).IsRequired();

        builder.Property(d => d.Genero)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(d => d.Peso).IsRequired();

        builder.Property(d => d.TipoSanguineo)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(5);

        builder.Property(d => d.FatorRh)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.HasMany(d => d.Doacoes)
            .WithOne(d => d.Doador)
            .HasForeignKey(d => d.DoadorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Endereco)
            .WithOne(e => e.Doador)
            .HasForeignKey<Endereco>(e => e.DoadorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Propriedades calculadas não são persistidas.
        builder.Ignore(d => d.Idade);
        builder.Ignore(d => d.EhMaiorDeIdade);
        builder.Ignore(d => d.UltimaDoacao);
        builder.Ignore(d => d.IntervaloMinimoDias);
        builder.Ignore(d => d.TipoSanguineoCompleto);
    }
}
