using Clinica.Api.Data;
using Clinica.Api.Entidades;
using Clinica.Api.Enums;
using Clinica.Api.Excecoes;
using Clinica.Api.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Api.Servicos;

/// <summary>
/// CRUD de atendimentos e de anexos. Concentra a detecção de conflito de
/// agenda, que é a regra mais importante do sistema.
/// </summary>
public class AtendimentoService
{
    public static readonly TimeSpan AberturaClinica = new(7, 0, 0);
    public static readonly TimeSpan FechamentoClinica = new(20, 0, 0);

    private readonly ClinicaDbContext _context;

    public AtendimentoService(ClinicaDbContext context) => _context = context;

    private IQueryable<Atendimento> ConsultaCompleta()
        => _context.Atendimentos
            .Include(a => a.Paciente)
            .Include(a => a.Medico)
            .Include(a => a.Servico)
            .Where(a => !a.EstaDeletado);

    public async Task<List<Atendimento>> ListarAsync(
        DateTime? dia = null,
        int? medicoId = null,
        int? pacienteId = null,
        StatusAtendimento? status = null)
    {
        var consulta = ConsultaCompleta();

        if (dia is not null)
        {
            var inicio = dia.Value.Date;
            var fim = inicio.AddDays(1);
            consulta = consulta.Where(a => a.DataHora >= inicio && a.DataHora < fim);
        }

        if (medicoId is not null) consulta = consulta.Where(a => a.MedicoId == medicoId);
        if (pacienteId is not null) consulta = consulta.Where(a => a.PacienteId == pacienteId);
        if (status is not null) consulta = consulta.Where(a => a.Status == status);

        return await consulta
            .OrderBy(a => a.DataHora)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Atendimento> ObterPorIdAsync(int id)
        => await ConsultaCompleta()
               .Include(a => a.Anexos.Where(x => !x.EstaDeletado))
               .AsNoTracking()
               .FirstOrDefaultAsync(a => a.Id == id)
           ?? throw new RegraNegocioException($"Atendimento {id} não encontrado.");

    /// <summary>Versão rastreada pelo contexto, para alterações.</summary>
    private async Task<Atendimento> ObterParaAlterarAsync(int id)
        => await _context.Atendimentos
               .Include(a => a.Paciente)
               .Include(a => a.Medico)
               .Include(a => a.Servico)
               .FirstOrDefaultAsync(a => a.Id == id && !a.EstaDeletado)
           ?? throw new RegraNegocioException($"Atendimento {id} não encontrado.");

    /// <summary>
    /// Atendimentos de amanhã, em aberto e sem lembrete enviado. É a consulta
    /// que o background service roda.
    /// </summary>
    public async Task<List<Atendimento>> ListarParaLembreteAsync()
    {
        var amanha = DateTime.Today.AddDays(1);
        var depois = amanha.AddDays(1);

        return await _context.Atendimentos
            .Include(a => a.Paciente)
            .Include(a => a.Medico)
            .Include(a => a.Servico)
            .Where(a => !a.EstaDeletado
                        && a.LembreteEnviadoEm == null
                        && a.DataHora >= amanha
                        && a.DataHora < depois
                        && (a.Status == StatusAtendimento.Agendado
                            || a.Status == StatusAtendimento.Confirmado))
            .OrderBy(a => a.DataHora)
            .ToListAsync();
    }

    // ------------------------------- Agendamento -------------------------------

    public async Task<Atendimento> AgendarAsync(AgendamentoRequisicao requisicao)
    {
        var paciente = await _context.Pacientes
                           .FirstOrDefaultAsync(p =>
                               p.Id == requisicao.PacienteId && !p.EstaDeletado)
                       ?? throw new RegraNegocioException(
                           $"Paciente {requisicao.PacienteId} não encontrado.");

        var medico = await _context.Medicos
                         .FirstOrDefaultAsync(m =>
                             m.Id == requisicao.MedicoId && !m.EstaDeletado)
                     ?? throw new RegraNegocioException(
                         $"Médico {requisicao.MedicoId} não encontrado.");

        var servico = await _context.Servicos
                          .FirstOrDefaultAsync(s =>
                              s.Id == requisicao.ServicoId && !s.EstaDeletado)
                      ?? throw new RegraNegocioException(
                          $"Serviço {requisicao.ServicoId} não encontrado.");

        ValidarHorario(requisicao.DataHora, servico.DuracaoMinutos);

        var atendimento = new Atendimento
        {
            PacienteId = paciente.Id,
            Paciente = paciente,
            MedicoId = medico.Id,
            Medico = medico,
            ServicoId = servico.Id,
            Servico = servico,
            DataHora = requisicao.DataHora,
            // Duração e valor viram cópia: mudar o serviço depois não mexe
            // nas agendas já marcadas.
            DuracaoMinutos = servico.DuracaoMinutos,
            Valor = servico.Valor,
            Status = StatusAtendimento.Agendado,
            Observacoes = string.IsNullOrWhiteSpace(requisicao.Observacoes)
                ? null
                : requisicao.Observacoes.Trim(),
            DataCriacao = DateTime.Now
        };

        await GarantirQueNaoHaConflitoAsync(atendimento);

        _context.Atendimentos.Add(atendimento);
        await _context.SaveChangesAsync();

        return atendimento;
    }

    public async Task<Atendimento> RemarcarAsync(int id, DateTime novaDataHora)
    {
        var atendimento = await ObterParaAlterarAsync(id);

        if (atendimento.EstaEncerrado)
            throw new RegraNegocioException(
                $"Um atendimento {atendimento.Status} não pode ser remarcado.");

        ValidarHorario(novaDataHora, atendimento.DuracaoMinutos);

        var dataAnterior = atendimento.DataHora;
        atendimento.DataHora = novaDataHora;

        try
        {
            await GarantirQueNaoHaConflitoAsync(atendimento);
        }
        catch
        {
            atendimento.DataHora = dataAnterior;
            throw;
        }

        // Remarcar invalida a confirmação e o lembrete já enviados.
        atendimento.Status = StatusAtendimento.Agendado;
        atendimento.ConfirmacaoEnviadaEm = null;
        atendimento.LembreteEnviadoEm = null;

        await _context.SaveChangesAsync();
        return atendimento;
    }

    public async Task RemoverAsync(int id)
    {
        var atendimento = await _context.Atendimentos
                              .Include(a => a.Anexos)
                              .FirstOrDefaultAsync(a => a.Id == id && !a.EstaDeletado)
                          ?? throw new RegraNegocioException($"Atendimento {id} não encontrado.");

        if (atendimento.Status == StatusAtendimento.Realizado)
            throw new RegraNegocioException(
                "Um atendimento realizado não pode ser removido: faz parte do histórico clínico.");

        atendimento.EstaDeletado = true;

        foreach (var anexo in atendimento.Anexos)
            anexo.EstaDeletado = true;

        await _context.SaveChangesAsync();
    }

    // ----------------------------- Conflito de agenda -----------------------------

    /// <summary>
    /// Carrega os atendimentos do mesmo dia (consulta traduzível para o banco)
    /// e compara os intervalos em memória com Atendimento.ConflitaCom.
    /// </summary>
    private async Task GarantirQueNaoHaConflitoAsync(Atendimento novo)
    {
        var inicioDoDia = novo.DataHora.Date;
        var fimDoDia = inicioDoDia.AddDays(1);

        var doDia = await _context.Atendimentos
            .Include(a => a.Paciente)
            .Include(a => a.Medico)
            .Where(a => !a.EstaDeletado
                        && a.Id != novo.Id
                        && (a.Status == StatusAtendimento.Agendado
                            || a.Status == StatusAtendimento.Confirmado)
                        && a.DataHora >= inicioDoDia
                        && a.DataHora < fimDoDia
                        && (a.MedicoId == novo.MedicoId || a.PacienteId == novo.PacienteId))
            .AsNoTracking()
            .ToListAsync();

        var conflitoMedico = doDia.FirstOrDefault(a =>
            a.MedicoId == novo.MedicoId && a.ConflitaCom(novo));

        if (conflitoMedico is not null)
            throw new RegraNegocioException(
                $"Dr(a). {novo.Medico?.NomeCompleto} já tem atendimento das " +
                $"{conflitoMedico.DataHora:HH:mm} às {conflitoMedico.DataHoraFim:HH:mm} " +
                $"em {conflitoMedico.DataHora:dd/MM/yyyy} " +
                $"(paciente {conflitoMedico.Paciente?.NomeCompleto}).");

        var conflitoPaciente = doDia.FirstOrDefault(a =>
            a.PacienteId == novo.PacienteId && a.ConflitaCom(novo));

        if (conflitoPaciente is not null)
            throw new RegraNegocioException(
                $"{novo.Paciente?.NomeCompleto} já tem atendimento das " +
                $"{conflitoPaciente.DataHora:HH:mm} às {conflitoPaciente.DataHoraFim:HH:mm} " +
                $"em {conflitoPaciente.DataHora:dd/MM/yyyy} " +
                $"(Dr(a). {conflitoPaciente.Medico?.NomeCompleto}).");
    }

    private static void ValidarHorario(DateTime dataHora, int duracaoMinutos)
    {
        if (dataHora == default)
            throw new RegraNegocioException("A data e hora do atendimento são obrigatórias.");

        if (dataHora < DateTime.Now)
            throw new RegraNegocioException(
                $"Não é possível agendar para {dataHora:dd/MM/yyyy HH:mm}, que já passou.");

        if (dataHora > DateTime.Now.AddYears(2))
            throw new RegraNegocioException(
                "Não é possível agendar com mais de 2 anos de antecedência.");

        if (dataHora.DayOfWeek == DayOfWeek.Sunday)
            throw new RegraNegocioException("A clínica não atende aos domingos.");

        var inicio = dataHora.TimeOfDay;
        var fim = inicio.Add(TimeSpan.FromMinutes(duracaoMinutos));

        if (inicio < AberturaClinica)
            throw new RegraNegocioException(
                $"A clínica abre às {AberturaClinica:hh\\:mm}. Horário informado: {inicio:hh\\:mm}.");

        if (fim > FechamentoClinica)
            throw new RegraNegocioException(
                $"O atendimento terminaria às {fim:hh\\:mm} e a clínica fecha às " +
                $"{FechamentoClinica:hh\\:mm}.");
    }

    // ----------------------------- Mudanças de status -----------------------------

    public async Task<Atendimento> ConfirmarAsync(int id)
    {
        var atendimento = await ObterParaAlterarAsync(id);
        atendimento.Confirmar();

        await _context.SaveChangesAsync();
        return atendimento;
    }

    public async Task<Atendimento> MarcarComoRealizadoAsync(int id)
    {
        var atendimento = await ObterParaAlterarAsync(id);
        atendimento.MarcarComoRealizado();

        await _context.SaveChangesAsync();
        return atendimento;
    }

    public async Task<Atendimento> CancelarAsync(int id, string? motivo)
    {
        var atendimento = await ObterParaAlterarAsync(id);
        atendimento.Cancelar(motivo);

        await _context.SaveChangesAsync();
        return atendimento;
    }

    public async Task<Atendimento> MarcarComoFaltaAsync(int id)
    {
        var atendimento = await ObterParaAlterarAsync(id);
        atendimento.MarcarComoNaoCompareceu();

        await _context.SaveChangesAsync();
        return atendimento;
    }

    /// <summary>Carimba o envio do lembrete. Usado pelo background service.</summary>
    public async Task RegistrarLembreteEnviadoAsync(int id)
    {
        var atendimento = await ObterParaAlterarAsync(id);
        atendimento.RegistrarLembreteEnviado();

        await _context.SaveChangesAsync();
    }

    // ---------------------------------- Anexos ----------------------------------

    public async Task<List<Anexo>> ListarAnexosAsync(int atendimentoId, TipoAnexo? tipo = null)
    {
        _ = await ObterParaAlterarAsync(atendimentoId);

        var consulta = _context.Anexos
            .Where(x => x.AtendimentoId == atendimentoId && !x.EstaDeletado);

        if (tipo is not null) consulta = consulta.Where(x => x.Tipo == tipo);

        return await consulta
            .OrderByDescending(x => x.DataCriacao)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>Todos os anexos de um paciente, de todos os atendimentos dele.</summary>
    public async Task<List<Anexo>> ListarAnexosDoPacienteAsync(int pacienteId)
        => await _context.Anexos
            .Include(x => x.Atendimento)
            .Where(x => !x.EstaDeletado
                        && x.Atendimento != null
                        && x.Atendimento.PacienteId == pacienteId
                        && !x.Atendimento.EstaDeletado)
            .OrderByDescending(x => x.DataCriacao)
            .AsNoTracking()
            .ToListAsync();

    public async Task<Anexo> ObterAnexoAsync(int atendimentoId, int anexoId)
        => await _context.Anexos
               .AsNoTracking()
               .FirstOrDefaultAsync(x =>
                   x.Id == anexoId && x.AtendimentoId == atendimentoId && !x.EstaDeletado)
           ?? throw new RegraNegocioException(
               $"Anexo {anexoId} não encontrado no atendimento {atendimentoId}.");

    /// <summary>
    /// Adiciona um anexo. Só atendimentos realizados aceitam anexo, porque
    /// atestado, receita e evolução nascem na consulta.
    /// </summary>
    public async Task<Anexo> AdicionarAnexoAsync(
        int atendimentoId,
        TipoAnexo tipo,
        string nomeArquivo,
        byte[] conteudo,
        string? tipoConteudo = null,
        string? descricao = null)
    {
        var atendimento = await ObterParaAlterarAsync(atendimentoId);
        atendimento.GarantirQueAceitaAnexos();

        if (!Enum.IsDefined(tipo))
            throw new RegraNegocioException($"Tipo de anexo '{tipo}' inválido.");

        if (string.IsNullOrWhiteSpace(nomeArquivo))
            throw new RegraNegocioException("O nome do arquivo é obrigatório.");

        if (conteudo.Length == 0)
            throw new RegraNegocioException("O arquivo do anexo está vazio.");

        if (conteudo.Length > Anexo.TamanhoMaximoBytes)
            throw new RegraNegocioException(
                $"O anexo deve ter no máximo {Anexo.TamanhoMaximoBytes / 1024 / 1024}MB.");

        var anexo = new Anexo
        {
            AtendimentoId = atendimento.Id,
            Tipo = tipo,
            NomeArquivo = nomeArquivo.Trim(),
            TipoConteudo = string.IsNullOrWhiteSpace(tipoConteudo)
                ? DetectarTipoConteudo(nomeArquivo)
                : tipoConteudo,
            Conteudo = conteudo,
            Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim(),
            DataCriacao = DateTime.Now
        };

        _context.Anexos.Add(anexo);
        await _context.SaveChangesAsync();

        return anexo;
    }

    public async Task<Anexo> AdicionarAnexoDeTextoAsync(
        int atendimentoId, AnexoRequisicao requisicao)
    {
        if (string.IsNullOrWhiteSpace(requisicao.Conteudo))
            throw new RegraNegocioException("O conteúdo do documento é obrigatório.");

        return await AdicionarAnexoAsync(
            atendimentoId,
            requisicao.Tipo,
            requisicao.NomeArquivo,
            System.Text.Encoding.UTF8.GetBytes(requisicao.Conteudo),
            "text/plain; charset=utf-8",
            requisicao.Descricao ?? requisicao.Conteudo);
    }

    /// <summary>Remoção lógica: documento clínico não se apaga.</summary>
    public async Task RemoverAnexoAsync(int atendimentoId, int anexoId)
    {
        var anexo = await _context.Anexos
                        .FirstOrDefaultAsync(x =>
                            x.Id == anexoId
                            && x.AtendimentoId == atendimentoId
                            && !x.EstaDeletado)
                    ?? throw new RegraNegocioException(
                        $"Anexo {anexoId} não encontrado no atendimento {atendimentoId}.");

        anexo.EstaDeletado = true;
        await _context.SaveChangesAsync();
    }

    private static string DetectarTipoConteudo(string nomeArquivo)
        => Path.GetExtension(nomeArquivo).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".txt" => "text/plain; charset=utf-8",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
}
