using Caixinhas.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Caixinhas.Infrastructure.Services;

/// <summary>
/// Implementação de INotificadorDeMetas que registra em log. Em produção
/// seria substituída por push, e-mail ou publicação numa fila.
/// </summary>
public class NotificadorDeMetasLog : INotificadorDeMetas
{
    private readonly ILogger<NotificadorDeMetasLog> _logger;

    public NotificadorDeMetasLog(ILogger<NotificadorDeMetasLog> logger) => _logger = logger;

    public Task NotificarMetaAlcancadaAsync(
        string tituloObjetivo,
        decimal valorTotal,
        decimal quantidadeAlvo,
        int diasParaOPrazo,
        CancellationToken cancellationToken = default)
    {
        var excedente = valorTotal - quantidadeAlvo;

        var folga = diasParaOPrazo switch
        {
            > 0 => $"{diasParaOPrazo} dia(s) antes do prazo",
            0 => "exatamente no prazo",
            _ => $"{Math.Abs(diasParaOPrazo)} dia(s) depois do prazo"
        };

        _logger.LogInformation(
            "=== META ALCANÇADA === '{Titulo}': R$ {Valor} de R$ {Meta} " +
            "(excedente R$ {Excedente}), {Folga}.",
            tituloObjetivo, valorTotal, quantidadeAlvo, excedente, folga);

        return Task.CompletedTask;
    }

    public Task NotificarPrazoVencidoAsync(
        string tituloObjetivo,
        decimal valorTotal,
        decimal quantidadeAlvo,
        int diasDeAtraso,
        CancellationToken cancellationToken = default)
    {
        var faltam = quantidadeAlvo - valorTotal;

        _logger.LogWarning(
            "=== PRAZO VENCIDO === '{Titulo}' está {Dias} dia(s) atrasado. " +
            "Faltam R$ {Faltam} de R$ {Meta}.",
            tituloObjetivo, diasDeAtraso, faltam, quantidadeAlvo);

        return Task.CompletedTask;
    }
}
