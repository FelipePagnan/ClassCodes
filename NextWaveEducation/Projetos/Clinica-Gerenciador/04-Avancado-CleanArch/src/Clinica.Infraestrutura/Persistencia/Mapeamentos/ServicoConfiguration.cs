using Clinica.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinica.Infraestrutura.Persistencia.Mapeamentos;

public class ServicoConfiguration : IEntityTypeConfiguration<Servico>
{
    public void Configure(EntityTypeBuilder<Servico> builder)
    {
        builder.ToTable("Servicos");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Nome).IsRequired().HasMaxLength(150);
        builder.Property(s => s.Descricao).HasMaxLength(500);

        // Dinheiro com precisão explícita: decimal sem precisão definida
        // escolhe um padrão do provider, e em dinheiro isso é arredondamento
        // onde não deveria haver.
        builder.Property(s => s.Valor).HasPrecision(18, 2).IsRequired();
        builder.Property(s => s.DuracaoMinutos).IsRequired();

        builder.HasIndex(s => s.Nome).IsUnique().HasDatabaseName("IX_Servicos_Nome");

        builder.Metadata
            .FindNavigation(nameof(Servico.Atendimentos))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(s => s.Duracao);

        // A lista de eventos de domínio é estado em memória, não coluna:
        // o Unit of Work a consome e descarta depois de publicar.
        builder.Ignore(s => s.Eventos);

        builder.HasQueryFilter(s => !s.EstaDeletado);
    }
}
