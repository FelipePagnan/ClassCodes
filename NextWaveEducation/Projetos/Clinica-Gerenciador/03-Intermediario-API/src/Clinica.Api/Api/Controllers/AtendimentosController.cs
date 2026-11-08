using Clinica.Api.Api.Seguranca;
using Clinica.Api.Aplicacao.InputModels;
using Clinica.Api.Aplicacao.Servicos;
using Clinica.Api.Aplicacao.ViewModels;
using Clinica.Api.Dominio.Enums;
using Clinica.Api.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api.Api.Controllers;

/// <summary>
/// A agenda: CRUD de atendimentos, confirmação, mudanças de status, convite
/// para o Google Agenda e os anexos clínicos de cada atendimento.
/// </summary>
[ApiController]
[Route("api/atendimentos")]
[Authorize(Roles = Perfis.Todos)]
public class AtendimentosController : ControllerBase
{
    private readonly AtendimentoAppService _service;
    private readonly NotificacaoAppService _notificacaoService;

    public AtendimentosController(
        AtendimentoAppService service, NotificacaoAppService notificacaoService)
    {
        _service = service;
        _notificacaoService = notificacaoService;
    }

    /// <summary>
    /// Lista a agenda. O médico logado vê apenas os próprios atendimentos;
    /// administração e recepção veem todos.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<AtendimentoResumoViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] DateTime? dia,
        [FromQuery] int? medicoId,
        [FromQuery] int? pacienteId,
        [FromQuery] StatusAtendimento? status,
        CancellationToken cancellationToken)
    {
        // Para o perfil Médico o filtro do token vence o que vier na query.
        var filtro = User.FiltroDeMedico() ?? medicoId;

        return Ok(await _service.ListarAsync(
            dia, filtro, pacienteId, status, cancellationToken));
    }

    /// <summary>Agenda de um dia, com os totais por status.</summary>
    [HttpGet("agenda")]
    [ProducesResponseType(typeof(AgendaDoDiaViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Agenda(
        [FromQuery] DateTime? dia,
        [FromQuery] int? medicoId,
        CancellationToken cancellationToken)
    {
        var filtro = User.FiltroDeMedico() ?? medicoId;

        return Ok(await _service.ObterAgendaDoDiaAsync(
            dia ?? DateTime.Today, filtro, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AtendimentoViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken cancellationToken)
        => Ok(await _service.ObterPorIdAsync(id, cancellationToken));

    /// <summary>Agenda um atendimento, recusando conflito de horário.</summary>
    [HttpPost]
    [Authorize(Roles = Perfis.AdministradorOuRecepcionista)]
    [ProducesResponseType(typeof(AtendimentoViewModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Agendar(
        [FromBody] AgendamentoInputModel entrada, CancellationToken cancellationToken)
    {
        var atendimento = await _service.AgendarAsync(entrada, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = atendimento.Id }, atendimento);
    }

    /// <summary>
    /// Remarca. Confirmação e lembrete são zerados, porque o paciente foi
    /// avisado de um horário que já não vale.
    /// </summary>
    [HttpPut("{id:int}/remarcar")]
    [Authorize(Roles = Perfis.AdministradorOuRecepcionista)]
    [ProducesResponseType(typeof(AtendimentoViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Remarcar(
        int id, [FromBody] RemarcacaoInputModel entrada, CancellationToken cancellationToken)
        => Ok(await _service.RemarcarAsync(id, entrada, cancellationToken));

    [HttpPut("{id:int}/observacoes")]
    [ProducesResponseType(typeof(AtendimentoViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AlterarObservacoes(
        int id, [FromBody] ObservacoesInputModel entrada, CancellationToken cancellationToken)
        => Ok(await _service.AlterarObservacoesAsync(id, entrada.Observacoes, cancellationToken));

    /// <summary>
    /// Confirma o agendamento e dispara a confirmação por e-mail (com o .ics
    /// anexado) e por SMS (com o link do Google Agenda).
    /// </summary>
    [HttpPost("{id:int}/confirmar")]
    [Authorize(Roles = Perfis.AdministradorOuRecepcionista)]
    [ProducesResponseType(typeof(List<Notificacao>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Confirmar(int id, CancellationToken cancellationToken)
        => Ok(await _notificacaoService.ConfirmarEEnviarAsync(id, cancellationToken));

    [HttpPost("{id:int}/realizado")]
    [Authorize(Roles = Perfis.AdministradorOuMedico)]
    [ProducesResponseType(typeof(AtendimentoViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarcarComoRealizado(
        int id, CancellationToken cancellationToken)
        => Ok(await _service.MarcarComoRealizadoAsync(id, cancellationToken));

    [HttpPost("{id:int}/cancelar")]
    [ProducesResponseType(typeof(AtendimentoViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancelar(
        int id,
        [FromBody] CancelamentoInputModel? entrada,
        CancellationToken cancellationToken)
        => Ok(await _service.CancelarAsync(id, entrada?.Motivo, cancellationToken));

    [HttpPost("{id:int}/falta")]
    [ProducesResponseType(typeof(AtendimentoViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarcarComoFalta(int id, CancellationToken cancellationToken)
        => Ok(await _service.MarcarComoFaltaAsync(id, cancellationToken));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Perfis.AdministradorOuRecepcionista)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Remover(int id, CancellationToken cancellationToken)
    {
        await _service.RemoverAsync(id, cancellationToken);
        return NoContent();
    }

    // ------------------------------ Google Agenda ------------------------------

    /// <summary>
    /// Baixa o convite .ics, que o Google Agenda, o Outlook e o Apple
    /// Calendar abrem direto.
    /// </summary>
    [HttpGet("{id:int}/agenda.ics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BaixarIcs(int id, CancellationToken cancellationToken)
    {
        var ics = await _notificacaoService.GerarIcsAsync(id, cancellationToken);

        return File(
            System.Text.Encoding.UTF8.GetBytes(ics), "text/calendar", $"atendimento-{id}.ics");
    }

    /// <summary>Link que abre o Google Agenda com o evento preenchido.</summary>
    [HttpGet("{id:int}/link-google-agenda")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LinkGoogleAgenda(int id, CancellationToken cancellationToken)
        => Ok(new { link = await _notificacaoService.GerarLinkGoogleAgendaAsync(
            id, cancellationToken) });

    // --------------------------------- Anexos ---------------------------------

    /// <summary>Lista os anexos do atendimento, podendo filtrar pelo tipo.</summary>
    [HttpGet("{id:int}/anexos")]
    [ProducesResponseType(typeof(List<AnexoViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListarAnexos(
        int id, [FromQuery] TipoAnexo? tipo, CancellationToken cancellationToken)
        => Ok(await _service.ListarAnexosAsync(id, tipo, cancellationToken));

    /// <summary>Baixa o arquivo do anexo.</summary>
    [HttpGet("{id:int}/anexos/{anexoId:int}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BaixarAnexo(
        int id, int anexoId, CancellationToken cancellationToken)
    {
        var anexo = await _service.ObterAnexoParaDownloadAsync(id, anexoId, cancellationToken);
        return File(anexo.Conteudo, anexo.TipoConteudo, anexo.NomeArquivo);
    }

    /// <summary>
    /// Cria um anexo de texto (atestado ou receita digitados). Só atendimentos
    /// realizados aceitam anexo.
    /// </summary>
    [HttpPost("{id:int}/anexos")]
    [Authorize(Roles = Perfis.AdministradorOuMedico)]
    [ProducesResponseType(typeof(AnexoViewModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AdicionarAnexo(
        int id, [FromBody] AnexoInputModel entrada, CancellationToken cancellationToken)
    {
        var anexo = await _service.AdicionarAnexoDeTextoAsync(id, entrada, cancellationToken);

        return CreatedAtAction(nameof(BaixarAnexo), new { id, anexoId = anexo.Id }, anexo);
    }

    /// <summary>Envia um arquivo como anexo (PDF digitalizado, foto de exame).</summary>
    [HttpPost("{id:int}/anexos/upload")]
    [Authorize(Roles = Perfis.AdministradorOuMedico)]
    [ProducesResponseType(typeof(AnexoViewModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnviarArquivo(
        int id,
        [FromForm] TipoAnexo tipo,
        IFormFile arquivo,
        [FromForm] string? descricao,
        CancellationToken cancellationToken)
    {
        if (arquivo is null || arquivo.Length == 0)
            return BadRequest(new { mensagem = "Envie um arquivo." });

        if (arquivo.Length > Dominio.Entidades.Anexo.TamanhoMaximoBytes)
            return BadRequest(new { mensagem = "O anexo deve ter no máximo 10MB." });

        using var memoria = new MemoryStream();
        await arquivo.CopyToAsync(memoria, cancellationToken);

        var anexo = await _service.AdicionarAnexoDeArquivoAsync(
            id,
            tipo,
            arquivo.FileName,
            string.IsNullOrWhiteSpace(arquivo.ContentType)
                ? Arquivos.DetectarTipoConteudo(arquivo.FileName)
                : arquivo.ContentType,
            memoria.ToArray(),
            descricao,
            cancellationToken);

        return CreatedAtAction(nameof(BaixarAnexo), new { id, anexoId = anexo.Id }, anexo);
    }

    /// <summary>Remoção lógica: documento clínico não se apaga.</summary>
    [HttpDelete("{id:int}/anexos/{anexoId:int}")]
    [Authorize(Roles = Perfis.AdministradorOuMedico)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoverAnexo(
        int id, int anexoId, CancellationToken cancellationToken)
    {
        await _service.RemoverAnexoAsync(id, anexoId, cancellationToken);
        return NoContent();
    }
}
