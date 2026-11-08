using Clinica.Aplicacao.Contratos;
using Clinica.Aplicacao.Dtos;
using Clinica.Dominio.Entidades;
using Clinica.Dominio.Enums;
using Clinica.Dominio.Excecoes;
using Clinica.Dominio.ObjetosDeValor;
using Clinica.Dominio.Repositorios;
using Clinica.Dominio.Servicos;
using FluentValidation;
using MediatR;

namespace Clinica.Aplicacao.Usuarios;

// ================================= Comandos =================================

public record AutenticarUsuario(string Email, string Senha) : IRequest<LoginDto>;

public record CadastrarUsuario(
    string Nome,
    string Email,
    string Senha,
    PerfilUsuario Perfil,
    int? MedicoId) : IRequest<UsuarioDto>;

public record AlterarSenha(int UsuarioId, string SenhaAtual, string SenhaNova) : IRequest<Unit>;

public record DesativarUsuario(int Id) : IRequest<Unit>;

public record ReativarUsuario(int Id) : IRequest<Unit>;

// ================================= Consultas =================================

public record ListarUsuarios : IRequest<List<UsuarioDto>>;

public record ObterUsuario(int Id) : IRequest<UsuarioDto>;

// ================================ Validadores ================================

public class AutenticarUsuarioValidator : AbstractValidator<AutenticarUsuario>
{
    public AutenticarUsuarioValidator()
    {
        RuleFor(c => c.Email).NotEmpty().WithMessage("Informe o e-mail.");
        RuleFor(c => c.Senha).NotEmpty().WithMessage("Informe a senha.");
    }
}

public class CadastrarUsuarioValidator : AbstractValidator<CadastrarUsuario>
{
    public CadastrarUsuarioValidator()
    {
        RuleFor(c => c.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MinimumLength(3).WithMessage("O nome deve ter ao menos 3 caracteres.")
            .MaximumLength(200).WithMessage("O nome deve ter no máximo 200 caracteres.");

        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .Must(email => Email.EhValido(email)).WithMessage("O e-mail é inválido.");

        RuleFor(c => c.Senha)
            .Must(senha => Senhas.SenhaForte(senha, out _))
            .WithMessage("A senha deve ter ao menos 8 caracteres, com letra e número.");

        RuleFor(c => c.Perfil)
            .IsInEnum()
            .WithMessage("O perfil deve ser Administrador, Medico ou Recepcionista.");

        // Perfil Médico precisa de vínculo; os outros não aceitam.
        RuleFor(c => c.MedicoId)
            .NotNull()
            .When(c => c.Perfil == PerfilUsuario.Medico)
            .WithMessage("Um usuário com perfil Médico precisa do Id do médico vinculado.");

        RuleFor(c => c.MedicoId)
            .Null()
            .When(c => c.Perfil != PerfilUsuario.Medico)
            .WithMessage("Só o perfil Médico aceita vínculo com cadastro de médico.");
    }
}

public class AlterarSenhaValidator : AbstractValidator<AlterarSenha>
{
    public AlterarSenhaValidator()
    {
        RuleFor(c => c.SenhaAtual).NotEmpty().WithMessage("Informe a senha atual.");

        RuleFor(c => c.SenhaNova)
            .Must(senha => Senhas.SenhaForte(senha, out _))
            .WithMessage("A nova senha deve ter ao menos 8 caracteres, com letra e número.");
    }
}

// ================================= Handlers =================================

public class AutenticarUsuarioHandler : IRequestHandler<AutenticarUsuario, LoginDto>
{
    private readonly IUsuarioRepositorio _repositorio;
    private readonly IGeradorDeToken _geradorDeToken;
    private readonly IUnitOfWork _unitOfWork;

    public AutenticarUsuarioHandler(
        IUsuarioRepositorio repositorio,
        IGeradorDeToken geradorDeToken,
        IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _geradorDeToken = geradorDeToken;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoginDto> Handle(
        AutenticarUsuario comando, CancellationToken cancellationToken)
    {
        // Mensagem genérica de propósito: dizer "e-mail não existe" entrega
        // ao atacante quais contas são válidas.
        const string erro = "E-mail ou senha inválidos.";

        if (!Email.EhValido(comando.Email)) throw new RegraNegocioException(erro);

        var usuario = await _repositorio.ObterPorEmailAsync(
            Email.Criar(comando.Email), cancellationToken);

        if (usuario is null || !usuario.ConferirSenha(comando.Senha))
            throw new RegraNegocioException(erro);

        if (!usuario.Ativo)
            throw new RegraNegocioException("Este usuário está inativo. Procure o administrador.");

        usuario.RegistrarAcesso();
        await _unitOfWork.CommitAsync(cancellationToken);

        var (token, expiraEm) = _geradorDeToken.Gerar(usuario);

        return new LoginDto(token, expiraEm, UsuarioDto.De(usuario));
    }
}

public class CadastrarUsuarioHandler : IRequestHandler<CadastrarUsuario, UsuarioDto>
{
    private readonly IUsuarioRepositorio _repositorio;
    private readonly IMedicoRepositorio _medicoRepositorio;
    private readonly IUnitOfWork _unitOfWork;

