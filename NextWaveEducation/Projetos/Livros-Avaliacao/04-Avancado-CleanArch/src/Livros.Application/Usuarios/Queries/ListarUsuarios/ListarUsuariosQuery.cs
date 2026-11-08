using Livros.Application.Common;
using Livros.Domain.Repositories;
using MediatR;

namespace Livros.Application.Usuarios.Queries.ListarUsuarios;

public record ListarUsuariosQuery : IRequest<List<UsuarioDto>>;

public class ListarUsuariosQueryHandler : IRequestHandler<ListarUsuariosQuery, List<UsuarioDto>>
{
    private readonly IUsuarioRepository _repository;

    public ListarUsuariosQueryHandler(IUsuarioRepository repository) => _repository = repository;

    public async Task<List<UsuarioDto>> Handle(
        ListarUsuariosQuery request,
        CancellationToken cancellationToken)
    {
        var usuarios = await _repository.ObterTodosAsync(cancellationToken);
        return usuarios.Select(UsuarioDto.De).ToList();
    }
}
