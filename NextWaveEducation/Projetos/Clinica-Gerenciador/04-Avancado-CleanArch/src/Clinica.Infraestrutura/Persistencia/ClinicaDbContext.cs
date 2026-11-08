using Clinica.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Clinica.Infraestrutura.Persistencia;

/// <summary>
/// Contexto do Entity Framework Core sobre SQLite.
///
/// Repare que este arquivo está na infraestrutura e o domínio não o conhece:
/// as entidades não têm atributo de mapeamento nenhum, e toda a configuração
/// vive nas classes IEntityTypeConfiguration ao lado.
/// </summary>
public class ClinicaDbContext : DbContext
{
    public ClinicaDbContext(DbContextOptions<ClinicaDbContext> options) : base(options) { }

    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Medico> Medicos => Set<Medico>();
    public DbSet<Servico> Servicos => Set<Servico>();
    public DbSet<Atendimento> Atendimentos => Set<Atendimento>();
    public DbSet<Anexo> Anexos => Set<Anexo>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClinicaDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // O EF avisa quando um dependente obrigatório e o principal têm, os
        // dois, filtro global de consulta — caso de Anexo e Atendimento com a
        // remoção lógica. O aviso está certo em geral, mas aqui o
        // comportamento é o desejado: anexo de atendimento removido também
        // some das consultas.
        optionsBuilder.ConfigureWarnings(avisos => avisos.Ignore(
            CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning));

        base.OnConfiguring(optionsBuilder);
    }
}
