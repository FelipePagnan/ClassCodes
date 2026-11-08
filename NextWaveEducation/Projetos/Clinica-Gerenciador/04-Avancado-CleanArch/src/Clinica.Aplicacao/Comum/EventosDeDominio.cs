using Clinica.Dominio.Eventos;
using MediatR;

namespace Clinica.Aplicacao.Comum;

/// <summary>
/// Embalagem que transforma um evento de domínio numa notificação do MediatR.
///
/// Existe por um motivo de arquitetura: IEventoDeDominio mora no projeto de
/// domínio, que não referencia MediatR. Sem este invólucro, ou o domínio
/// passaria a depender de uma biblioteca de infraestrutura, ou não haveria
/// como publicar os eventos pelo MediatR. Com ele, cada camada fica no lugar.
/// </summary>
public class NotificacaoDeEvento<TEvento> : INotification
    where TEvento : IEventoDeDominio
{
    public NotificacaoDeEvento(TEvento evento) => Evento = evento;

    public TEvento Evento { get; }
}

/// <summary>
/// Contrato de publicação de eventos, declarado na aplicação e implementado
/// na infraestrutura (que é onde o MediatR está registrado).
/// </summary>
public interface IDespachanteDeEventos
{
    Task DespacharAsync(
        IEnumerable<IEventoDeDominio> eventos, CancellationToken cancellationToken = default);
}

/// <summary>
/// Publica cada evento pelo MediatR, embalado em NotificacaoDeEvento&lt;T&gt;.
///
/// O tipo concreto do evento só é conhecido em tempo de execução, então o
/// invólucro genérico é fechado por reflexão. É o preço de manter o domínio
/// sem dependência de MediatR, e acontece uma vez por evento publicado.
/// </summary>
public class DespachanteDeEventosMediatR : IDespachanteDeEventos
{
    private readonly IPublisher _publisher;

    public DespachanteDeEventosMediatR(IPublisher publisher) => _publisher = publisher;

    public async Task DespacharAsync(
        IEnumerable<IEventoDeDominio> eventos, CancellationToken cancellationToken = default)
    {
        foreach (var evento in eventos)
        {
            var tipoFechado = typeof(NotificacaoDeEvento<>).MakeGenericType(evento.GetType());

            var notificacao = (INotification)Activator.CreateInstance(tipoFechado, evento)!;

            await _publisher.Publish(notificacao, cancellationToken);
        }
    }
}
