using Clinica.Api.Entidades;
using Clinica.Api.Enums;
using Clinica.Api.Modelos;
using Clinica.Api.Seguranca;
using Clinica.Api.Servicos;
using Clinica.Api.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api.Controllers;

/// <summary>
/// CRUD de atendimentos (a agenda), confirmação, mudanças de status e os
/// anexos clínicos de cada atendimento.
/// </summary>
[ApiController]
[Route("api/atendimentos")]
[Authorize(Roles = Perfis.Todos)]
public class AtendimentosController : ControllerBase
{
    private readonly AtendimentoService _service;
    private readonly NotificacaoService _notificacaoService;

    public AtendimentosController(
        AtendimentoService service, NotificacaoService notificacaoService)
    {
        _service = service;
        _notificacaoService = notificacaoService;
    }

    /// <summary>
    /// Lista a agenda. O médico logado vê apenas os próprios atendimentos;
    /// administração e recepção veem todos.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<Atendimento>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] DateTime? dia,
        [FromQuery] int? medicoId,
        [FromQuery] int? pacienteId,
        [FromQuery] StatusAtendimento? status)
    {
        // Se o usuário é médico, o filtro dele vence o que vier na query.
        var filtro = User.FiltroDeMedico() ?? medicoId;

        return Ok(await _service.ListarAsync(dia, filtro, pacienteId, status));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Atendimento), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObterPorId(int id)
        => Ok(await _service.ObterPorIdAsync(id));

    /// <summary>Agenda um atendimento, recusando conflito de horário.</summary>
    [HttpPost]
    [Authorize(Roles = Perfis.AdministradorOuRecepcionista)]
    [ProducesResponseType(typeof(Atendimento), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Agendar([FromBody] AgendamentoRequisicao requisicao)
    {
        var atendimento = await _service.AgendarAsync(requisicao);
        return CreatedAtAction(nameof(ObterPorId), new { id = atendimento.Id }, atendimento);
    }

    /// <summary>
    /// Remarca o atendimento. A confirmação e o lembrete são zerados, porque
    /// o paciente foi avisado de um horário que já não vale.
    /// </summary>
    [HttpPut("{id:int}/remarcar")]
    [Authorize(Roles = Perfis.AdministradorOuRecepcionista)]
    [ProducesResponseType(typeof(Atendimento), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Remarcar(int id, [FromBody] RemarcacaoRequisicao requisicao)
        => Ok(await _service.RemarcarAsync(id, requisicao.NovaDataHora));

    /// <summary>
    /// Confirma o agendamento e dispara a confirmação por e-mail (com o .ics
    /// anexado) e por SMS (com o link do Google Agenda).
    /// </summary>
    [HttpPost("{id:int}/confirmar")]
    [Authorize(Roles = Perfis.AdministradorOuRecepcionista)]
    [ProducesResponseType(typeof(List<Notificacao>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Confirmar(int id)
        => Ok(await _notificacaoService.EnviarConfirmacaoAsync(id));

    [HttpPost("{id:int}/realizado")]
    [Authorize(Roles = Perfis.AdministradorOuMedico)]
    [ProducesResponseType(typeof(Atendimento), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarcarComoRealizado(int id)
        => Ok(await _service.MarcarComoRealizadoAsync(id));

    [HttpPost("{id:int}/cancelar")]
    [Authorize(Roles = Perfis.Todos)]
    [ProducesResponseType(typeof(Atendimento), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancelar(int id, [FromBody] CancelamentoRequisicao? requisicao)
        => Ok(await _service.CancelarAsync(id, requisicao?.Motivo));

    [HttpPost("{id:int}/falta")]
    [Authorize(Roles = Perfis.Todos)]
    [ProducesResponseType(typeof(Atendimento), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarcarComoFalta(int id)
        => Ok(await _service.MarcarComoFaltaAsync(id));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Perfis.AdministradorOuRecepcionista)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Remover(int id)
    {
        await _service.RemoverAsync(id);
        return NoContent();
    }

    // ----------------------------- Google Agenda -----------------------------

    /// <summary>
    /// Baixa o convite .ics do atendimento, que o Google Agenda, o Outlook e
    /// o Apple Calendar abrem direto.
    /// </summary>
    [HttpGet("{id:int}/agenda.ics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BaixarIcs(int id)
    {
        var atendimento = await _service.ObterPorIdAsync(id);
        var conteudo = System.Text.Encoding.UTF8.GetBytes(CalendarioIcs.Gerar(atendimento));

        return File(conteudo, "text/calendar", $"atendimento-{id}.ics");
    }

    /// <summary>Link que abre o Google Agenda com o evento preenchido.</summary>
    [HttpGet("{id:int}/link-google-agenda")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LinkGoogleAgenda(int id)
    {
        var atendimento = await _service.ObterPorIdAsync(id);
        return Ok(new { link = CalendarioIcs.GerarLinkGoogleAgenda(atendimento) });
    }

    // -------------------------------- Anexos --------------------------------

    /// <summary>Lista os anexos do atendimento, podendo filtrar pelo tipo.</summary>
    [HttpGet("{id:int}/anexos")]
    [ProducesResponseType(typeof(List<Anexo>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListarAnexos(int id, [FromQuery] TipoAnexo? tipo)
        => Ok(await _service.ListarAnexosAsync(id, tipo));

    [HttpGet("{id:int}/anexos/{anexoId:int}")]
    [ProducesResponseType(typeof(Anexo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObterAnexo(int id, int anexoId)
        => Ok(await _service.ObterAnexoAsync(id, anexoId));

    /// <summary>Baixa o arquivo do anexo.</summary>
    [HttpGet("{id:int}/anexos/{anexoId:int}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BaixarAnexo(int id, int anexoId)
    {
        var anexo = await _service.ObterAnexoAsync(id, anexoId);
        return File(anexo.Conteudo, anexo.TipoConteudo, anexo.NomeArquivo);
    }

    /// <summary>
    /// Cria um anexo de texto (atestado ou receita digitados). Só atendimentos
    /// realizados aceitam anexo.
    /// </summary>
    [HttpPost("{id:int}/anexos")]
    [Authorize(Roles = Perfis.AdministradorOuMedico)]
    [ProducesResponseType(typeof(Anexo), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AdicionarAnexo(int id, [FromBody] AnexoRequisicao requisicao)
    {
        var anexo = await _service.AdicionarAnexoDeTextoAsync(id, requisicao);

        return CreatedAtAction(nameof(ObterAnexo),
            new { id, anexoId = anexo.Id }, anexo);
    }

    /// <summary>Envia um arquivo como anexo (PDF digitalizado, foto de exame).</summary>
    [HttpPost("{id:int}/anexos/upload")]
    [Authorize(Roles = Perfis.AdministradorOuMedico)]
    [ProducesResponseType(typeof(Anexo), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnviarArquivo(
        int id,
        [FromForm] TipoAnexo tipo,
        IFormFile arquivo,
        [FromForm] string? descricao)
    {
        if (arquivo is null || arquivo.Length == 0)
            return BadRequest(new { mensagem = "Envie um arquivo." });

        using var memoria = new MemoryStream();
        await arquivo.CopyToAsync(memoria);

        var anexo = await _service.AdicionarAnexoAsync(
            id, tipo, arquivo.FileName, memoria.ToArray(), arquivo.ContentType, descricao);

        return CreatedAtAction(nameof(ObterAnexo),
            new { id, anexoId = anexo.Id }, anexo);
    }

    /// <summary>Remoção lógica: documento clínico não se apaga.</summary>
    [HttpDelete("{id:int}/anexos/{anexoId:int}")]
    [Authorize(Roles = Perfis.AdministradorOuMedico)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoverAnexo(int id, int anexoId)
    {
        await _service.RemoverAnexoAsync(id, anexoId);
        return NoContent();
    }
}
