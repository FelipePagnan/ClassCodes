using Caixinhas.Intermediario.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Caixinhas.Intermediario.Infraestrutura.Dados.Configuracoes;

public class ObjetivoFinanceiroConfiguration : IEntityTypeConfiguration<ObjetivoFinanceiro>
{
    public void Configure(EntityTypeBuilder<ObjetivoFinanceiro> builder)
    {
        builder.ToTable("Objetivos");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Titulo)
            .IsRequired()
            .HasMaxLength(120);

        // Dinheiro sempre com 2 casas decimais na coluna.
        builder.Property(o => o.QuantidadeAlvo)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.ValorTotal)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.QuantidadeIdealAporteMensal)
            .HasPrecision(18, 2);

        builder.Property(o => o.Prazo);

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(o => o.DataCriacao).IsRequired();
        builder.Property(o => o.EstaDeletado).IsRequired();

        // A capa é gravada como BLOB.
        builder.Property(o => o.CapaObjetivo)
            .HasColumnType("BLOB");

        builder.HasMany(o => o.Transacoes)
            .WithOne(t => t.ObjetivoFinanceiro)
            .HasForeignKey(t => t.ObjetivoFinanceiroId)
            .OnDelete(DeleteBehavior.Cascade);

        // A coleção é somente leitura, então o EF escreve direto no campo.
        builder.Metadata
            .FindNavigation(nameof(ObjetivoFinanceiro.Transacoes))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Soft delete: objetivos deletados somem das consultas automaticamente.
        builder.HasQueryFilter(o => !o.EstaDeletado);

        builder.HasIndex(o => o.Titulo).HasDatabaseName("IX_Objetivos_Titulo");
        builder.HasIndex(o => o.Status).HasDatabaseName("IX_Objetivos_Status");

        // Propriedades calculadas não têm coluna.
        builder.Ignore(o => o.TemCapa);
        builder.Ignore(o => o.TotalTransacoes);
        builder.Ignore(o => o.ValorRestante);
        builder.Ignore(o => o.PercentualConcluido);
        builder.Ignore(o => o.MetaAlcancada);
        builder.Ignore(o => o.DiasRestantes);
        builder.Ignore(o => o.PrazoVencido);
        builder.Ignore(o => o.AporteMensalNecessario);
        builder.Ignore(o => o.AceitaTransacoes);
    }
}
