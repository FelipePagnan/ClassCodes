using Clinica.Aplicacao.Dtos;
using Clinica.Dominio.Entidades;
using Clinica.Dominio.Enums;
using Clinica.Dominio.Excecoes;
using Clinica.Dominio.Repositorios;
using FluentValidation;
using MediatR;

namespace Clinica.Aplicacao.Atendimentos;

// ================================= Comandos =================================

public record AgendarAtendimento(
    int PacienteId,
    int MedicoId,
    int ServicoId,
    DateTime DataHora,
    string? Observacoes) : IRequest<AtendimentoDto>;

public record RemarcarAtendimento(int Id, DateTime NovaDataHora) : IRequest<AtendimentoDto>;

public record ConfirmarAtendimento(int Id) : IRequest<AtendimentoDto>;

public record MarcarAtendimentoComoRealizado(int Id) : IRequest<AtendimentoDto>;

public record CancelarAtendimento(int Id, string? Motivo) : IRequest<AtendimentoDto>;

public record MarcarFalta(int Id) : IRequest<AtendimentoDto>;

public record AlterarObservacoesDoAtendimento(int Id, string? Observacoes)
    : IRequest<AtendimentoDto>;

public record RemoverAtendimento(int Id) : IRequest<Unit>;

// ================================= Consultas =================================

public record ListarAtendimentos(
    DateTime? Dia,
    int? MedicoId,
    int? PacienteId,
    StatusAtendimento? Status) : IRequest<List<AtendimentoResumoDto>>;

public record ObterAtendimento(int Id) : IRequest<AtendimentoDto>;

public record ObterAgendaDoDia(DateTime Dia, int? MedicoId) : IRequest<AgendaDoDiaDto>;

// ================================ Validadores ================================

public class AgendarAtendimentoValidator : AbstractValidator<AgendarAtendimento>
{
    public AgendarAtendimentoValidator()
    {
        RuleFor(c => c.PacienteId).GreaterThan(0).WithMessage("Informe o paciente.");
        RuleFor(c => c.MedicoId).GreaterThan(0).WithMessage("Informe o médico.");
        RuleFor(c => c.ServicoId).GreaterThan(0).WithMessage("Informe o serviço.");

        RuleFor(c => c.DataHora)
            .NotEmpty().WithMessage("A data e hora são obrigatórias.")
            .GreaterThan(_ => DateTime.Now)
            .WithMessage("Não é possível agendar para um horário que já passou.")
            .Must(data => data.DayOfWeek != DayOfWeek.Sunday)
            .WithMessage("A clínica não atende aos domingos.")
            .Must(data => data.TimeOfDay >= Atendimento.AberturaClinica
                          && data.TimeOfDay < Atendimento.FechamentoClinica)
            .WithMessage($"A clínica atende das {Atendimento.AberturaClinica:hh\\:mm} "
                         + $"às {Atendimento.FechamentoClinica:hh\\:mm}.");

        RuleFor(c => c.Observacoes)
            .MaximumLength(1000).WithMessage("As observações devem ter no máximo 1000 caracteres.");
    }
}

public class RemarcarAtendimentoValidator : AbstractValidator<RemarcarAtendimento>
{
    public RemarcarAtendimentoValidator()
    {
        RuleFor(c => c.Id).GreaterThan(0).WithMessage("Informe o atendimento.");

        RuleFor(c => c.NovaDataHora)
            .NotEmpty().WithMessage("A nova data e hora são obrigatórias.")
            .GreaterThan(_ => DateTime.Now)
            .WithMessage("Não é possível remarcar para um horário que já passou.")
            .Must(data => data.DayOfWeek != DayOfWeek.Sunday)
            .WithMessage("A clínica não atende aos domingos.");
    }
}

// ============================= Serviço de domínio =============================

/// <summary>
/// A verificação de conflito de agenda, isolada porque dois handlers precisam
/// dela (agendar e remarcar) e porque é a regra mais importante do sistema —
/// vale ter um lugar único e testável.
///
/// A comparação de intervalos em si está na entidade
/// (Atendimento.ConflitaComIntervalo); aqui fica só o que depende do banco.
/// </summary>
public class VerificadorDeConflitoDeAgenda
{
    private readonly IAtendimentoRepositorio _repositorio;

    public VerificadorDeConflitoDeAgenda(IAtendimentoRepositorio repositorio)
        => _repositorio = repositorio;

