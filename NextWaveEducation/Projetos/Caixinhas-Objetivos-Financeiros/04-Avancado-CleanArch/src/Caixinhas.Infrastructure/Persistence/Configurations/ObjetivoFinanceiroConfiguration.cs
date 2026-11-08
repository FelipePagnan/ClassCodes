using Caixinhas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Caixinhas.Infrastructure.Persistence.Configurations;

public class ObjetivoFinanceiroConfiguration : IEntityTypeConfiguration<ObjetivoFinanceiro>
{
    public void Configure(EntityTypeBuilder<ObjetivoFinanceiro> builder)
    {
        builder.ToTable("Objetivos");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Titulo)
            .IsRequired()
            .HasMaxLength(120);

        // Dinheiro sempre com 2 casas decimais na coluna, espelhando a regra
        // de negócio que o Value Object Dinheiro aplica no domínio.
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
            .WithOne(t => t.Objetivo)
            .HasForeignKey(t => t.ObjetivoFinanceiroId)
            .OnDelete(DeleteBehavior.Cascade);

        // A coleção é somente leitura (IReadOnlyCollection), então o EF
        // escreve direto no campo _transacoes.
        builder.Metadata
            .FindNavigation(nameof(ObjetivoFinanceiro.Transacoes))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Soft delete: objetivos deletados somem das consultas automaticamente.
        builder.HasQueryFilter(o => !o.EstaDeletado);

        builder.HasIndex(o => o.Titulo).HasDatabaseName("IX_Objetivos_Titulo");
        builder.HasIndex(o => o.Status).HasDatabaseName("IX_Objetivos_Status");

        // Propriedades calculadas, Value Objects e eventos não são colunas.
        builder.Ignore(o => o.Saldo);
        builder.Ignore(o => o.Meta);
        builder.Ignore(o => o.TransacoesAtivas);
        builder.Ignore(o => o.TemCapa);
        builder.Ignore(o => o.TotalTransacoes);
        builder.Ignore(o => o.ValorRestante);
        builder.Ignore(o => o.PercentualConcluido);
        builder.Ignore(o => o.MetaAlcancada);
        builder.Ignore(o => o.DiasRestantes);
        builder.Ignore(o => o.PrazoVencido);
        builder.Ignore(o => o.AporteMensalNecessario);
        builder.Ignore(o => o.AceitaTransacoes);
        builder.Ignore(o => o.DomainEvents);
    }
}
