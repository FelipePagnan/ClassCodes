using Clinica.Api.Data;
using Clinica.Api.Entidades;
using Clinica.Api.Excecoes;
using Clinica.Api.Modelos;
using Clinica.Api.Util;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Api.Servicos;

/// <summary>CRUD de pacientes sobre o Entity Framework Core.</summary>
public class PacienteService
{
    private readonly ClinicaDbContext _context;

    public PacienteService(ClinicaDbContext context) => _context = context;

    public async Task<List<Paciente>> ListarAsync()
        => await _context.Pacientes
            .Where(p => !p.EstaDeletado)
            .OrderBy(p => p.NomeCompleto)
            .AsNoTracking()
            .ToListAsync();

    public async Task<Paciente?> BuscarPorIdAsync(int id)
        => await _context.Pacientes
            .FirstOrDefaultAsync(p => p.Id == id && !p.EstaDeletado);

    public async Task<Paciente> ObterPorIdAsync(int id)
        => await BuscarPorIdAsync(id)
           ?? throw new RegraNegocioException($"Paciente {id} não encontrado.");

    /// <summary>Ficha do paciente com o histórico de atendimentos e anexos.</summary>
    public async Task<Paciente> ObterFichaAsync(int id)
        => await _context.Pacientes
               .Include(p => p.Atendimentos.Where(a => !a.EstaDeletado))
                   .ThenInclude(a => a.Medico)
               .Include(p => p.Atendimentos.Where(a => !a.EstaDeletado))
                   .ThenInclude(a => a.Servico)
               .Include(p => p.Atendimentos.Where(a => !a.EstaDeletado))
                   .ThenInclude(a => a.Anexos.Where(x => !x.EstaDeletado))
               .AsNoTracking()
               .FirstOrDefaultAsync(p => p.Id == id && !p.EstaDeletado)
           ?? throw new RegraNegocioException($"Paciente {id} não encontrado.");