    public async Task GarantirQueEstaLivreAsync(
        DateTime dataHora,
        int duracaoMinutos,
        Medico medico,
        Paciente paciente,
        int? ignorarId = null,
        CancellationToken cancellationToken = default)
    {
        var doDia = await _repositorio.ListarDoDiaParaConflitoAsync(
            dataHora, medico.Id, paciente.Id, ignorarId, cancellationToken);

        var conflitoMedico = doDia.FirstOrDefault(a =>
            a.MedicoId == medico.Id && a.ConflitaComIntervalo(dataHora, duracaoMinutos));

        if (conflitoMedico is not null)
            throw new RegraNegocioException(
                $"Dr(a). {medico.NomeCompleto} já tem atendimento das "
                + $"{conflitoMedico.DataHora:HH:mm} às {conflitoMedico.DataHoraFim:HH:mm} "
                + $"em {conflitoMedico.DataHora:dd/MM/yyyy} "
                + $"(paciente {conflitoMedico.Paciente.NomeCompleto}).");

        var conflitoPaciente = doDia.FirstOrDefault(a =>
            a.PacienteId == paciente.Id && a.ConflitaComIntervalo(dataHora, duracaoMinutos));

        if (conflitoPaciente is not null)
            throw new RegraNegocioException(
                $"{paciente.NomeCompleto} já tem atendimento das "
                + $"{conflitoPaciente.DataHora:HH:mm} às {conflitoPaciente.DataHoraFim:HH:mm} "
                + $"em {conflitoPaciente.DataHora:dd/MM/yyyy} "
                + $"(Dr(a). {conflitoPaciente.Medico.NomeCompleto}).");
    }
}

// ================================= Handlers =================================

public class AgendarAtendimentoHandler : IRequestHandler<AgendarAtendimento, AtendimentoDto>
{
    private readonly IAtendimentoRepositorio _repositorio;
    private readonly IPacienteRepositorio _pacienteRepositorio;
    private readonly IMedicoRepositorio _medicoRepositorio;
    private readonly IServicoRepositorio _servicoRepositorio;
    private readonly VerificadorDeConflitoDeAgenda _verificador;
    private readonly IUnitOfWork _unitOfWork;

    public AgendarAtendimentoHandler(
        IAtendimentoRepositorio repositorio,
        IPacienteRepositorio pacienteRepositorio,
        IMedicoRepositorio medicoRepositorio,
        IServicoRepositorio servicoRepositorio,
        VerificadorDeConflitoDeAgenda verificador,
        IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _pacienteRepositorio = pacienteRepositorio;
        _medicoRepositorio = medicoRepositorio;
        _servicoRepositorio = servicoRepositorio;
        _verificador = verificador;
        _unitOfWork = unitOfWork;
    }

    public async Task<AtendimentoDto> Handle(
        AgendarAtendimento comando, CancellationToken cancellationToken)
    {
        // Verificar conflito e gravar em transação é o que fecha a janela de
        // corrida: sem isso, duas recepcionistas marcando o mesmo horário ao
        // mesmo tempo passariam as duas pela verificação.
        return await _unitOfWork.ExecutarEmTransacaoAsync(async ct =>
        {
            var paciente = await _pacienteRepositorio.ObterPorIdAsync(comando.PacienteId, ct)
                           ?? throw new RegraNegocioException(
                               $"Paciente {comando.PacienteId} não encontrado.");

            var medico = await _medicoRepositorio.ObterPorIdAsync(comando.MedicoId, ct)
                         ?? throw new RegraNegocioException(
                             $"Médico {comando.MedicoId} não encontrado.");

            var servico = await _servicoRepositorio.ObterPorIdAsync(comando.ServicoId, ct)
                          ?? throw new RegraNegocioException(
                              $"Serviço {comando.ServicoId} não encontrado.");

            // O construtor valida o horário, copia duração e valor do serviço
            // e registra o evento AtendimentoAgendado.
            var atendimento = new Atendimento(
                paciente, medico, servico, comando.DataHora, comando.Observacoes);

            await _verificador.GarantirQueEstaLivreAsync(
                atendimento.DataHora, atendimento.DuracaoMinutos,
                medico, paciente, null, ct);

            _repositorio.Adicionar(atendimento);
            await _unitOfWork.CommitAsync(ct);

            return AtendimentoDto.De(atendimento);
        }, cancellationToken);
    }
}

public class RemarcarAtendimentoHandler : IRequestHandler<RemarcarAtendimento, AtendimentoDto>
{
    private readonly IAtendimentoRepositorio _repositorio;
    private readonly VerificadorDeConflitoDeAgenda _verificador;
    private readonly IUnitOfWork _unitOfWork;

