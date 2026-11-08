using Clinica.Api.Dominio.Entidades;
using Clinica.Api.Dominio.Enums;
using Clinica.Api.Dominio.ObjetosDeValor;
using Clinica.Api.Infraestrutura.Contexto;
using Clinica.Api.Util;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Api.Infraestrutura.Repositorios;

/// <summary>
/// Base comum dos repositórios: guarda o contexto e salva as alterações.
/// Nenhuma consulta aqui filtra EstaDeletado — isso é o filtro global
/// configurado no mapeamento.
/// </summary>
public abstract class RepositorioBase : IRepositorio
{
    protected readonly ClinicaDbContext Context;

    protected RepositorioBase(ClinicaDbContext context) => Context = context;

    public Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
        => Context.SaveChangesAsync(cancellationToken);
}

// ================================= Paciente =================================

public class PacienteRepositorio : RepositorioBase, IPacienteRepositorio
{
    public PacienteRepositorio(ClinicaDbContext context) : base(context) { }

    public Task<List<Paciente>> ListarAsync(CancellationToken cancellationToken = default)
        => Context.Pacientes
            .OrderBy(p => p.NomeCompleto)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<Paciente?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        => Context.Pacientes.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Paciente?> ObterFichaAsync(int id, CancellationToken cancellationToken = default)
        => Context.Pacientes
            .Include(p => p.Atendimentos).ThenInclude(a => a.Medico)
            .Include(p => p.Atendimentos).ThenInclude(a => a.Servico)
            .Include(p => p.Atendimentos).ThenInclude(a => a.Anexos)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Paciente?> ObterPorCpfAsync(Cpf cpf, CancellationToken cancellationToken = default)
        => Context.Pacientes
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Cpf == cpf, cancellationToken);

    public Task<List<Paciente>> ListarPorCelularAsync(
        Celular celular, CancellationToken cancellationToken = default)
        => Context.Pacientes
            .Where(p => p.Celular == celular)
            .OrderBy(p => p.DataNascimento)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<List<Paciente>> PesquisarAsync(
        string? termo, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(termo)) return ListarAsync(cancellationToken);

        var texto = termo.Trim();
        var digitos = Documentos.SomenteDigitos(texto);
        var temDigitos = digitos.Length >= 3;

        // Cpf e Celular são objetos de valor convertidos para uma coluna, e o
        // EF não sabe traduzir p.Cpf.Valor.Contains(...). EF.Property lê a
        // coluna direto, então o LIKE acontece no banco e não em memória.
        return Context.Pacientes
            .Where(p =>
                p.NomeCompleto.Contains(texto)
                || (temDigitos && EF.Property<string>(p, nameof(Paciente.Cpf)).Contains(digitos))
                || (temDigitos && EF.Property<string>(p, nameof(Paciente.Celular)).Contains(digitos)))
            .OrderBy(p => p.NomeCompleto)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExisteCpfAsync(
        Cpf cpf, int? ignorarId = null, CancellationToken cancellationToken = default)
        => Context.Pacientes
            .AnyAsync(p => p.Cpf == cpf && (ignorarId == null || p.Id != ignorarId),
                cancellationToken);

    public Task<bool> PossuiAtendimentoAbertoAsync(
        int pacienteId, CancellationToken cancellationToken = default)
        => Context.Atendimentos
            .AnyAsync(a => a.PacienteId == pacienteId
                           && (a.Status == StatusAtendimento.Agendado
                               || a.Status == StatusAtendimento.Confirmado),
                cancellationToken);

    public void Adicionar(Paciente paciente) => Context.Pacientes.Add(paciente);
}

// ================================== Medico ==================================

public class MedicoRepositorio : RepositorioBase, IMedicoRepositorio
{
    public MedicoRepositorio(ClinicaDbContext context) : base(context) { }

    public Task<List<Medico>> ListarAsync(
        string? especialidade = null, CancellationToken cancellationToken = default)
    {
        var consulta = Context.Medicos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(especialidade))
        {
            var texto = especialidade.Trim();
            consulta = consulta.Where(m => m.Especialidade.Contains(texto));
        }

        return consulta
            .OrderBy(m => m.NomeCompleto)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<Medico?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        => Context.Medicos.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public Task<Medico?> ObterPorCrmAsync(Crm crm, CancellationToken cancellationToken = default)
        => Context.Medicos
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Crm!.Numero == crm.Numero && m.Crm.Uf == crm.Uf,
                cancellationToken);

    public Task<bool> ExisteCrmAsync(
        Crm crm, int? ignorarId = null, CancellationToken cancellationToken = default)
        => Context.Medicos
            .AnyAsync(m => m.Crm!.Numero == crm.Numero
                           && m.Crm.Uf == crm.Uf
                           && (ignorarId == null || m.Id != ignorarId),
                cancellationToken);

    public Task<bool> PossuiAtendimentoAbertoAsync(
        int medicoId, CancellationToken cancellationToken = default)
        => Context.Atendimentos
            .AnyAsync(a => a.MedicoId == medicoId
                           && (a.Status == StatusAtendimento.Agendado
                               || a.Status == StatusAtendimento.Confirmado),
                cancellationToken);

    public void Adicionar(Medico medico) => Context.Medicos.Add(medico);
}

// ================================== Servico ==================================

public class ServicoRepositorio : RepositorioBase, IServicoRepositorio
{
    public ServicoRepositorio(ClinicaDbContext context) : base(context) { }

