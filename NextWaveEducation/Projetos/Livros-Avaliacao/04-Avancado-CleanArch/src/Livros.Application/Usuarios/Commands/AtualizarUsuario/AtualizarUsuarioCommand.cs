using FluentValidation;
using Livros.Application.Common;
using Livros.Domain.Exceptions;
using Livros.Domain.Repositories;
using MediatR;

namespace Livros.Application.Usuarios.Commands.AtualizarUsuario;

public record AtualizarUsuarioCommand : IRequest<UsuarioDto>
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

public class AtualizarUsuarioCommandValidator : AbstractValidator<AtualizarUsuarioCommand>
{
    public AtualizarUsuarioCommandValidator()
    {
        RuleFor(u => u.Id).GreaterThan(0).WithMessage("O id do usuário é obrigatório.");

        RuleFor(u => u.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MinimumLength(3).WithMessage("O nome deve ter ao menos 3 caracteres.")
            .MaximumLength(200).WithMessage("O nome deve ter no máximo 200 caracteres.");

        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("O e-mail informado não é válido.");
    }
}

public class AtualizarUsuarioCommandHandler : IRequestHandler<AtualizarUsuarioCommand, UsuarioDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public AtualizarUsuarioCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<UsuarioDto> Handle(
        AtualizarUsuarioCommand request,
        CancellationToken cancellationToken)
    {
        var usuario = await _unitOfWork.Usuarios
            .ObterPorIdComAvaliacoesAsync(request.Id, cancellationToken)
            ?? throw NaoEncontradoException.Para("Usuário", request.Id);

        var email = request.Email.Trim().ToLowerInvariant();

        if (await _unitOfWork.Usuarios.ExisteEmailAsync(email, request.Id, cancellationToken))
            throw new RegraNegocioException($"Já existe outro usuário com o e-mail '{email}'.");

        usuario.Atualizar(request.Nome, email);
        await _unitOfWork.CommitAsync(cancellationToken);

        return UsuarioDto.De(usuario);
    }
}
