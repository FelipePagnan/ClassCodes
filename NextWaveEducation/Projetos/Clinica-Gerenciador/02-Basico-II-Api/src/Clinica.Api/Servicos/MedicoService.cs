using Clinica.Api.Data;
using Clinica.Api.Entidades;
using Clinica.Api.Excecoes;
using Clinica.Api.Modelos;
using Clinica.Api.Util;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Api.Servicos;

/// <summary>CRUD de médicos.</summary>
public class MedicoService
{
    private readonly ClinicaDbContext _context;

    public MedicoService(ClinicaDbContext context) => _context = context;

    public async Task<List<Medico>> ListarAsync(string? especialidade = null)
    {
        var consulta = _context.Medicos.Where(m => !m.EstaDeletado);

        if (!string.IsNullOrWhiteSpace(especialidade))
        {
            var texto = especialidade.Trim();
            consulta = consulta.Where(m => m.Especialidade.Contains(texto));
        }

        return await consulta
            .OrderBy(m => m.NomeCompleto)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Medico?> BuscarPorIdAsync(int id)
        => await _context.Medicos.FirstOrDefaultAsync(m => m.Id == id && !m.EstaDeletado);

    public async Task<Medico> ObterPorIdAsync(int id)
        => await BuscarPorIdAsync(id)
           ?? throw new RegraNegocioException($"Médico {id} não encontrado.");

    /// <summary>
    /// Busca pelo CRM. O número só é único dentro da UF, então os dois
    /// precisam ser informados.
    /// </summary>
    public async Task<Medico?> BuscarPorCrmAsync(string crm, string uf)
    {
        var digitos = Documentos.SomenteDigitos(crm);
        var ufNormalizada = (uf ?? string.Empty).Trim().ToUpperInvariant();

        return await _context.Medicos
            .AsNoTracking()
            .FirstOrDefaultAsync(m =>
                m.Crm == digitos && m.UfCrm == ufNormalizada && !m.EstaDeletado);
    }

    public async Task<Medico> CadastrarAsync(MedicoRequisicao requisicao)
    {
        Validar(requisicao);

        var crm = Documentos.SomenteDigitos(requisicao.Crm);
        var uf = requisicao.UfCrm.Trim().ToUpperInvariant();

        // Regra de negócio: o par CRM + UF identifica o médico.
        if (await _context.Medicos.AnyAsync(m =>
                m.Crm == crm && m.UfCrm == uf && !m.EstaDeletado))
            throw new RegraNegocioException($"Já existe um médico cadastrado com CRM/{uf} {crm}.");

        var medico = new Medico
        {
            NomeCompleto = requisicao.NomeCompleto.Trim(),
            Crm = crm,
            UfCrm = uf,
            Especialidade = requisicao.Especialidade.Trim(),
            Email = string.IsNullOrWhiteSpace(requisicao.Email)
                ? null
                : requisicao.Email.Trim().ToLowerInvariant(),
            Celular = string.IsNullOrWhiteSpace(requisicao.Celular)
                ? null
                : Documentos.SomenteDigitos(requisicao.Celular),
            DataCriacao = DateTime.Now
        };

        _context.Medicos.Add(medico);
        await _context.SaveChangesAsync();

        return medico;
    }

    public async Task<Medico> AtualizarAsync(int id, MedicoRequisicao requisicao)
    {
        var medico = await ObterPorIdAsync(id);
        Validar(requisicao);

        var crm = Documentos.SomenteDigitos(requisicao.Crm);
        var uf = requisicao.UfCrm.Trim().ToUpperInvariant();

        if (await _context.Medicos.AnyAsync(m =>
                m.Crm == crm && m.UfCrm == uf && m.Id != id && !m.EstaDeletado))
            throw new RegraNegocioException($"Já existe outro médico com CRM/{uf} {crm}.");

        medico.NomeCompleto = requisicao.NomeCompleto.Trim();
        medico.Crm = crm;
        medico.UfCrm = uf;
        medico.Especialidade = requisicao.Especialidade.Trim();
        medico.Email = string.IsNullOrWhiteSpace(requisicao.Email)
            ? null
            : requisicao.Email.Trim().ToLowerInvariant();
        medico.Celular = string.IsNullOrWhiteSpace(requisicao.Celular)
            ? null
            : Documentos.SomenteDigitos(requisicao.Celular);

        await _context.SaveChangesAsync();
        return medico;
    }

    public async Task RemoverAsync(int id)
    {
        var medico = await _context.Medicos
                         .Include(m => m.Atendimentos)
                         .FirstOrDefaultAsync(m => m.Id == id && !m.EstaDeletado)
                     ?? throw new RegraNegocioException($"Médico {id} não encontrado.");

        var abertos = medico.Atendimentos.Count(a => !a.EstaDeletado && a.EstaAberto);

        if (abertos > 0)
            throw new RegraNegocioException(
                $"Dr(a). {medico.NomeCompleto} tem {abertos} atendimento(s) em aberto. " +
                "Cancele ou remarque antes de remover o médico.");

        medico.EstaDeletado = true;
        await _context.SaveChangesAsync();
    }

    private static void Validar(MedicoRequisicao requisicao)
    {
        if (string.IsNullOrWhiteSpace(requisicao.NomeCompleto)
            || requisicao.NomeCompleto.Trim().Length < 3)
            throw new RegraNegocioException("O nome completo deve ter ao menos 3 caracteres.");

        if (!Documentos.CrmValido(requisicao.Crm))
            throw new RegraNegocioException(
                $"O CRM '{requisicao.Crm}' é inválido. Informe de 4 a 7 dígitos.");

        if (!Documentos.UfValida(requisicao.UfCrm))
            throw new RegraNegocioException(
                $"A UF '{requisicao.UfCrm}' é inválida. Use a sigla do estado, ex: SP.");

        if (string.IsNullOrWhiteSpace(requisicao.Especialidade))
            throw new RegraNegocioException("A especialidade é obrigatória.");

        if (!string.IsNullOrWhiteSpace(requisicao.Celular)
            && !Documentos.CelularValido(requisicao.Celular))
            throw new RegraNegocioException($"O celular '{requisicao.Celular}' é inválido.");

        if (!string.IsNullOrWhiteSpace(requisicao.Email) && !requisicao.Email.Contains('@'))
            throw new RegraNegocioException($"O e-mail '{requisicao.Email}' é inválido.");
    }
}
