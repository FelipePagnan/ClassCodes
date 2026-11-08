using Caixinhas.Intermediario.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Caixinhas.Intermediario.Infraestrutura.Dados.Configuracoes;

public class TransacaoConfiguration : IEntityTypeConfiguration<Transacao>
{
    public void Configure(EntityTypeBuilder<Transacao> builder)
    {
        builder.ToTable("Transacoes");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Quantidade)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(t => t.Tipo)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.DataTransacao).IsRequired();
        builder.Property(t => t.DataCriacao).IsRequired();
        builder.Property(t => t.ObjetivoFinanceiroId).IsRequired();
        builder.Property(t => t.EstaDeletado).IsRequired();

        // Soft delete: transações deletadas somem das consultas e dos Includes.
        builder.HasQueryFilter(t => !t.EstaDeletado);

        builder.HasIndex(t => t.ObjetivoFinanceiroId)
            .HasDatabaseName("IX_Transacoes_ObjetivoId");

        builder.HasIndex(t => t.DataTransacao)
            .HasDatabaseName("IX_Transacoes_DataTransacao");

        builder.Ignore(t => t.Efeito);
        builder.Ignore(t => t.EhDeposito);
        builder.Ignore(t => t.Valor);
    }
}
