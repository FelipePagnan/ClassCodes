using Livros.Application.Common;
using Livros.Domain.Exceptions;
using Livros.Domain.Repositories;
using MediatR;

namespace Livros.Application.Usuarios.Queries.ObterUsuarioPorId;

/// <summary>Detalhes do usuário com as avaliações feitas por ele.</summary>
public record ObterUsuarioPorIdQuery(int Id) : IRequest<UsuarioDetalheDto>;

public class ObterUsuarioPorIdQueryHandler
    : IRequestHandler<ObterUsuarioPorIdQuery, UsuarioDetalheDto>
{
    private readonly IUsuarioRepository _repository;

    public ObterUsuarioPorIdQueryHandler(IUsuarioRepository repository) => _repository = repository;

    public async Task<UsuarioDetalheDto> Handle(
        ObterUsuarioPorIdQuery request,
        CancellationToken cancellationToken)
    {
        var usuario = await _repository.ObterPorIdComAvaliacoesAsync(request.Id, cancellationToken)
            ?? throw NaoEncontradoException.Para("Usuário", request.Id);

        return UsuarioDetalheDto.De(usuario);
    }
}