    public RemarcarAtendimentoHandler(
        IAtendimentoRepositorio repositorio,
        VerificadorDeConflitoDeAgenda verificador,
        IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _verificador = verificador;
        _unitOfWork = unitOfWork;
    }

    public async Task<AtendimentoDto> Handle(
        RemarcarAtendimento comando, CancellationToken cancellationToken)
        => await _unitOfWork.ExecutarEmTransacaoAsync(async ct =>
        {
            var atendimento = await _repositorio.ObterComAnexosAsync(comando.Id, ct)
                              ?? throw new RegraNegocioException(
                                  $"Atendimento {comando.Id} não encontrado.");

            // A verificação vem antes de alterar o atendimento: ele está
            // rastreado pelo contexto, e desfazer a mudança depois exigiria
            // revalidar um horário que pode já ter passado.
            await _verificador.GarantirQueEstaLivreAsync(
                comando.NovaDataHora, atendimento.DuracaoMinutos,
                atendimento.Medico, atendimento.Paciente, comando.Id, ct);

            atendimento.Remarcar(comando.NovaDataHora);
            await _unitOfWork.CommitAsync(ct);

            return AtendimentoDto.De(atendimento);
        }, cancellationToken);
}

/// <summary>
/// Base dos handlers que só mudam o status: evitam repetir buscar, chamar o
/// método do domínio, commitar e mapear.
/// </summary>
public abstract class MudancaDeStatusHandler<TComando> : IRequestHandler<TComando, AtendimentoDto>
    where TComando : IRequest<AtendimentoDto>
{
    protected readonly IAtendimentoRepositorio Repositorio;
    protected readonly IUnitOfWork UnitOfWork;

    protected MudancaDeStatusHandler(IAtendimentoRepositorio repositorio, IUnitOfWork unitOfWork)
    {
        Repositorio = repositorio;
        UnitOfWork = unitOfWork;
    }

    protected abstract int ObterId(TComando comando);

    protected abstract void Aplicar(Atendimento atendimento, TComando comando);

    public async Task<AtendimentoDto> Handle(
        TComando comando, CancellationToken cancellationToken)
    {
        var id = ObterId(comando);

        var atendimento = await Repositorio.ObterComAnexosAsync(id, cancellationToken)
                          ?? throw new RegraNegocioException(
                              $"Atendimento {id} não encontrado.");

        // A regra de qual transição é permitida está na entidade.
        Aplicar(atendimento, comando);

        await UnitOfWork.CommitAsync(cancellationToken);

        return AtendimentoDto.De(atendimento);
    }
}

public class ConfirmarAtendimentoHandler : MudancaDeStatusHandler<ConfirmarAtendimento>
{
    public ConfirmarAtendimentoHandler(
        IAtendimentoRepositorio repositorio, IUnitOfWork unitOfWork)
        : base(repositorio, unitOfWork) { }

    protected override int ObterId(ConfirmarAtendimento comando) => comando.Id;

    protected override void Aplicar(Atendimento atendimento, ConfirmarAtendimento comando)
        => atendimento.Confirmar();
}

public class MarcarAtendimentoComoRealizadoHandler
    : MudancaDeStatusHandler<MarcarAtendimentoComoRealizado>
{
    public MarcarAtendimentoComoRealizadoHandler(
        IAtendimentoRepositorio repositorio, IUnitOfWork unitOfWork)
        : base(repositorio, unitOfWork) { }

    protected override int ObterId(MarcarAtendimentoComoRealizado comando) => comando.Id;

    protected override void Aplicar(
        Atendimento atendimento, MarcarAtendimentoComoRealizado comando)
        => atendimento.MarcarComoRealizado();
}

public class CancelarAtendimentoHandler : MudancaDeStatusHandler<CancelarAtendimento>
{
    public CancelarAtendimentoHandler(
        IAtendimentoRepositorio repositorio, IUnitOfWork unitOfWork)
        : base(repositorio, unitOfWork) { }

    protected override int ObterId(CancelarAtendimento comando) => comando.Id;

    protected override void Aplicar(Atendimento atendimento, CancelarAtendimento comando)
        => atendimento.Cancelar(comando.Motivo);
}

public class MarcarFaltaHandler : MudancaDeStatusHandler<MarcarFalta>
{
    public MarcarFaltaHandler(IAtendimentoRepositorio repositorio, IUnitOfWork unitOfWork)
        : base(repositorio, unitOfWork) { }

