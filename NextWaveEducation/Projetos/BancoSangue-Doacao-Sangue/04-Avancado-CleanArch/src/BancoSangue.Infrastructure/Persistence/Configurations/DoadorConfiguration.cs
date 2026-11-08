using BancoSangue.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BancoSangue.Infrastructure.Persistence.Configurations;

public class DoadorConfiguration : IEntityTypeConfiguration<Doador>
{
    public void Configure(EntityTypeBuilder<Doador> builder)
    {
        builder.ToTable("Doadores");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.NomeCompleto)
            .IsRequired()
            .HasMaxLength(200);

        // O domínio expõe o e-mail como Value Object (Email); a coluna persiste a string.
        builder.Property(d => d.EmailValor)
            .HasColumnName("Email")
            .IsRequired()
            .HasMaxLength(150);

        // Regra de negócio: não permitir dois doadores com o mesmo e-mail.
        builder.HasIndex(d => d.EmailValor)
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

        // A coleção Doacoes é somente leitura (IReadOnlyCollection) e o EF precisa
        // escrever direto no campo _doacoes em vez de usar a propriedade.
        builder.Metadata
            .FindNavigation(nameof(Doador.Doacoes))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne(d => d.Endereco)
            .WithOne(e => e.Doador)
            .HasForeignKey<Endereco>(e => e.DoadorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Propriedades calculadas e eventos não são persistidos.
        builder.Ignore(d => d.Email);
        builder.Ignore(d => d.Idade);
        builder.Ignore(d => d.EhMaiorDeIdade);
        builder.Ignore(d => d.TipoSanguineoCompleto);
        builder.Ignore(d => d.UltimaDoacao);
        builder.Ignore(d => d.IntervaloMinimoDias);
        builder.Ignore(d => d.ProximaDoacaoPermitida);
        builder.Ignore(d => d.EstaAptoParaDoar);
        builder.Ignore(d => d.DomainEvents);
    }
}
