using Livros.Domain.Exceptions;
using Livros.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Livros.Application.Livros.Commands.RemoverLivro;

public record RemoverLivroCommand(int Id) : IRequest;

public class RemoverLivroCommandHandler : IRequestHandler<RemoverLivroCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoverLivroCommandHandler> _logger;

    public RemoverLivroCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<RemoverLivroCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(RemoverLivroCommand request, CancellationToken cancellationToken)
    {
        var livro = await _unitOfWork.Livros.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw NaoEncontradoException.Para("Livro", request.Id);

        _unitOfWork.Livros.Remover(livro);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Livro {LivroId} removido.", request.Id);
    }
}