    protected override int ObterId(MarcarFalta comando) => comando.Id;

    protected override void Aplicar(Atendimento atendimento, MarcarFalta comando)
        => atendimento.MarcarComoNaoCompareceu();
}

public class AlterarObservacoesDoAtendimentoHandler
    : MudancaDeStatusHandler<AlterarObservacoesDoAtendimento>
{
    public AlterarObservacoesDoAtendimentoHandler(
        IAtendimentoRepositorio repositorio, IUnitOfWork unitOfWork)
        : base(repositorio, unitOfWork) { }

    protected override int ObterId(AlterarObservacoesDoAtendimento comando) => comando.Id;

    protected override void Aplicar(
        Atendimento atendimento, AlterarObservacoesDoAtendimento comando)
        => atendimento.AlterarObservacoes(comando.Observacoes);
}

public class RemoverAtendimentoHandler : IRequestHandler<RemoverAtendimento, Unit>
{
    private readonly IAtendimentoRepositorio _repositorio;
    private readonly IUnitOfWork _unitOfWork;

    public RemoverAtendimentoHandler(IAtendimentoRepositorio repositorio, IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RemoverAtendimento comando, CancellationToken cancellationToken)
    {
        var atendimento = await _repositorio.ObterComAnexosAsync(comando.Id, cancellationToken)
                          ?? throw new RegraNegocioException(
                              $"Atendimento {comando.Id} não encontrado.");

        // Remoção lógica, e os anexos vão junto.
        atendimento.DeletarComAnexos();
        await _unitOfWork.CommitAsync(cancellationToken);

        return Unit.Value;
    }
}

// ============================= Handlers de consulta =============================

public class ListarAtendimentosHandler
    : IRequestHandler<ListarAtendimentos, List<AtendimentoResumoDto>>
{
    private readonly IAtendimentoRepositorio _repositorio;

    public ListarAtendimentosHandler(IAtendimentoRepositorio repositorio)
        => _repositorio = repositorio;

    public async Task<List<AtendimentoResumoDto>> Handle(
        ListarAtendimentos consulta, CancellationToken cancellationToken)
    {
        var atendimentos = await _repositorio.ListarAsync(
            consulta.Dia, consulta.MedicoId, consulta.PacienteId,
            consulta.Status, cancellationToken);

        return atendimentos.Select(AtendimentoResumoDto.De).ToList();
    }
}

public class ObterAtendimentoHandler : IRequestHandler<ObterAtendimento, AtendimentoDto>
{
    private readonly IAtendimentoRepositorio _repositorio;

    public ObterAtendimentoHandler(IAtendimentoRepositorio repositorio)
        => _repositorio = repositorio;

    public async Task<AtendimentoDto> Handle(
        ObterAtendimento consulta, CancellationToken cancellationToken)
        => AtendimentoDto.De(
            await _repositorio.ObterComAnexosAsync(consulta.Id, cancellationToken)
            ?? throw new RegraNegocioException($"Atendimento {consulta.Id} não encontrado."));
}

public class ObterAgendaDoDiaHandler : IRequestHandler<ObterAgendaDoDia, AgendaDoDiaDto>
{
    private readonly IAtendimentoRepositorio _repositorio;

    public ObterAgendaDoDiaHandler(IAtendimentoRepositorio repositorio)
        => _repositorio = repositorio;

    public async Task<AgendaDoDiaDto> Handle(
        ObterAgendaDoDia consulta, CancellationToken cancellationToken)
    {
        var atendimentos = await _repositorio.ListarAsync(
            consulta.Dia, consulta.MedicoId, null, null, cancellationToken);

        return new AgendaDoDiaDto(
            consulta.Dia.Date,
            atendimentos.Count,
            atendimentos.Count(a => a.Status == StatusAtendimento.Agendado),
            atendimentos.Count(a => a.Status == StatusAtendimento.Confirmado),
            atendimentos.Count(a => a.Status == StatusAtendimento.Realizado),
            atendimentos.Count(a => a.Status == StatusAtendimento.Cancelado),
            atendimentos.Count(a => a.Status == StatusAtendimento.NaoCompareceu),
            atendimentos
                .Where(a => a.EstaAberto || a.Status == StatusAtendimento.Realizado)
                .Sum(a => a.Valor),
            atendimentos.Select(AtendimentoResumoDto.De).ToList());
    }
}