    public Task<List<Servico>> ListarAsync(CancellationToken cancellationToken = default)
        => Context.Servicos
            .OrderBy(s => s.Nome)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<Servico?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        => Context.Servicos.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<bool> ExisteNomeAsync(
        string nome, int? ignorarId = null, CancellationToken cancellationToken = default)
    {
        var texto = nome.Trim();

        return Context.Servicos
            .AnyAsync(s => s.Nome == texto && (ignorarId == null || s.Id != ignorarId),
                cancellationToken);
    }

    public Task<bool> PossuiAtendimentoAbertoAsync(
        int servicoId, CancellationToken cancellationToken = default)
        => Context.Atendimentos
            .AnyAsync(a => a.ServicoId == servicoId
                           && (a.Status == StatusAtendimento.Agendado
                               || a.Status == StatusAtendimento.Confirmado),
                cancellationToken);

    public void Adicionar(Servico servico) => Context.Servicos.Add(servico);
}

// ================================ Atendimento ================================

public class AtendimentoRepositorio : RepositorioBase, IAtendimentoRepositorio
{
    public AtendimentoRepositorio(ClinicaDbContext context) : base(context) { }

    private IQueryable<Atendimento> ComRelacionamentos()
        => Context.Atendimentos
            .Include(a => a.Paciente)
            .Include(a => a.Medico)
            .Include(a => a.Servico);

    public Task<List<Atendimento>> ListarAsync(
        DateTime? dia = null,
        int? medicoId = null,
        int? pacienteId = null,
        StatusAtendimento? status = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = ComRelacionamentos();

        if (dia is not null)
        {
            var inicio = dia.Value.Date;
            var fim = inicio.AddDays(1);
            consulta = consulta.Where(a => a.DataHora >= inicio && a.DataHora < fim);
        }

        if (medicoId is not null) consulta = consulta.Where(a => a.MedicoId == medicoId);
        if (pacienteId is not null) consulta = consulta.Where(a => a.PacienteId == pacienteId);
        if (status is not null) consulta = consulta.Where(a => a.Status == status);

        return consulta
            .OrderBy(a => a.DataHora)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<Atendimento?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        => ComRelacionamentos().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<Atendimento?> ObterComAnexosAsync(
        int id, CancellationToken cancellationToken = default)
        => ComRelacionamentos()
            .Include(a => a.Anexos)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<List<Atendimento>> ListarDoDiaParaConflitoAsync(
        DateTime dia,
        int medicoId,
        int pacienteId,
        int? ignorarId = null,
        CancellationToken cancellationToken = default)
    {
        var inicio = dia.Date;
        var fim = inicio.AddDays(1);

        // O recorte por dia e por envolvido roda no banco; a comparação de
        // intervalos acontece em memória, com Atendimento.ConflitaCom.
        return Context.Atendimentos
            .Include(a => a.Paciente)
            .Include(a => a.Medico)
            .Where(a => a.DataHora >= inicio
                        && a.DataHora < fim
                        && (a.Status == StatusAtendimento.Agendado
                            || a.Status == StatusAtendimento.Confirmado)
                        && (a.MedicoId == medicoId || a.PacienteId == pacienteId)
                        && (ignorarId == null || a.Id != ignorarId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<List<Atendimento>> ListarParaLembreteAsync(
        CancellationToken cancellationToken = default)
    {
        var amanha = DateTime.Today.AddDays(1);
        var depois = amanha.AddDays(1);

        return ComRelacionamentos()
            .Where(a => a.LembreteEnviadoEm == null
                        && a.DataHora >= amanha
                        && a.DataHora < depois
                        && (a.Status == StatusAtendimento.Agendado
                            || a.Status == StatusAtendimento.Confirmado))
            .OrderBy(a => a.DataHora)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Anexo>> ListarAnexosDoPacienteAsync(
        int pacienteId, CancellationToken cancellationToken = default)
        => Context.Anexos
            .Include(x => x.Atendimento)
            .Where(x => x.Atendimento.PacienteId == pacienteId)
            .OrderByDescending(x => x.DataCriacao)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public void Adicionar(Atendimento atendimento) => Context.Atendimentos.Add(atendimento);
}

// ================================== Usuario ==================================

public class UsuarioRepositorio : RepositorioBase, IUsuarioRepositorio
{
    public UsuarioRepositorio(ClinicaDbContext context) : base(context) { }

    public Task<List<Usuario>> ListarAsync(CancellationToken cancellationToken = default)
        => Context.Usuarios
            .Include(u => u.Medico)
            .OrderBy(u => u.Nome)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<Usuario?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        => Context.Usuarios
            .Include(u => u.Medico)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<Usuario?> ObterPorEmailAsync(
        Email email, CancellationToken cancellationToken = default)
        => Context.Usuarios
            .Include(u => u.Medico)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<bool> ExisteEmailAsync(Email email, CancellationToken cancellationToken = default)
        => Context.Usuarios.AnyAsync(u => u.Email == email, cancellationToken);

    public Task<bool> ExisteUsuarioDoMedicoAsync(
        int medicoId, CancellationToken cancellationToken = default)
        => Context.Usuarios.AnyAsync(u => u.MedicoId == medicoId, cancellationToken);

    public Task<int> ContarAdministradoresAtivosAsync(
        CancellationToken cancellationToken = default)
        => Context.Usuarios.CountAsync(
            u => u.Perfil == PerfilUsuario.Administrador && u.Ativo, cancellationToken);

    public void Adicionar(Usuario usuario) => Context.Usuarios.Add(usuario);
}
