using Clinica.Api.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinica.Api.Infraestrutura.Mapeamentos;

public class AtendimentoConfiguration : IEntityTypeConfiguration<Atendimento>
{
    public void Configure(EntityTypeBuilder<Atendimento> builder)
    {
        builder.ToTable("Atendimentos");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.DataHora).IsRequired();
        builder.Property(a => a.DuracaoMinutos).IsRequired();
        builder.Property(a => a.Valor).HasPrecision(18, 2).IsRequired();
        builder.Property(a => a.Observacoes).HasMaxLength(1000);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        // Restrict em vez de Cascade: ninguém apaga um paciente e leva o
        // histórico clínico junto. Aqui a remoção é sempre lógica.
        builder.HasOne(a => a.Paciente)
            .WithMany(p => p.Atendimentos)
            .HasForeignKey(a => a.PacienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Medico)
            .WithMany(m => m.Atendimentos)
            .HasForeignKey(a => a.MedicoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Servico)
            .WithMany(s => s.Atendimentos)
            .HasForeignKey(a => a.ServicoId)
            .OnDelete(DeleteBehavior.Restrict);

        // A agenda é consultada por médico e data; o lembrete, por data.
        builder.HasIndex(a => new { a.MedicoId, a.DataHora })
            .HasDatabaseName("IX_Atendimentos_MedicoId_DataHora");

        builder.HasIndex(a => new { a.PacienteId, a.DataHora })
            .HasDatabaseName("IX_Atendimentos_PacienteId_DataHora");

        builder.HasIndex(a => a.DataHora).HasDatabaseName("IX_Atendimentos_DataHora");

        builder.Metadata
            .FindNavigation(nameof(Atendimento.Anexos))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(a => a.DataHoraFim);
        builder.Ignore(a => a.EstaAberto);
        builder.Ignore(a => a.EstaEncerrado);
        builder.Ignore(a => a.EhHoje);
        builder.Ignore(a => a.EhAmanha);
        builder.Ignore(a => a.JaPassou);

        builder.HasQueryFilter(a => !a.EstaDeletado);
    }
}
