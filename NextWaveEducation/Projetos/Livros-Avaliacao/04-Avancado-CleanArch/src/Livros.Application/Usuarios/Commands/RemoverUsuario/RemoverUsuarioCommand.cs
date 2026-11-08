using Livros.Domain.Exceptions;
using Livros.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Livros.Application.Usuarios.Commands.RemoverUsuario;

public record RemoverUsuarioCommand(int Id) : IRequest;

public class RemoverUsuarioCommandHandler : IRequestHandler<RemoverUsuarioCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoverUsuarioCommandHandler> _logger;

    public RemoverUsuarioCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<RemoverUsuarioCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(RemoverUsuarioCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _unitOfWork.Usuarios
            .ObterPorIdComAvaliacoesAsync(request.Id, cancellationToken)
            ?? throw NaoEncontradoException.Para("Usuário", request.Id);

        // A entidade decide se pode ser removida.
        usuario.GarantirQuePodeSerRemovido();

        _unitOfWork.Usuarios.Remover(usuario);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Usuário {UsuarioId} removido.", request.Id);
    }
}