    /// <summary>
    /// Busca por CPF. A comparação é feita sobre os dígitos, então tanto faz
    /// receber "529.982.247-25" ou "52998224725".
    /// </summary>
    public async Task<Paciente?> BuscarPorCpfAsync(string cpf)
    {
        var digitos = Documentos.SomenteDigitos(cpf);
        if (digitos.Length == 0) return null;

        return await _context.Pacientes
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Cpf == digitos && !p.EstaDeletado);
    }

    /// <summary>
    /// Busca por celular. Devolve lista porque o mesmo número costuma ser
    /// usado por mais de um paciente da família.
    /// </summary>
    public async Task<List<Paciente>> BuscarPorCelularAsync(string celular)
    {
        var digitos = Documentos.SomenteDigitos(celular);
        if (digitos.Length == 0) return new List<Paciente>();

        return await _context.Pacientes
            .Where(p => p.Celular == digitos && !p.EstaDeletado)
            .OrderBy(p => p.DataNascimento)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>Busca livre por nome, CPF ou celular de uma vez.</summary>
    public async Task<List<Paciente>> PesquisarAsync(string? termo)
    {
        if (string.IsNullOrWhiteSpace(termo)) return await ListarAsync();

        var texto = termo.Trim();
        var digitos = Documentos.SomenteDigitos(texto);
        var temDigitos = digitos.Length >= 3;

        return await _context.Pacientes
            .Where(p => !p.EstaDeletado && (
                p.NomeCompleto.Contains(texto) ||
                (temDigitos && p.Cpf.Contains(digitos)) ||
                (temDigitos && p.Celular.Contains(digitos))))
            .OrderBy(p => p.NomeCompleto)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Paciente> CadastrarAsync(PacienteRequisicao requisicao)
    {
        Validar(requisicao);

        var cpf = Documentos.SomenteDigitos(requisicao.Cpf);

        // Regra de negócio: o CPF identifica o paciente, então não repete.
        if (await _context.Pacientes.AnyAsync(p => p.Cpf == cpf && !p.EstaDeletado))
            throw new RegraNegocioException(
                $"Já existe um paciente cadastrado com o CPF {Documentos.FormatarCpf(cpf)}.");

        var paciente = new Paciente
        {
            NomeCompleto = requisicao.NomeCompleto.Trim(),
            Cpf = cpf,
            Celular = Documentos.SomenteDigitos(requisicao.Celular),
            Email = Normalizar(requisicao.Email),
            DataNascimento = requisicao.DataNascimento.Date,
            Sexo = requisicao.Sexo,
            Observacoes = string.IsNullOrWhiteSpace(requisicao.Observacoes)
                ? null
                : requisicao.Observacoes.Trim(),
            DataCriacao = DateTime.Now
        };

        _context.Pacientes.Add(paciente);
        await _context.SaveChangesAsync();

        return paciente;
    }

    public async Task<Paciente> AtualizarAsync(int id, PacienteRequisicao requisicao)
    {
        var paciente = await ObterPorIdAsync(id);
        Validar(requisicao);

        var cpf = Documentos.SomenteDigitos(requisicao.Cpf);

        if (await _context.Pacientes.AnyAsync(p =>
                p.Cpf == cpf && p.Id != id && !p.EstaDeletado))
            throw new RegraNegocioException(
                $"Já existe outro paciente com o CPF {Documentos.FormatarCpf(cpf)}.");

        paciente.NomeCompleto = requisicao.NomeCompleto.Trim();
        paciente.Cpf = cpf;
        paciente.Celular = Documentos.SomenteDigitos(requisicao.Celular);
        paciente.Email = Normalizar(requisicao.Email);
        paciente.DataNascimento = requisicao.DataNascimento.Date;
        paciente.Sexo = requisicao.Sexo;
        paciente.Observacoes = string.IsNullOrWhiteSpace(requisicao.Observacoes)
            ? null
            : requisicao.Observacoes.Trim();

        await _context.SaveChangesAsync();
        return paciente;
    }

    /// <summary>Remoção lógica: o histórico clínico é preservado.</summary>
    public async Task RemoverAsync(int id)
    {
        var paciente = await _context.Pacientes
                           .Include(p => p.Atendimentos)
                           .FirstOrDefaultAsync(p => p.Id == id && !p.EstaDeletado)
                       ?? throw new RegraNegocioException($"Paciente {id} não encontrado.");

        var abertos = paciente.Atendimentos.Count(a => !a.EstaDeletado && a.EstaAberto);

        if (abertos > 0)
            throw new RegraNegocioException(
                $"'{paciente.NomeCompleto}' tem {abertos} atendimento(s) em aberto. " +
                "Cancele-os antes de remover o paciente.");

        paciente.EstaDeletado = true;
        await _context.SaveChangesAsync();
    }

    private static string? Normalizar(string? email)
        => string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();

    private static void Validar(PacienteRequisicao requisicao)
    {
        if (string.IsNullOrWhiteSpace(requisicao.NomeCompleto)
            || requisicao.NomeCompleto.Trim().Length < 3)
            throw new RegraNegocioException("O nome completo deve ter ao menos 3 caracteres.");

        if (requisicao.NomeCompleto.Trim().Length > 200)
            throw new RegraNegocioException("O nome completo deve ter no máximo 200 caracteres.");

        // Regra de negócio: CPF com dígitos verificadores válidos.
        if (!Documentos.CpfValido(requisicao.Cpf))
            throw new RegraNegocioException(
                $"O CPF '{requisicao.Cpf}' é inválido (confira os dígitos verificadores).");

        if (!Documentos.CelularValido(requisicao.Celular))
            throw new RegraNegocioException(
                $"O celular '{requisicao.Celular}' é inválido. " +
                "Informe DDD + 9 dígitos, ex: (11) 98765-4321.");

        if (!string.IsNullOrWhiteSpace(requisicao.Email) && !requisicao.Email.Contains('@'))
            throw new RegraNegocioException($"O e-mail '{requisicao.Email}' é inválido.");

        if (requisicao.DataNascimento == default)
            throw new RegraNegocioException("A data de nascimento é obrigatória.");

        if (requisicao.DataNascimento.Date > DateTime.Today)
            throw new RegraNegocioException("A data de nascimento não pode estar no futuro.");

        if (requisicao.DataNascimento.Year < 1900)
            throw new RegraNegocioException("A data de nascimento informada é inválida.");

        if (!Enum.IsDefined(requisicao.Sexo))
            throw new RegraNegocioException($"Sexo '{requisicao.Sexo}' inválido.");
    }
}
