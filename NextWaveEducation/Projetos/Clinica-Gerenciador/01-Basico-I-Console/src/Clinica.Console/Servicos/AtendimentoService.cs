using Clinica.Console.Entidades;
using Clinica.Console.Enums;
using Clinica.Console.Excecoes;

namespace Clinica.Console.Servicos;

/// <summary>
/// CRUD de atendimentos. Concentra a detecção de conflito de agenda, que é
/// a regra mais importante de uma clínica: dois pacientes marcados com o
/// mesmo médico no mesmo horário é um problema na recepção, não no código.
/// </summary>
public class AtendimentoService
{
    /// <summary>Horário em que a clínica abre.</summary>
    public static readonly TimeSpan AberturaClinica = new(7, 0, 0);

    /// <summary>Horário em que a clínica fecha.</summary>
    public static readonly TimeSpan FechamentoClinica = new(20, 0, 0);

    private readonly PacienteService _pacienteService;
    private readonly MedicoService _medicoService;
    private readonly ServicoService _servicoService;

    private readonly List<Atendimento> _atendimentos = new();
    private int _proximoId = 1;
    private int _proximoIdAnexo = 1;

    public AtendimentoService(
        PacienteService pacienteService,
        MedicoService medicoService,
        ServicoService servicoService)
    {
        _pacienteService = pacienteService;
        _medicoService = medicoService;
        _servicoService = servicoService;
    }

    public IReadOnlyList<Atendimento> ListarTodos()
        => _atendimentos.Where(a => !a.EstaDeletado)
            .OrderBy(a => a.DataHora)
            .ToList()
            .AsReadOnly();

    public Atendimento? BuscarPorId(int id)
        => _atendimentos.FirstOrDefault(a => a.Id == id && !a.EstaDeletado);

    public Atendimento ObterPorId(int id)
        => BuscarPorId(id) ?? throw new RegraNegocioException($"Atendimento {id} não encontrado.");

    public IEnumerable<Atendimento> ListarPorDia(DateTime dia)
        => ListarTodos().Where(a => a.DataHora.Date == dia.Date);

    public IEnumerable<Atendimento> ListarPorMedico(int medicoId)
        => ListarTodos().Where(a => a.MedicoId == medicoId);

    public IEnumerable<Atendimento> ListarPorPaciente(int pacienteId)
        => ListarTodos().Where(a => a.PacienteId == pacienteId)
            .OrderByDescending(a => a.DataHora);

    public IEnumerable<Atendimento> ListarPorStatus(StatusAtendimento status)
        => ListarTodos().Where(a => a.Status == status);

    /// <summary>Atendimentos de amanhã que ainda não receberam lembrete.</summary>
    public IEnumerable<Atendimento> ListarParaLembrete()
        => ListarTodos().Where(a =>
            a.EhAmanha && a.EstaAberto && a.LembreteEnviadoEm is null);

    // ------------------------------- Agendamento -------------------------------

    public Atendimento Agendar(
        int pacienteId,
        int medicoId,
        int servicoId,
        DateTime dataHora,
        string? observacoes = null)
    {
        var paciente = _pacienteService.ObterPorId(pacienteId);
        var medico = _medicoService.ObterPorId(medicoId);
        var servico = _servicoService.ObterPorId(servicoId);

        ValidarHorario(dataHora, servico.DuracaoMinutos);

        var atendimento = new Atendimento
        {
            Id = _proximoId++,
            PacienteId = paciente.Id,
            Paciente = paciente,
            MedicoId = medico.Id,
            Medico = medico,
            ServicoId = servico.Id,
            Servico = servico,
            DataHora = dataHora,
            // Duração e valor são copiados do serviço: mudar o serviço depois
            // não deve mexer em agendas já marcadas.
            DuracaoMinutos = servico.DuracaoMinutos,
            Valor = servico.Valor,
            Status = StatusAtendimento.Agendado,
            Observacoes = string.IsNullOrWhiteSpace(observacoes) ? null : observacoes.Trim(),
            DataCriacao = DateTime.Now
        };

        GarantirQueNaoHaConflito(atendimento);

        _atendimentos.Add(atendimento);
        paciente.Atendimentos.Add(atendimento);
        medico.Atendimentos.Add(atendimento);
        servico.Atendimentos.Add(atendimento);

        return atendimento;
    }

