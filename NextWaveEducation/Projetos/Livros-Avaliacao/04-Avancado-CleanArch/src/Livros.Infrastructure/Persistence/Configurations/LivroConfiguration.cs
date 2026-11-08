using Livros.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Livros.Infrastructure.Persistence.Configurations;

public class LivroConfiguration : IEntityTypeConfiguration<Livro>
{
    public void Configure(EntityTypeBuilder<Livro> builder)
    {
        builder.ToTable("Livros");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Titulo)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(l => l.Descricao)
            .HasMaxLength(4000);

        // O domínio expõe o ISBN como Value Object; estas duas colunas o persistem.
        builder.Property(l => l.IsbnValor)
            .HasColumnName("ISBN")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(l => l.IsbnNormalizado)
            .HasColumnName("IsbnNormalizado")
            .IsRequired()
            .HasMaxLength(20);

        // Regra de negócio: livros não podem ser cadastrados com o mesmo ISBN.
        // O índice é sobre a versão normalizada, então "978-0321125217" e
        // "9780321125217" colidem corretamente.
        builder.HasIndex(l => l.IsbnNormalizado)
            .IsUnique()
            .HasDatabaseName("IX_Livros_Isbn_Unico");

        builder.Property(l => l.Autor)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.Editora)
            .HasMaxLength(150);

        builder.Property(l => l.Genero)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(l => l.AnoDePublicacao).IsRequired();
        builder.Property(l => l.QuantidadePaginas).IsRequired();
        builder.Property(l => l.DataCriacao).IsRequired();

        builder.Property(l => l.NotaMedia)
            .IsRequired()
            .HasPrecision(4, 2);

        // A capa é gravada como BLOB.
        builder.Property(l => l.CapaLivro)
            .HasColumnType("BLOB");

        builder.HasMany(l => l.Avaliacoes)
            .WithOne(a => a.Livro)
            .HasForeignKey(a => a.IdLivro)
            .OnDelete(DeleteBehavior.Cascade);

        // A coleção Avaliacoes é somente leitura (IReadOnlyCollection) e o EF
        // precisa escrever direto no campo _avaliacoes.
        builder.Metadata
            .FindNavigation(nameof(Livro.Avaliacoes))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(l => l.Titulo).HasDatabaseName("IX_Livros_Titulo");
        builder.HasIndex(l => l.Autor).HasDatabaseName("IX_Livros_Autor");

        // Propriedades calculadas e eventos não são persistidos.
        builder.Ignore(l => l.Isbn);
        builder.Ignore(l => l.TemCapa);
        builder.Ignore(l => l.TotalAvaliacoes);
        builder.Ignore(l => l.DistribuicaoDeNotas);
        builder.Ignore(l => l.DomainEvents);
    }
}