    public CadastrarUsuarioHandler(
        IUsuarioRepositorio repositorio,
        IMedicoRepositorio medicoRepositorio,
        IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _medicoRepositorio = medicoRepositorio;
        _unitOfWork = unitOfWork;
    }

    public async Task<UsuarioDto> Handle(
        CadastrarUsuario comando, CancellationToken cancellationToken)
    {
        var email = Email.Criar(comando.Email);

        if (await _repositorio.ExisteEmailAsync(email, cancellationToken))
            throw new RegraNegocioException($"Já existe um usuário com o e-mail {email.Valor}.");

        Medico? medico = null;

        if (comando.Perfil == PerfilUsuario.Medico)
        {
            medico = await _medicoRepositorio.ObterPorIdAsync(
                         comando.MedicoId!.Value, cancellationToken)
                     ?? throw new RegraNegocioException(
                         $"Médico {comando.MedicoId} não encontrado.");

            if (await _repositorio.ExisteUsuarioDoMedicoAsync(medico.Id, cancellationToken))
                throw new RegraNegocioException(
                    $"O médico {medico.NomeCompleto} já possui usuário no sistema.");
        }

        var usuario = new Usuario(comando.Nome, email, comando.Senha, comando.Perfil, medico);

        _repositorio.Adicionar(usuario);
        await _unitOfWork.CommitAsync(cancellationToken);

        return UsuarioDto.De(usuario);
    }
}

public class AlterarSenhaHandler : IRequestHandler<AlterarSenha, Unit>
{
    private readonly IUsuarioRepositorio _repositorio;
    private readonly IUnitOfWork _unitOfWork;

    public AlterarSenhaHandler(IUsuarioRepositorio repositorio, IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(AlterarSenha comando, CancellationToken cancellationToken)
    {
        var usuario = await _repositorio.ObterPorIdAsync(comando.UsuarioId, cancellationToken)
                      ?? throw new RegraNegocioException(
                          $"Usuário {comando.UsuarioId} não encontrado.");

        usuario.AlterarSenha(comando.SenhaAtual, comando.SenhaNova);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Unit.Value;
    }
}

public class DesativarUsuarioHandler : IRequestHandler<DesativarUsuario, Unit>
{
    private readonly IUsuarioRepositorio _repositorio;
    private readonly IUnitOfWork _unitOfWork;

    public DesativarUsuarioHandler(IUsuarioRepositorio repositorio, IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DesativarUsuario comando, CancellationToken cancellationToken)
    {
        var usuario = await _repositorio.ObterPorIdAsync(comando.Id, cancellationToken)
                      ?? throw new RegraNegocioException($"Usuário {comando.Id} não encontrado.");

        // Desativar o último administrador deixaria o sistema sem ninguém
        // capaz de reativá-lo.
        if (usuario.Perfil == PerfilUsuario.Administrador
            && await _repositorio.ContarAdministradoresAtivosAsync(cancellationToken) == 1)
            throw new RegraNegocioException(
                "Não é possível desativar o último administrador ativo do sistema.");

        usuario.Desativar();
        await _unitOfWork.CommitAsync(cancellationToken);

        return Unit.Value;
    }
}

public class ReativarUsuarioHandler : IRequestHandler<ReativarUsuario, Unit>
{
    private readonly IUsuarioRepositorio _repositorio;
    private readonly IUnitOfWork _unitOfWork;

    public ReativarUsuarioHandler(IUsuarioRepositorio repositorio, IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(ReativarUsuario comando, CancellationToken cancellationToken)
    {
        var usuario = await _repositorio.ObterPorIdAsync(comando.Id, cancellationToken)
                      ?? throw new RegraNegocioException($"Usuário {comando.Id} não encontrado.");

        usuario.Reativar();
        await _unitOfWork.CommitAsync(cancellationToken);

        return Unit.Value;
    }
}

public class ListarUsuariosHandler : IRequestHandler<ListarUsuarios, List<UsuarioDto>>
{
    private readonly IUsuarioRepositorio _repositorio;

    public ListarUsuariosHandler(IUsuarioRepositorio repositorio) => _repositorio = repositorio;

    public async Task<List<UsuarioDto>> Handle(
        ListarUsuarios consulta, CancellationToken cancellationToken)
    {
        var usuarios = await _repositorio.ListarAsync(cancellationToken);
        return usuarios.Select(UsuarioDto.De).ToList();
    }
}

public class ObterUsuarioHandler : IRequestHandler<ObterUsuario, UsuarioDto>
{
    private readonly IUsuarioRepositorio _repositorio;

    public ObterUsuarioHandler(IUsuarioRepositorio repositorio) => _repositorio = repositorio;

    public async Task<UsuarioDto> Handle(
        ObterUsuario consulta, CancellationToken cancellationToken)
        => UsuarioDto.De(
            await _repositorio.ObterPorIdAsync(consulta.Id, cancellationToken)
            ?? throw new RegraNegocioException($"Usuário {consulta.Id} não encontrado."));
}
