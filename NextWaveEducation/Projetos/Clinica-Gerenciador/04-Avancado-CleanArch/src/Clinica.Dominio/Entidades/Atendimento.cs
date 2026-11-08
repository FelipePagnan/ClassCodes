using Clinica.Dominio.Enums;
using Clinica.Dominio.Eventos;
using Clinica.Dominio.Excecoes;

namespace Clinica.Dominio.Entidades;

/// <summary>
/// O agendamento de um serviço, para um paciente, com um médico, num horário.
/// É o centro do sistema: tudo mais existe para alimentá-lo.
/// </summary>
public class Atendimento : EntidadeBase
{
    /// <summary>Horário em que a clínica abre.</summary>
    public static readonly TimeSpan AberturaClinica = new(7, 0, 0);

    /// <summary>Horário em que a clínica fecha.</summary>
    public static readonly TimeSpan FechamentoClinica = new(20, 0, 0);

    private readonly List<Anexo> _anexos = new();

    private Atendimento() { }

    public Atendimento(
        Paciente paciente,
        Medico medico,
        Servico servico,
        DateTime dataHora,
        string? observacoes = null)
    {
        ArgumentNullException.ThrowIfNull(paciente);
        ArgumentNullException.ThrowIfNull(medico);
        ArgumentNullException.ThrowIfNull(servico);

        ValidarHorario(dataHora, servico.DuracaoMinutos);

        PacienteId = paciente.Id;
        Paciente = paciente;
        MedicoId = medico.Id;
        Medico = medico;
        ServicoId = servico.Id;
        Servico = servico;
        DataHora = dataHora;

        // Duração e valor são cópias do serviço no momento do agendamento.
        // Sem isso, mudar a duração do serviço mexeria em agendas já marcadas.
        DuracaoMinutos = servico.DuracaoMinutos;
        Valor = servico.Valor;

        Status = StatusAtendimento.Agendado;
        Observacoes = string.IsNullOrWhiteSpace(observacoes) ? null : observacoes.Trim();
        DataCriacao = DateTime.Now;

        RegistrarEvento(new AtendimentoAgendado(this));
    }

    public int PacienteId { get; private set; }
    public Paciente Paciente { get; private set; } = null!;

    public int MedicoId { get; private set; }
    public Medico Medico { get; private set; } = null!;

    public int ServicoId { get; private set; }
    public Servico Servico { get; private set; } = null!;

    public DateTime DataHora { get; private set; }
    public int DuracaoMinutos { get; private set; }
    public decimal Valor { get; private set; }
    public StatusAtendimento Status { get; private set; } = StatusAtendimento.Agendado;
    public string? Observacoes { get; private set; }

    /// <summary>Quando a confirmação foi enviada ao paciente.</summary>
    public DateTime? ConfirmacaoEnviadaEm { get; private set; }

    /// <summary>Quando o lembrete do dia anterior foi enviado.</summary>
    public DateTime? LembreteEnviadoEm { get; private set; }

    public IReadOnlyCollection<Anexo> Anexos => _anexos.AsReadOnly();

    public DateTime DataHoraFim => DataHora.AddMinutes(DuracaoMinutos);

    /// <summary>Ainda vai acontecer e não foi cancelado.</summary>
    public bool EstaAberto
        => Status is StatusAtendimento.Agendado or StatusAtendimento.Confirmado;

    /// <summary>Status finais: o atendimento não muda mais.</summary>
    public bool EstaEncerrado
        => Status is StatusAtendimento.Realizado
                  or StatusAtendimento.Cancelado
                  or StatusAtendimento.NaoCompareceu;

    public bool EhHoje => DataHora.Date == DateTime.Today;
    public bool EhAmanha => DataHora.Date == DateTime.Today.AddDays(1);
    public bool JaPassou => DataHora < DateTime.Now;

    /// <summary>
    /// Verifica se este atendimento se sobrepõe no tempo com outro.
    /// Dois intervalos colidem quando cada um começa antes do outro terminar.
    /// </summary>
    public bool ConflitaCom(Atendimento outro)
        => ConflitaComIntervalo(outro.DataHora, outro.DuracaoMinutos);

    /// <summary>
    /// Mesma comparação, mas contra um intervalo solto. Serve para testar um
    /// horário candidato antes de alterar o atendimento — sem isso, verificar
    /// conflito exigiria mudar a data primeiro e desfazer em caso de erro.
    /// </summary>
    public bool ConflitaComIntervalo(DateTime inicio, int duracaoMinutos)
        => DataHora < inicio.AddMinutes(duracaoMinutos) && inicio < DataHoraFim;

    // ----------------------------- Comportamento -----------------------------

