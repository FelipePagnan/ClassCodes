using Clinica.Api.Aplicacao.InputModels;
using Clinica.Api.Aplicacao.ViewModels;
using Clinica.Api.Dominio.Entidades;
using Clinica.Api.Dominio.Enums;
using Clinica.Api.Dominio.Excecoes;
using Clinica.Api.Infraestrutura.Repositorios;
using FluentValidation;

namespace Clinica.Api.Aplicacao.Servicos;

/// <summary>
/// Caso de uso da agenda. A verificação de conflito mora aqui porque depende
/// do banco; a comparação de dois intervalos mora na entidade
/// (Atendimento.ConflitaCom), que é onde a regra pertence.
/// </summary>
public class AtendimentoAppService
{
    private readonly IAtendimentoRepositorio _repositorio;
    private readonly IPacienteRepositorio _pacienteRepositorio;
    private readonly IMedicoRepositorio _medicoRepositorio;
    private readonly IServicoRepositorio _servicoRepositorio;
    private readonly IValidator<AgendamentoInputModel> _validadorAgendamento;
    private readonly IValidator<RemarcacaoInputModel> _validadorRemarcacao;
    private readonly IValidator<AnexoInputModel> _validadorAnexo;

    public AtendimentoAppService(
        IAtendimentoRepositorio repositorio,
        IPacienteRepositorio pacienteRepositorio,
        IMedicoRepositorio medicoRepositorio,
        IServicoRepositorio servicoRepositorio,
        IValidator<AgendamentoInputModel> validadorAgendamento,
        IValidator<RemarcacaoInputModel> validadorRemarcacao,
        IValidator<AnexoInputModel> validadorAnexo)
    {
        _repositorio = repositorio;
        _pacienteRepositorio = pacienteRepositorio;
        _medicoRepositorio = medicoRepositorio;
        _servicoRepositorio = servicoRepositorio;
        _validadorAgendamento = validadorAgendamento;
        _validadorRemarcacao = validadorRemarcacao;
        _validadorAnexo = validadorAnexo;
    }

    // ------------------------------- Consultas -------------------------------

    public async Task<List<AtendimentoResumoViewModel>> ListarAsync(
        DateTime? dia,
        int? medicoId,
        int? pacienteId,
        StatusAtendimento? status,
        CancellationToken cancellationToken = default)
    {
        var atendimentos = await _repositorio.ListarAsync(
            dia, medicoId, pacienteId, status, cancellationToken);

        return atendimentos.Select(AtendimentoResumoViewModel.De).ToList();
    }

    public async Task<AtendimentoViewModel> ObterPorIdAsync(
        int id, CancellationToken cancellationToken = default)
        => AtendimentoViewModel.De(await ObterEntidadeAsync(id, cancellationToken));

    /// <summary>Agenda de um dia, com os totais por status.</summary>
    public async Task<AgendaDoDiaViewModel> ObterAgendaDoDiaAsync(
        DateTime dia, int? medicoId, CancellationToken cancellationToken = default)
    {
        var atendimentos = await _repositorio.ListarAsync(
            dia, medicoId, null, null, cancellationToken);

        return new AgendaDoDiaViewModel(
            dia.Date,
            atendimentos.Count,
            atendimentos.Count(a => a.Status == StatusAtendimento.Agendado),
            atendimentos.Count(a => a.Status == StatusAtendimento.Confirmado),
            atendimentos.Count(a => a.Status == StatusAtendimento.Realizado),
            atendimentos.Count(a => a.Status == StatusAtendimento.Cancelado),
            atendimentos.Count(a => a.Status == StatusAtendimento.NaoCompareceu),
            atendimentos
                .Where(a => a.EstaAberto || a.Status == StatusAtendimento.Realizado)
                .Sum(a => a.Valor),
            atendimentos.Select(AtendimentoResumoViewModel.De).ToList());
    }

    private async Task<Atendimento> ObterEntidadeAsync(
        int id, CancellationToken cancellationToken)
        => await _repositorio.ObterComAnexosAsync(id, cancellationToken)
           ?? throw new RegraNegocioException($"Atendimento {id} não encontrado.");

    // ------------------------------ Agendamento ------------------------------

