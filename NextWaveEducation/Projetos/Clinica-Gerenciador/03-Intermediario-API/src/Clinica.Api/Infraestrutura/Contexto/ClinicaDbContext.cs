using Clinica.Api.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Api.Infraestrutura.Contexto;

/// <summary>
/// Contexto do Entity Framework Core sobre SQLite.
///
/// Duas coisas mudaram em relação ao Básico II:
///   - o mapeamento saiu daqui e foi para classes IEntityTypeConfiguration,
///     uma por entidade, carregadas por ApplyConfigurationsFromAssembly;
///   - a remoção lógica virou filtro global (HasQueryFilter), aplicado nas
///     configurações. Assim nenhuma consulta precisa lembrar de escrever
///     "where EstaDeletado = 0" — esquecer disso uma vez já é um vazamento
///     de dado removido.
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
}