    public void Remarcar(DateTime novaDataHora)
    {
        if (EstaEncerrado)
            throw new RegraNegocioException(
                $"Um atendimento {Status} não pode ser remarcado.");

        ValidarHorario(novaDataHora, DuracaoMinutos);

        var dataAnterior = DataHora;
        DataHora = novaDataHora;

        // O paciente foi avisado de um horário que já não vale: confirmação
        // e lembrete voltam à estaca zero.
        Status = StatusAtendimento.Agendado;
        ConfirmacaoEnviadaEm = null;
        LembreteEnviadoEm = null;

        RegistrarEvento(new AtendimentoRemarcado(this, dataAnterior));
    }

    public void AlterarObservacoes(string? observacoes)
        => Observacoes = string.IsNullOrWhiteSpace(observacoes) ? null : observacoes.Trim();

    public void Confirmar()
    {
        if (Status != StatusAtendimento.Agendado)
            throw new RegraNegocioException(
                $"Só um atendimento agendado pode ser confirmado. Status atual: {Status}.");

        Status = StatusAtendimento.Confirmado;
        ConfirmacaoEnviadaEm = DateTime.Now;

        RegistrarEvento(new AtendimentoConfirmado(this));
    }

    public void MarcarComoRealizado()
    {
        if (!EstaAberto)
            throw new RegraNegocioException(
                $"Um atendimento {Status} não pode ser marcado como realizado.");

        if (DataHora > DateTime.Now)
            throw new RegraNegocioException(
                $"O atendimento está marcado para {DataHora:dd/MM/yyyy HH:mm} " +
                "e ainda não aconteceu.");

        Status = StatusAtendimento.Realizado;
    }

    public void Cancelar(string? motivo = null)
    {
        if (Status == StatusAtendimento.Realizado)
            throw new RegraNegocioException("Um atendimento realizado não pode ser cancelado.");

        if (Status == StatusAtendimento.Cancelado)
            throw new RegraNegocioException("Este atendimento já está cancelado.");

        Status = StatusAtendimento.Cancelado;

        if (!string.IsNullOrWhiteSpace(motivo))
        {
            Observacoes = string.IsNullOrWhiteSpace(Observacoes)
                ? $"Cancelado: {motivo}"
                : $"{Observacoes} | Cancelado: {motivo}";
        }

        RegistrarEvento(new AtendimentoCancelado(this, motivo));
    }

    public void MarcarComoNaoCompareceu()
    {
        if (!EstaAberto)
            throw new RegraNegocioException(
                $"Um atendimento {Status} não pode ser marcado como falta.");

        if (DataHora > DateTime.Now)
            throw new RegraNegocioException(
                "Não é possível marcar falta num atendimento que ainda não aconteceu.");

        Status = StatusAtendimento.NaoCompareceu;

        RegistrarEvento(new PacienteNaoCompareceu(this));
    }

    public void RegistrarLembreteEnviado()
    {
        LembreteEnviadoEm = DateTime.Now;
        RegistrarEvento(new LembreteEnviado(this));
    }

    /// <summary>
    /// Anexa um documento clínico. Só atendimentos realizados aceitam anexo,
    /// porque atestado, receita e evolução nascem durante a consulta.
    /// </summary>
    public Anexo AnexarDocumento(
        TipoAnexo tipo,
        string nomeArquivo,
        string tipoConteudo,
        byte[] conteudo,
        string? descricao = null)
    {
        if (Status != StatusAtendimento.Realizado)
            throw new RegraNegocioException(
                "Anexos só podem ser adicionados a atendimentos realizados. " +
                $"Status atual: {Status}.");

        var anexo = new Anexo(this, tipo, nomeArquivo, tipoConteudo, conteudo, descricao);
        _anexos.Add(anexo);

        RegistrarEvento(new DocumentoAnexado(this, anexo));

        return anexo;
    }

    /// <summary>Remoção lógica do atendimento, junto com os anexos dele.</summary>
    public void DeletarComAnexos()
    {
        if (Status == StatusAtendimento.Realizado)
            throw new RegraNegocioException(
                "Um atendimento realizado não pode ser removido: faz parte do histórico clínico.");

        Deletar();

        foreach (var anexo in _anexos)
            anexo.Deletar();
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
                $"A clínica abre às {AberturaClinica:hh\\:mm}. " +
                $"Horário informado: {inicio:hh\\:mm}.");

        if (fim > FechamentoClinica)
            throw new RegraNegocioException(
                $"O atendimento terminaria às {fim:hh\\:mm} e a clínica fecha às " +
                $"{FechamentoClinica:hh\\:mm}.");
    }
}