    public async Task<AtendimentoViewModel> AgendarAsync(
        AgendamentoInputModel entrada, CancellationToken cancellationToken = default)
    {
        await _validadorAgendamento.ValidateAndThrowAsync(entrada, cancellationToken);

        var paciente = await _pacienteRepositorio.ObterPorIdAsync(
                           entrada.PacienteId, cancellationToken)
                       ?? throw new RegraNegocioException(
                           $"Paciente {entrada.PacienteId} não encontrado.");

        var medico = await _medicoRepositorio.ObterPorIdAsync(
                         entrada.MedicoId, cancellationToken)
                     ?? throw new RegraNegocioException(
                         $"Médico {entrada.MedicoId} não encontrado.");

        var servico = await _servicoRepositorio.ObterPorIdAsync(
                          entrada.ServicoId, cancellationToken)
                      ?? throw new RegraNegocioException(
                          $"Serviço {entrada.ServicoId} não encontrado.");

        // O construtor valida o horário e copia duração e valor do serviço.
        var atendimento = new Atendimento(
            paciente, medico, servico, entrada.DataHora, entrada.Observacoes);

        await GarantirQueNaoHaConflitoAsync(
            atendimento.DataHora,
            atendimento.DuracaoMinutos,
            medico,
            paciente,
            null,
            cancellationToken);

        _repositorio.Adicionar(atendimento);
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        return AtendimentoViewModel.De(atendimento);
    }

    public async Task<AtendimentoViewModel> RemarcarAsync(
        int id, RemarcacaoInputModel entrada, CancellationToken cancellationToken = default)
    {
        await _validadorRemarcacao.ValidateAndThrowAsync(entrada, cancellationToken);

        var atendimento = await ObterEntidadeAsync(id, cancellationToken);

        // A verificação vem antes de alterar o atendimento. Fazer o contrário
        // exigiria desfazer a mudança numa entidade já rastreada pelo
        // contexto, e um SaveChanges posterior gravaria a data recusada.
        await GarantirQueNaoHaConflitoAsync(
            entrada.NovaDataHora,
            atendimento.DuracaoMinutos,
            atendimento.Medico,
            atendimento.Paciente,
            id,
            cancellationToken);

        atendimento.Remarcar(entrada.NovaDataHora);

        await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        return AtendimentoViewModel.De(atendimento);
    }

    /// <summary>
    /// Verifica se o médico ou o paciente já têm compromisso no intervalo.
    /// </summary>
    private async Task GarantirQueNaoHaConflitoAsync(
        DateTime dataHora,
        int duracaoMinutos,
        Medico medico,
        Paciente paciente,
        int? ignorarId,
        CancellationToken cancellationToken)
    {
        var doDia = await _repositorio.ListarDoDiaParaConflitoAsync(
            dataHora, medico.Id, paciente.Id, ignorarId, cancellationToken);

        var conflitoMedico = doDia.FirstOrDefault(a =>
            a.MedicoId == medico.Id && a.ConflitaComIntervalo(dataHora, duracaoMinutos));

        if (conflitoMedico is not null)
            throw new RegraNegocioException(
                $"Dr(a). {medico.NomeCompleto} já tem atendimento das " +
                $"{conflitoMedico.DataHora:HH:mm} às {conflitoMedico.DataHoraFim:HH:mm} " +
                $"em {conflitoMedico.DataHora:dd/MM/yyyy} " +
                $"(paciente {conflitoMedico.Paciente.NomeCompleto}).");

        var conflitoPaciente = doDia.FirstOrDefault(a =>
            a.PacienteId == paciente.Id && a.ConflitaComIntervalo(dataHora, duracaoMinutos));

        if (conflitoPaciente is not null)
            throw new RegraNegocioException(
                $"{paciente.NomeCompleto} já tem atendimento das " +
                $"{conflitoPaciente.DataHora:HH:mm} às {conflitoPaciente.DataHoraFim:HH:mm} " +
                $"em {conflitoPaciente.DataHora:dd/MM/yyyy} " +
                $"(Dr(a). {conflitoPaciente.Medico.NomeCompleto}).");
    }

    // ---------------------------- Mudanças de status ----------------------------

    public async Task<AtendimentoViewModel> MarcarComoRealizadoAsync(
        int id, CancellationToken cancellationToken = default)
    {
        var atendimento = await ObterEntidadeAsync(id, cancellationToken);
        atendimento.MarcarComoRealizado();

        await _repositorio.SalvarAlteracoesAsync(cancellationToken);
        return AtendimentoViewModel.De(atendimento);
    }

    public async Task<AtendimentoViewModel> CancelarAsync(
        int id, string? motivo, CancellationToken cancellationToken = default)
    {
        var atendimento = await ObterEntidadeAsync(id, cancellationToken);
        atendimento.Cancelar(motivo);

        await _repositorio.SalvarAlteracoesAsync(cancellationToken);
        return AtendimentoViewModel.De(atendimento);
    }

