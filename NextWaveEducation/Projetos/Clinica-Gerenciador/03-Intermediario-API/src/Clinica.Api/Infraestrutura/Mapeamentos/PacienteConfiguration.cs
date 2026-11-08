using Clinica.Api.Dominio.Entidades;
using Clinica.Api.Dominio.ObjetosDeValor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinica.Api.Infraestrutura.Mapeamentos;

/// <summary>
/// Mapeamento do paciente. Cada entidade tem a sua configuração num arquivo
/// próprio: o OnModelCreating para de crescer e fica fácil achar onde um
/// campo é configurado.
/// </summary>
public class PacienteConfiguration : IEntityTypeConfiguration<Paciente>
{
    public void Configure(EntityTypeBuilder<Paciente> builder)
    {
        builder.ToTable("Pacientes");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.NomeCompleto).IsRequired().HasMaxLength(200);
        builder.Property(p => p.DataNascimento).IsRequired();
        builder.Property(p => p.Observacoes).HasMaxLength(1000);

        // Enum como texto: a tabela fica legível e inserir um valor novo no
        // meio do enum não embaralha os registros já gravados.
        builder.Property(p => p.Sexo).HasConversion<string>().HasMaxLength(20);

        // Objetos de valor viram uma coluna só, convertidos na ida e na
        // volta. Reconstruir não revalida: o dado já foi validado ao entrar.
        builder.Property(p => p.Cpf)
            .HasConversion(cpf => cpf.Valor, valor => Cpf.Reconstruir(valor))
            .HasColumnName("Cpf")
            .IsRequired()
            .HasMaxLength(11);

        builder.Property(p => p.Celular)
            .HasConversion(celular => celular.Valor, valor => Celular.Reconstruir(valor))
            .HasColumnName("Celular")
            .IsRequired()
            .HasMaxLength(11);

        builder.Property(p => p.Email)
            .HasConversion(email => email!.Valor, valor => Email.Reconstruir(valor))
            .HasColumnName("Email")
            .HasMaxLength(200);

        // As duas buscas exigidas pela especificação.
        builder.HasIndex(p => p.Cpf).HasDatabaseName("IX_Pacientes_Cpf");
        builder.HasIndex(p => p.Celular).HasDatabaseName("IX_Pacientes_Celular");

        // A coleção é IReadOnlyCollection apoiada num campo privado, então o
        // EF precisa escrever no campo e não na propriedade.
        builder.Metadata
            .FindNavigation(nameof(Paciente.Atendimentos))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(p => p.Idade);
        builder.Ignore(p => p.EhMenorDeIdade);
        builder.Ignore(p => p.PrimeiroNome);

        // Remoção lógica como filtro global: nenhuma consulta precisa
        // lembrar de excluir os removidos.
        builder.HasQueryFilter(p => !p.EstaDeletado);
    }
}
