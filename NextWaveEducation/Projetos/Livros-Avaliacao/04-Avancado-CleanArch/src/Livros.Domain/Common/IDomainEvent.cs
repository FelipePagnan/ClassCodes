namespace Livros.Domain.Common;

/// <summary>
/// Marca um evento de domínio. A camada de Application faz a ponte com o
/// MediatR, mantendo o domínio livre de dependências externas.
/// </summary>
public interface IDomainEvent
{
    DateTime OcorridoEm { get; }
}