    public async Task<AtendimentoViewModel> MarcarComoFaltaAsync(
        int id, CancellationToken cancellationToken = default)
    {
        var atendimento = await ObterEntidadeAsync(id, cancellationToken);
        atendimento.MarcarComoNaoCompareceu();

        await _repositorio.SalvarAlteracoesAsync(cancellationToken);
        return AtendimentoViewModel.De(atendimento);
    }

    public async Task<AtendimentoViewModel> AlterarObservacoesAsync(
        int id, string? observacoes, CancellationToken cancellationToken = default)
    {
        var atendimento = await ObterEntidadeAsync(id, cancellationToken);
        atendimento.AlterarObservacoes(observacoes);

        await _repositorio.SalvarAlteracoesAsync(cancellationToken);
        return AtendimentoViewModel.De(atendimento);
    }

    public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var atendimento = await ObterEntidadeAsync(id, cancellationToken);
        atendimento.DeletarComAnexos();

        await _repositorio.SalvarAlteracoesAsync(cancellationToken);
    }

    // -------------------------------- Anexos --------------------------------

    public async Task<List<AnexoViewModel>> ListarAnexosAsync(
        int atendimentoId, TipoAnexo? tipo, CancellationToken cancellationToken = default)
    {
        var atendimento = await ObterEntidadeAsync(atendimentoId, cancellationToken);

        var anexos = atendimento.Anexos.AsEnumerable();
        if (tipo is not null) anexos = anexos.Where(x => x.Tipo == tipo);

        return anexos
            .OrderByDescending(x => x.DataCriacao)
            .Select(AnexoViewModel.De)
            .ToList();
    }

    public async Task<List<AnexoViewModel>> ListarAnexosDoPacienteAsync(
        int pacienteId, CancellationToken cancellationToken = default)
    {
        var anexos = await _repositorio.ListarAnexosDoPacienteAsync(
            pacienteId, cancellationToken);

        return anexos.Select(AnexoViewModel.De).ToList();
    }

    /// <summary>Entidade do anexo, para o endpoint de download.</summary>
    public async Task<Anexo> ObterAnexoParaDownloadAsync(
        int atendimentoId, int anexoId, CancellationToken cancellationToken = default)
    {
        var atendimento = await ObterEntidadeAsync(atendimentoId, cancellationToken);

        return atendimento.Anexos.FirstOrDefault(x => x.Id == anexoId)
               ?? throw new RegraNegocioException(
                   $"Anexo {anexoId} não encontrado no atendimento {atendimentoId}.");
    }

    public async Task<AnexoViewModel> AdicionarAnexoDeTextoAsync(
        int atendimentoId, AnexoInputModel entrada, CancellationToken cancellationToken = default)
    {
        await _validadorAnexo.ValidateAndThrowAsync(entrada, cancellationToken);

        var atendimento = await ObterEntidadeAsync(atendimentoId, cancellationToken);

        // A regra de que só atendimento realizado aceita anexo está na
        // entidade, em AnexarDocumento.
        var anexo = atendimento.AnexarDocumento(
            entrada.Tipo,
            entrada.NomeArquivo,
            "text/plain; charset=utf-8",
            System.Text.Encoding.UTF8.GetBytes(entrada.Conteudo),
            entrada.Descricao ?? entrada.Conteudo);

        await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        return AnexoViewModel.De(anexo);
    }

    public async Task<AnexoViewModel> AdicionarAnexoDeArquivoAsync(
        int atendimentoId,
        TipoAnexo tipo,
        string nomeArquivo,
        string tipoConteudo,
        byte[] conteudo,
        string? descricao,
        CancellationToken cancellationToken = default)
    {
        var atendimento = await ObterEntidadeAsync(atendimentoId, cancellationToken);

        var anexo = atendimento.AnexarDocumento(
            tipo, nomeArquivo, tipoConteudo, conteudo, descricao);

        await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        return AnexoViewModel.De(anexo);
    }

    /// <summary>Remoção lógica: documento clínico não se apaga.</summary>
    public async Task RemoverAnexoAsync(
        int atendimentoId, int anexoId, CancellationToken cancellationToken = default)
    {
        var anexo = await ObterAnexoParaDownloadAsync(
            atendimentoId, anexoId, cancellationToken);

        anexo.Deletar();
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);
    }
}
