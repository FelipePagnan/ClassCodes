using FluentValidation;
using Livros.Application.Common;
using Livros.Domain.Exceptions;
using Livros.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Livros.Application.Usuarios.Commands.CadastrarUsuario;

public record CadastrarUsuarioCommand : IRequest<UsuarioDto>
{
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

public class CadastrarUsuarioCommandValidator : AbstractValidator<CadastrarUsuarioCommand>
{
    public CadastrarUsuarioCommandValidator()
    {
        RuleFor(u => u.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MinimumLength(3).WithMessage("O nome deve ter ao menos 3 caracteres.")
            .MaximumLength(200).WithMessage("O nome deve ter no máximo 200 caracteres.");

        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("O e-mail informado não é válido.")
            .MaximumLength(150).WithMessage("O e-mail deve ter no máximo 150 caracteres.");
    }
}

public class CadastrarUsuarioCommandHandler : IRequestHandler<CadastrarUsuarioCommand, UsuarioDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CadastrarUsuarioCommandHandler> _logger;

    public CadastrarUsuarioCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CadastrarUsuarioCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UsuarioDto> Handle(
        CadastrarUsuarioCommand request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _unitOfWork.Usuarios.ExisteEmailAsync(email, ct: cancellationToken))
            throw new RegraNegocioException($"Já existe um usuário cadastrado com o e-mail '{email}'.");

        var usuario = new Domain.Entities.Usuario(request.Nome, email);

        await _unitOfWork.Usuarios.AdicionarAsync(usuario, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Usuário {UsuarioId} '{Nome}' cadastrado.", usuario.Id, usuario.Nome);

        return UsuarioDto.De(usuario);
    }
}
