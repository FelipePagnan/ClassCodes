using Livros.Intermediario.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Livros.Intermediario.Infraestrutura.Dados.Configuracoes;

public class AvaliacaoConfiguration : IEntityTypeConfiguration<Avaliacao>
{
    public void Configure(EntityTypeBuilder<Avaliacao> builder)
    {
        builder.ToTable("Avaliacoes");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Nota).IsRequired();

        builder.Property(a => a.Descricao)
            .HasMaxLength(2000);

        builder.Property(a => a.IdLivro).IsRequired();
        builder.Property(a => a.IdUsuario).IsRequired();
        builder.Property(a => a.DataInicioLeitura).IsRequired();
        builder.Property(a => a.DataFimLeitura).IsRequired();
        builder.Property(a => a.DataCriacao).IsRequired();

        // Um usuário só pode avaliar o mesmo livro uma vez.
        builder.HasIndex(a => new { a.IdLivro, a.IdUsuario })
            .IsUnique()
            .HasDatabaseName("IX_Avaliacoes_Livro_Usuario_Unico");

        // Usado pelo relatório de livros lidos por ano.
        builder.HasIndex(a => a.DataFimLeitura)
            .HasDatabaseName("IX_Avaliacoes_DataFimLeitura");

        builder.Ignore(a => a.DiasDeLeitura);
    }
}
