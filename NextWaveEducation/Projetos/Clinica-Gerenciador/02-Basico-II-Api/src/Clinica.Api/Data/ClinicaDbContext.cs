using Clinica.Api.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Api.Data;

/// <summary>
/// Contexto do Entity Framework Core. Neste nível o provider é InMemory, então
/// os dados vivem enquanto a aplicação estiver rodando.
///
/// O mapeamento está todo aqui dentro, no OnModelCreating — no nível
/// Intermediário ele é quebrado em arquivos com IEntityTypeConfiguration.
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
        // ----------------------------- Paciente -----------------------------
        modelBuilder.Entity<Paciente>(entidade =>
        {
            entidade.HasKey(p => p.Id);
            entidade.Property(p => p.NomeCompleto).IsRequired().HasMaxLength(200);
            entidade.Property(p => p.Cpf).IsRequired().HasMaxLength(11);
            entidade.Property(p => p.Celular).IsRequired().HasMaxLength(11);
            entidade.Property(p => p.Email).HasMaxLength(200);
            entidade.Property(p => p.Observacoes).HasMaxLength(1000);

            // Enum gravado como texto: a tabela fica legível e um valor novo
            // no meio do enum não embaralha os registros já salvos.
            entidade.Property(p => p.Sexo).HasConversion<string>().HasMaxLength(20);

            entidade.HasIndex(p => p.Cpf);
            entidade.HasIndex(p => p.Celular);

            // Propriedades calculadas não existem no banco: são derivadas
            // dos dados que já estão lá.
            entidade.Ignore(p => p.CpfFormatado);
            entidade.Ignore(p => p.CelularFormatado);
            entidade.Ignore(p => p.Idade);
            entidade.Ignore(p => p.EhMenorDeIdade);
            entidade.Ignore(p => p.AtendimentosAtivos);
        });

        // ------------------------------ Medico ------------------------------
        modelBuilder.Entity<Medico>(entidade =>
        {
            entidade.HasKey(m => m.Id);
            entidade.Property(m => m.NomeCompleto).IsRequired().HasMaxLength(200);
            entidade.Property(m => m.Crm).IsRequired().HasMaxLength(7);
            entidade.Property(m => m.UfCrm).IsRequired().HasMaxLength(2);
            entidade.Property(m => m.Especialidade).IsRequired().HasMaxLength(120);
            entidade.Property(m => m.Email).HasMaxLength(200);
            entidade.Property(m => m.Celular).HasMaxLength(11);

            // CRM é único dentro da UF, não globalmente.
            entidade.HasIndex(m => new { m.Crm, m.UfCrm });

            entidade.Ignore(m => m.CrmCompleto);
            entidade.Ignore(m => m.CelularFormatado);
            entidade.Ignore(m => m.AtendimentosAtivos);
        });

        // ------------------------------ Servico ------------------------------
        modelBuilder.Entity<Servico>(entidade =>
        {
            entidade.HasKey(s => s.Id);
            entidade.Property(s => s.Nome).IsRequired().HasMaxLength(150);
            entidade.Property(s => s.Descricao).HasMaxLength(500);
            entidade.Property(s => s.Valor).HasPrecision(18, 2);

            entidade.Ignore(s => s.Duracao);
        });

        // ---------------------------- Atendimento ----------------------------
        modelBuilder.Entity<Atendimento>(entidade =>
        {
            entidade.HasKey(a => a.Id);
            entidade.Property(a => a.DataHora).IsRequired();
            entidade.Property(a => a.Valor).HasPrecision(18, 2);
            entidade.Property(a => a.Observacoes).HasMaxLength(1000);
            entidade.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);

            entidade.HasOne(a => a.Paciente)
                .WithMany(p => p.Atendimentos)
                .HasForeignKey(a => a.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entidade.HasOne(a => a.Medico)
                .WithMany(m => m.Atendimentos)
                .HasForeignKey(a => a.MedicoId)
                .OnDelete(DeleteBehavior.Restrict);

            entidade.HasOne(a => a.Servico)
                .WithMany(s => s.Atendimentos)
                .HasForeignKey(a => a.ServicoId)
                .OnDelete(DeleteBehavior.Restrict);

            // A agenda é quase sempre consultada por médico e data.
            entidade.HasIndex(a => new { a.MedicoId, a.DataHora });
            entidade.HasIndex(a => a.DataHora);

            entidade.Ignore(a => a.DataHoraFim);
            entidade.Ignore(a => a.AnexosAtivos);
            entidade.Ignore(a => a.EstaAberto);
            entidade.Ignore(a => a.EstaEncerrado);
            entidade.Ignore(a => a.EhHoje);
            entidade.Ignore(a => a.EhAmanha);
            entidade.Ignore(a => a.JaPassou);
        });

        // ------------------------------- Anexo -------------------------------
        modelBuilder.Entity<Anexo>(entidade =>
        {
            entidade.HasKey(x => x.Id);
            entidade.Property(x => x.NomeArquivo).IsRequired().HasMaxLength(255);
            entidade.Property(x => x.TipoConteudo).IsRequired().HasMaxLength(120);
            entidade.Property(x => x.Conteudo).IsRequired();
            entidade.Property(x => x.Descricao).HasMaxLength(4000);
            entidade.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(20);

            entidade.HasOne(x => x.Atendimento)
                .WithMany(a => a.Anexos)
                .HasForeignKey(x => x.AtendimentoId)
                .OnDelete(DeleteBehavior.Cascade);

            entidade.Ignore(x => x.TamanhoBytes);
            entidade.Ignore(x => x.TamanhoLegivel);
        });

        // ------------------------------ Usuario ------------------------------
        modelBuilder.Entity<Usuario>(entidade =>
        {
            entidade.HasKey(u => u.Id);
            entidade.Property(u => u.Nome).IsRequired().HasMaxLength(200);
            entidade.Property(u => u.Email).IsRequired().HasMaxLength(200);
            entidade.Property(u => u.SenhaHash).IsRequired().HasMaxLength(200);
            entidade.Property(u => u.SenhaSalt).IsRequired().HasMaxLength(200);
            entidade.Property(u => u.Perfil).HasConversion<string>().HasMaxLength(20);

            entidade.HasIndex(u => u.Email).IsUnique();

            entidade.HasOne(u => u.Medico)
                .WithMany()
                .HasForeignKey(u => u.MedicoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }
}
