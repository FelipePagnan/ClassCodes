using FluentValidation;
using MediatR;

namespace BancoSangue.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior do MediatR que executa os validadores do FluentValidation
/// antes de qualquer command/query chegar ao seu handler.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var contexto = new ValidationContext<TRequest>(request);

        var resultados = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(contexto, cancellationToken)));

        var falhas = resultados
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (falhas.Count != 0)
            throw new ValidationException(falhas);

        return await next();
    }
}
