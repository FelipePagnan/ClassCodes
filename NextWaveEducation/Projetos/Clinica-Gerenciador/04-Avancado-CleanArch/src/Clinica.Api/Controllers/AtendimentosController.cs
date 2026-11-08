using Clinica.Api.Seguranca;
using Clinica.Aplicacao.Anexos;
using Clinica.Aplicacao.Atendimentos;
using Clinica.Aplicacao.Contratos;
using Clinica.Aplicacao.Dtos;
using Clinica.Aplicacao.Notificacoes;
using Clinica.Dominio.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api.Controllers;

/// <summary>
/// A agenda: CRUD de atendimentos, confirmação, mudanças de status, convite
/// para o Google Agenda e os anexos clínicos de cada atendimento.
/// </summary>
[ApiController]
[Route("api/atendimentos")]
[Authorize(Roles = Perfis.Todos)]
public class AtendimentosController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IUsuarioAtual _usuarioAtual;

    public AtendimentosController(ISender sender, IUsuarioAtual usuarioAtual)
    {
        _sender = sender;
        _usuarioAtual = usuarioAtual;
    }

    /// <summary>
    /// Lista a agenda. O médico logado vê apenas os próprios atendimentos;
    /// administração e recepção veem todos.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<AtendimentoResumoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] DateTime? dia,
        [FromQuery] int? medicoId,
        [FromQuery] int? pacienteId,
        [FromQuery] StatusAtendimento? status,
        CancellationToken cancellationToken)
    {
        // Para o perfil Médico o filtro do token vence o que vier na query.
        var filtro = _usuarioAtual.FiltroDeMedico ?? medicoId;

        return Ok(await _sender.Send(
            new ListarAtendimentos(dia, filtro, pacienteId, status), cancellationToken));
    }

    /// <summary>Agenda de um dia, com os totais por status.</summary>
    [HttpGet("agenda")]
    [ProducesResponseType(typeof(AgendaDoDiaDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Agenda(
        [FromQuery] DateTime? dia,
        [FromQuery] int? medicoId,
        CancellationToken cancellationToken)
    {
        var filtro = _usuarioAtual.FiltroDeMedico ?? medicoId;

        return Ok(await _sender.Send(
            new ObterAgendaDoDia(dia ?? DateTime.Today, filtro), cancellationToken));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AtendimentoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new ObterAtendimento(id), cancellationToken));

    /// <summary>Agenda um atendimento, recusando conflito de horário.</summary>
    [HttpPost]
    [Authorize(Roles = Perfis.AdministradorOuRecepcionista)]
    [ProducesResponseType(typeof(AtendimentoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Agendar(
        [FromBody] AgendarAtendimento comando, CancellationToken cancellationToken)
    {
        var atendimento = await _sender.Send(comando, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = atendimento.Id }, atendimento);
    }

    /// <summary>
    /// Remarca. A confirmação é reenviada automaticamente pelo handler do
    /// evento AtendimentoRemarcado — o controller não sabe disso, e é essa a
    /// ideia.
    /// </summary>
    [HttpPut("{id:int}/remarcar")]
    [Authorize(Roles = Perfis.AdministradorOuRecepcionista)]
    [ProducesResponseType(typeof(AtendimentoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Remarcar(
        int id, [FromBody] RemarcacaoCorpo corpo, CancellationToken cancellationToken)
        => Ok(await _sender.Send(
            new RemarcarAtendimento(id, corpo.NovaDataHora), cancellationToken));

    [HttpPut("{id:int}/observacoes")]
    [ProducesResponseType(typeof(AtendimentoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AlterarObservacoes(
        int id, [FromBody] ObservacoesCorpo corpo, CancellationToken cancellationToken)
        => Ok(await _sender.Send(
            new AlterarObservacoesDoAtendimento(id, corpo.Observacoes), cancellationToken));

    /// <summary>
    /// Confirma o agendamento. O envio da confirmação por e-mail (com .ics) e
    /// por SMS (com link do Google Agenda) acontece no handler do evento
    /// AtendimentoConfirmado.
    /// </summary>
    [HttpPost("{id:int}/confirmar")]
    [Authorize(Roles = Perfis.AdministradorOuRecepcionista)]
    [ProducesResponseType(typeof(AtendimentoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Confirmar(int id, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new ConfirmarAtendimento(id), cancellationToken));

    [HttpPost("{id:int}/realizado")]
    [Authorize(Roles = Perfis.AdministradorOuMedico)]
    [ProducesResponseType(typeof(AtendimentoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarcarComoRealizado(
        int id, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new MarcarAtendimentoComoRealizado(id), cancellationToken));

    [HttpPost("{id:int}/cancelar")]
    [ProducesResponseType(typeof(AtendimentoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancelar(
        int id, [FromBody] CancelamentoCorpo? corpo, CancellationToken cancellationToken)
        => Ok(await _sender.Send(
            new CancelarAtendimento(id, corpo?.Motivo), cancellationToken));

    [HttpPost("{id:int}/falta")]
    [ProducesResponseType(typeof(AtendimentoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarcarComoFalta(int id, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new MarcarFalta(id), cancellationToken));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Perfis.AdministradorOuRecepcionista)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Remover(int id, CancellationToken cancellationToken)
    {
        await _sender.Send(new RemoverAtendimento(id), cancellationToken);
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
        var ics = await _sender.Send(new ObterConviteIcs(id), cancellationToken);

        return File(
            System.Text.Encoding.UTF8.GetBytes(ics), "text/calendar", $"atendimento-{id}.ics");
    }

    /// <summary>Link que abre o Google Agenda com o evento preenchido.</summary>
    [HttpGet("{id:int}/link-google-agenda")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LinkGoogleAgenda(int id, CancellationToken cancellationToken)
        => Ok(new
        {
            link = await _sender.Send(new ObterLinkGoogleAgenda(id), cancellationToken)
        });

    // --------------------------------- Anexos ---------------------------------

    /// <summary>Lista os anexos do atendimento, podendo filtrar pelo tipo.</summary>
    [HttpGet("{id:int}/anexos")]
    [ProducesResponseType(typeof(List<AnexoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListarAnexos(
        int id, [FromQuery] TipoAnexo? tipo, CancellationToken cancellationToken)
        => Ok(await _sender.Send(
            new ListarAnexosDoAtendimento(id, tipo), cancellationToken));

    /// <summary>Baixa o arquivo do anexo.</summary>
    [HttpGet("{id:int}/anexos/{anexoId:int}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BaixarAnexo(
        int id, int anexoId, CancellationToken cancellationToken)
    {
        var anexo = await _sender.Send(
            new ObterAnexoParaDownload(id, anexoId), cancellationToken);

        return File(anexo.Conteudo, anexo.TipoConteudo, anexo.NomeArquivo);
    }

    /// <summary>
    /// Cria um anexo de texto (atestado ou receita digitados). Só atendimentos
    /// realizados aceitam anexo.
    /// </summary>
    [HttpPost("{id:int}/anexos")]
    [Authorize(Roles = Perfis.AdministradorOuMedico)]
    [ProducesResponseType(typeof(AnexoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AdicionarAnexo(
        int id, [FromBody] AnexoDeTextoCorpo corpo, CancellationToken cancellationToken)
    {
        var anexo = await _sender.Send(corpo.ParaComando(id), cancellationToken);

        return CreatedAtAction(nameof(BaixarAnexo), new { id, anexoId = anexo.Id }, anexo);
    }

    /// <summary>Envia um arquivo como anexo (PDF digitalizado, foto de exame).</summary>
    [HttpPost("{id:int}/anexos/upload")]
    [Authorize(Roles = Perfis.AdministradorOuMedico)]
    [ProducesResponseType(typeof(AnexoDto), StatusCodes.Status201Created)]
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

        using var memoria = new MemoryStream();
        await arquivo.CopyToAsync(memoria, cancellationToken);

        var anexo = await _sender.Send(
            new AnexarArquivo(
                id,
                tipo,
                arquivo.FileName,
                string.IsNullOrWhiteSpace(arquivo.ContentType)
                    ? "application/octet-stream"
                    : arquivo.ContentType,
                memoria.ToArray(),
                descricao),
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
        await _sender.Send(new RemoverAnexo(id, anexoId), cancellationToken);
        return NoContent();
    }
}

// ============================== Corpos das rotas ==============================
// Os Ids vêm da rota, então não entram no corpo: assim não há como mandar um
// Id na URL e outro no JSON.

public record RemarcacaoCorpo(DateTime NovaDataHora);

public record CancelamentoCorpo(string? Motivo);

public record ObservacoesCorpo(string? Observacoes);

public record AnexoDeTextoCorpo(
    TipoAnexo Tipo,
    string NomeArquivo,
    string Conteudo,
    string? Descricao)
{
    public AnexarDocumentoDeTexto ParaComando(int atendimentoId) => new(
        atendimentoId, Tipo, NomeArquivo, Conteudo, Descricao);
}
