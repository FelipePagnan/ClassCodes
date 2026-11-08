using Clinica.Dominio.Entidades;
using Clinica.Dominio.ObjetosDeValor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinica.Infraestrutura.Persistencia.Mapeamentos;

public class MedicoConfiguration : IEntityTypeConfiguration<Medico>
{
    public void Configure(EntityTypeBuilder<Medico> builder)
    {
        builder.ToTable("Medicos");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.NomeCompleto).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Especialidade).IsRequired().HasMaxLength(120);

        // Crm tem dois campos (número e UF), então não cabe numa conversão
        // para uma coluna: entra como tipo possuído, que gera duas colunas
        // na própria tabela de médicos.
        builder.OwnsOne(m => m.Crm, crm =>
        {
            crm.Property(c => c.Numero)
                .HasColumnName("Crm")
                .IsRequired()
                .HasMaxLength(7);

            crm.Property(c => c.Uf)
                .HasColumnName("UfCrm")
                .IsRequired()
                .HasMaxLength(2);

            crm.Ignore(c => c.Completo);

            // O CRM é único dentro da UF, não globalmente: existe
            // CRM/SP 123456 e CRM/RJ 123456, de médicos diferentes.
            crm.HasIndex(c => new { c.Numero, c.Uf })
                .HasDatabaseName("IX_Medicos_Crm_UfCrm");
        });

        builder.Property(m => m.Email)
            .HasConversion(email => email!.Valor, valor => Email.Reconstruir(valor))
            .HasColumnName("Email")
            .HasMaxLength(200);

        builder.Property(m => m.Celular)
            .HasConversion(celular => celular!.Valor, valor => Celular.Reconstruir(valor))
            .HasColumnName("Celular")
            .HasMaxLength(11);

        builder.HasIndex(m => m.Especialidade);

        builder.Metadata
            .FindNavigation(nameof(Medico.Atendimentos))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // A lista de eventos de domínio é estado em memória, não coluna:
        // o Unit of Work a consome e descarta depois de publicar.
        builder.Ignore(m => m.Eventos);

        builder.HasQueryFilter(m => !m.EstaDeletado);
    }
}