    /// <summary>Remarca o atendimento para outro horário.</summary>
    public Atendimento Remarcar(int id, DateTime novaDataHora)
    {
        var atendimento = ObterPorId(id);

        if (atendimento.EstaEncerrado)
            throw new RegraNegocioException(
                $"Um atendimento {atendimento.Status} não pode ser remarcado.");

        ValidarHorario(novaDataHora, atendimento.DuracaoMinutos);

        var dataAnterior = atendimento.DataHora;
        atendimento.DataHora = novaDataHora;

        try
        {
            GarantirQueNaoHaConflito(atendimento);
        }
        catch
        {
            // Desfaz a troca antes de propagar o erro.
            atendimento.DataHora = dataAnterior;
            throw;
        }

        // Remarcar invalida a confirmação e o lembrete já enviados.
        atendimento.Status = StatusAtendimento.Agendado;
        atendimento.ConfirmacaoEnviadaEm = null;
        atendimento.LembreteEnviadoEm = null;

        return atendimento;
    }

    public Atendimento AtualizarObservacoes(int id, string? observacoes)
    {
        var atendimento = ObterPorId(id);
        atendimento.Observacoes = string.IsNullOrWhiteSpace(observacoes) ? null : observacoes.Trim();
        return atendimento;
    }

    /// <summary>Remoção lógica do atendimento, junto com os anexos dele.</summary>
    public void Remover(int id)
    {
        var atendimento = ObterPorId(id);

        if (atendimento.Status == StatusAtendimento.Realizado)
            throw new RegraNegocioException(
                "Um atendimento realizado não pode ser removido: faz parte do histórico clínico.");

        atendimento.EstaDeletado = true;

        foreach (var anexo in atendimento.Anexos)
            anexo.EstaDeletado = true;
    }

    // ------------------------------ Conflito de agenda ------------------------------

