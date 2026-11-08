using Livros.Intermediario.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Livros.Intermediario.Infraestrutura.Dados.Configuracoes;

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

        builder.Property(l => l.ISBN)
            .IsRequired()
            .HasMaxLength(20);

        // Regra de negócio: livros não podem ser cadastrados com o mesmo ISBN.
        builder.HasIndex(l => l.ISBN)
            .IsUnique()
            .HasDatabaseName("IX_Livros_ISBN_Unico");

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

        builder.HasIndex(l => l.Titulo).HasDatabaseName("IX_Livros_Titulo");
        builder.HasIndex(l => l.Autor).HasDatabaseName("IX_Livros_Autor");

        // Propriedades calculadas não têm coluna.
        builder.Ignore(l => l.TemCapa);
        builder.Ignore(l => l.IsbnNormalizado);
    }
}
