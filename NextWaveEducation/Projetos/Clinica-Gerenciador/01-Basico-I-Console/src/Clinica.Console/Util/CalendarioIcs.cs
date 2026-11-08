using System.Text;
using Clinica.Console.Entidades;

namespace Clinica.Console.Util;

/// <summary>
/// Gera um convite no formato iCalendar (RFC 5545). É o formato que o Google
/// Agenda, o Outlook e o Apple Calendar entendem: anexando o .ics ao e-mail
/// de confirmação, o paciente adiciona o atendimento à agenda com um clique,
/// sem o sistema precisar integrar com a API de cada calendário.
/// </summary>
public static class CalendarioIcs
{
    private const string NomeDaClinica = "Clínica Vida";

    public static string Gerar(Atendimento atendimento)
    {
        var inicio = atendimento.DataHora;
        var fim = atendimento.DataHoraFim;

        var resumo = $"{atendimento.Servico?.Nome ?? "Atendimento"} - {NomeDaClinica}";

        var descricao = new StringBuilder();
        descricao.Append($"Paciente: {atendimento.Paciente?.NomeCompleto}");
        descricao.Append($"\\nMédico: {atendimento.Medico?.NomeCompleto}");

        if (atendimento.Medico is not null)
            descricao.Append($" ({atendimento.Medico.CrmCompleto})");

        descricao.Append($"\\nServiço: {atendimento.Servico?.Nome}");
        descricao.Append($"\\nDuração: {atendimento.DuracaoMinutos} minutos");

        if (!string.IsNullOrWhiteSpace(atendimento.Observacoes))
            descricao.Append($"\\nObservações: {Escapar(atendimento.Observacoes)}");

        var ics = new StringBuilder();
        ics.AppendLine("BEGIN:VCALENDAR");
        ics.AppendLine("VERSION:2.0");
        ics.AppendLine("PRODID:-//Clinica Vida//Gerenciador de Clinica//PT-BR");
        ics.AppendLine("CALSCALE:GREGORIAN");
        ics.AppendLine("METHOD:REQUEST");
        ics.AppendLine("BEGIN:VEVENT");
        ics.AppendLine($"UID:atendimento-{atendimento.Id}@clinicavida.local");
        ics.AppendLine($"DTSTAMP:{Formatar(DateTime.UtcNow)}");
        ics.AppendLine($"DTSTART:{Formatar(inicio.ToUniversalTime())}");
        ics.AppendLine($"DTEND:{Formatar(fim.ToUniversalTime())}");
        ics.AppendLine($"SUMMARY:{Escapar(resumo)}");
        ics.AppendLine($"DESCRIPTION:{descricao}");
        ics.AppendLine($"LOCATION:{Escapar(NomeDaClinica)}");
        ics.AppendLine($"STATUS:{(atendimento.EstaAberto ? "CONFIRMED" : "CANCELLED")}");
        ics.AppendLine("SEQUENCE:0");

        if (!string.IsNullOrWhiteSpace(atendimento.Paciente?.Email))
        {
            ics.AppendLine(
                $"ATTENDEE;CN={Escapar(atendimento.Paciente.NomeCompleto)};" +
                $"RSVP=TRUE:mailto:{atendimento.Paciente.Email}");
        }

        // Alarme 24 horas antes, que é o mesmo momento em que o
        // background service dispara o lembrete por e-mail e SMS.
        ics.AppendLine("BEGIN:VALARM");
        ics.AppendLine("TRIGGER:-P1D");
        ics.AppendLine("ACTION:DISPLAY");
        ics.AppendLine($"DESCRIPTION:{Escapar($"Lembrete: {resumo} amanhã")}");
        ics.AppendLine("END:VALARM");

        ics.AppendLine("END:VEVENT");
        ics.AppendLine("END:VCALENDAR");

        return ics.ToString();
    }

    /// <summary>
    /// Link que abre o Google Agenda já com o evento preenchido. Serve como
    /// alternativa ao .ics quando o canal é SMS ou WhatsApp.
    /// </summary>
    public static string GerarLinkGoogleAgenda(Atendimento atendimento)
    {
        var inicio = Formatar(atendimento.DataHora.ToUniversalTime());
        var fim = Formatar(atendimento.DataHoraFim.ToUniversalTime());

        var titulo = Uri.EscapeDataString(
            $"{atendimento.Servico?.Nome ?? "Atendimento"} - {NomeDaClinica}");

        var detalhes = Uri.EscapeDataString(
            $"Médico: {atendimento.Medico?.NomeCompleto}");

        var local = Uri.EscapeDataString(NomeDaClinica);

        return "https://calendar.google.com/calendar/render?action=TEMPLATE"
             + $"&text={titulo}"
             + $"&dates={inicio}/{fim}"
             + $"&details={detalhes}"
             + $"&location={local}";
    }

    /// <summary>Formato de data do iCalendar: 20261108T143000Z.</summary>
    private static string Formatar(DateTime data) => data.ToString("yyyyMMdd'T'HHmmss'Z'");

    /// <summary>
    /// No iCalendar vírgula, ponto e vírgula e barra invertida têm significado
    /// e precisam ser escapados.
    /// </summary>
    private static string Escapar(string texto)
        => texto.Replace("\\", "\\\\")
                .Replace(";", "\\;")
                .Replace(",", "\\,")
                .Replace("\n", "\\n");
}
