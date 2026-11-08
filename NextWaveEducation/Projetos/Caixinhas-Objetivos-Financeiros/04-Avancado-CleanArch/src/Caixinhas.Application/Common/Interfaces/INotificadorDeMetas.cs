namespace Caixinhas.Application.Common.Interfaces;

/// <summary>
/// Canal de notificação de metas alcançadas. A implementação pode logar,
/// enviar push, e-mail ou publicar numa fila.
/// </summary>
public interface INotificadorDeMetas
{
    Task NotificarMetaAlcancadaAsync(
        string tituloObjetivo,
        decimal valorTotal,
        decimal quantidadeAlvo,
        int diasParaOPrazo,
        CancellationToken cancellationToken = default);

    Task NotificarPrazoVencidoAsync(
        string tituloObjetivo,
        decimal valorTotal,
        decimal quantidadeAlvo,
        int diasDeAtraso,
        CancellationToken cancellationToken = default);
}
