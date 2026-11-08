using Livros.Domain.Common;

namespace Livros.Application.Common.Interfaces;

/// <summary>
/// Publica os eventos de domínio acumulados nas entidades.
/// Chamado pela Unit of Work ao confirmar as alterações.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DespacharAsync(IEnumerable<IDomainEvent> eventos, CancellationToken cancellationToken = default);
}