    /// <summary>
    /// Verifica se o médico ou o paciente já têm compromisso no intervalo.
    /// </summary>
    private void GarantirQueNaoHaConflito(Atendimento novo)
    {
        var abertos = ListarTodos().Where(a => a.Id != novo.Id && a.EstaAberto);

        var conflitoMedico = abertos.FirstOrDefault(a =>
            a.MedicoId == novo.MedicoId && a.ConflitaCom(novo));

        if (conflitoMedico is not null)
            throw new RegraNegocioException(
                $"Dr(a). {novo.Medico?.NomeCompleto} já tem atendimento das " +
                $"{conflitoMedico.DataHora:HH:mm} às {conflitoMedico.DataHoraFim:HH:mm} " +
                $"em {conflitoMedico.DataHora:dd/MM/yyyy} " +
                $"(paciente {conflitoMedico.Paciente?.NomeCompleto}).");

        var conflitoPaciente = abertos.FirstOrDefault(a =>
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
            throw new RegraNegocioException("Não é possível agendar com mais de 2 anos de antecedência.");

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

    // ------------------------------ Mudanças de status ------------------------------

    public Atendimento Confirmar(int id)
    {
        var atendimento = ObterPorId(id);
        atendimento.Confirmar();
        return atendimento;
    }

    public Atendimento MarcarComoRealizado(int id)
    {
        var atendimento = ObterPorId(id);
        atendimento.MarcarComoRealizado();
        return atendimento;
    }

    public Atendimento Cancelar(int id, string? motivo = null)
    {
        var atendimento = ObterPorId(id);
        atendimento.Cancelar(motivo);
        return atendimento;
    }

    public Atendimento MarcarComoFalta(int id)
    {
        var atendimento = ObterPorId(id);
        atendimento.MarcarComoNaoCompareceu();
        return atendimento;
    }

    // --------------------------------- Anexos ---------------------------------

    /// <summary>
    /// Adiciona um anexo ao atendimento. Só atendimentos realizados aceitam
    /// anexo, porque atestado, receita e evolução nascem na consulta.
    /// </summary>
    public Anexo AdicionarAnexo(
        int atendimentoId,
        TipoAnexo tipo,
        string nomeArquivo,
        byte[] conteudo,
        string? descricao = null)
    {
        var atendimento = ObterPorId(atendimentoId);
        atendimento.GarantirQueAceitaAnexos();

        if (!Enum.IsDefined(tipo))
            throw new RegraNegocioException($"Tipo de anexo '{tipo}' inválido.");

        if (string.IsNullOrWhiteSpace(nomeArquivo))
            throw new RegraNegocioException("O nome do arquivo é obrigatório.");

        if (conteudo is null || conteudo.Length == 0)
            throw new RegraNegocioException("O arquivo do anexo está vazio.");

        if (conteudo.Length > Anexo.TamanhoMaximoBytes)
            throw new RegraNegocioException(
                $"O anexo deve ter no máximo {Anexo.TamanhoMaximoBytes / 1024 / 1024}MB.");

        var anexo = new Anexo
        {
            Id = _proximoIdAnexo++,
            AtendimentoId = atendimento.Id,
            Atendimento = atendimento,
            Tipo = tipo,
            NomeArquivo = nomeArquivo.Trim(),
            TipoConteudo = DetectarTipoConteudo(nomeArquivo),
            Conteudo = conteudo,
            Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim(),
            DataCriacao = DateTime.Now
        };

        atendimento.Anexos.Add(anexo);
        return anexo;
    }

    /// <summary>Adiciona um anexo de texto (atestado ou receita digitados).</summary>
    public Anexo AdicionarAnexoDeTexto(
        int atendimentoId, TipoAnexo tipo, string nomeArquivo, string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            throw new RegraNegocioException("O conteúdo do documento é obrigatório.");

        var bytes = System.Text.Encoding.UTF8.GetBytes(texto);
        var anexo = AdicionarAnexo(atendimentoId, tipo, nomeArquivo, bytes, texto);
        anexo.TipoConteudo = "text/plain; charset=utf-8";

        return anexo;
    }

    public IEnumerable<Anexo> ListarAnexos(int atendimentoId)
        => ObterPorId(atendimentoId).AnexosAtivos.OrderByDescending(a => a.DataCriacao);

    public IEnumerable<Anexo> ListarAnexosPorTipo(int atendimentoId, TipoAnexo tipo)
        => ListarAnexos(atendimentoId).Where(a => a.Tipo == tipo);

    /// <summary>Todos os anexos de um paciente, de todos os atendimentos dele.</summary>
    public IEnumerable<Anexo> ListarAnexosDoPaciente(int pacienteId)
        => ListarPorPaciente(pacienteId)
            .SelectMany(a => a.AnexosAtivos)
            .OrderByDescending(a => a.DataCriacao);

    public Anexo ObterAnexo(int atendimentoId, int anexoId)
        => ListarAnexos(atendimentoId).FirstOrDefault(a => a.Id == anexoId)
           ?? throw new RegraNegocioException(
               $"Anexo {anexoId} não encontrado no atendimento {atendimentoId}.");

    /// <summary>Remoção lógica do anexo: documento clínico não se apaga.</summary>
    public void RemoverAnexo(int atendimentoId, int anexoId)
    {
        var anexo = ObterAnexo(atendimentoId, anexoId);
        anexo.EstaDeletado = true;
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

    // -------------------------------- Relatórios --------------------------------

    public AgendaDoDia GerarAgendaDoDia(DateTime dia, int? medicoId = null)
    {
        var atendimentos = ListarPorDia(dia).Where(a => a.EstaAberto || a.EstaEncerrado);

        if (medicoId is not null)
            atendimentos = atendimentos.Where(a => a.MedicoId == medicoId);

        var lista = atendimentos.OrderBy(a => a.DataHora).ToList();

        return new AgendaDoDia(
            dia.Date,
            lista.Count,
            lista.Count(a => a.Status == StatusAtendimento.Confirmado),
            lista.Count(a => a.Status == StatusAtendimento.Agendado),
            lista.Count(a => a.Status == StatusAtendimento.Realizado),
            lista.Count(a => a.Status == StatusAtendimento.Cancelado),
            lista.Count(a => a.Status == StatusAtendimento.NaoCompareceu),
            lista.Where(a => a.EstaAberto || a.Status == StatusAtendimento.Realizado)
                 .Sum(a => a.Valor),
            lista);
    }
}

public record AgendaDoDia(
    DateTime Dia,
    int Total,
    int Confirmados,
    int Agendados,
    int Realizados,
    int Cancelados,
    int Faltas,
    decimal ValorPrevisto,
    List<Atendimento> Atendimentos);
