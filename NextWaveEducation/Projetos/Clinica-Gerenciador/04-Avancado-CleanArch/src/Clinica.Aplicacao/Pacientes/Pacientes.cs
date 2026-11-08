using Clinica.Aplicacao.Dtos;
using Clinica.Dominio.Entidades;
using Clinica.Dominio.Enums;
using Clinica.Dominio.Excecoes;
using Clinica.Dominio.ObjetosDeValor;
using Clinica.Dominio.Repositorios;
using Clinica.Dominio.Servicos;
using FluentValidation;
using MediatR;

namespace Clinica.Aplicacao.Pacientes;

// ================================= Comandos =================================

/// <summary>
/// No CQRS, comando é intenção de mudar estado; query é intenção de ler.
/// Cada um é um tipo com nome próprio, o que torna óbvio no log e no
/// pipeline o que o sistema está fazendo.
/// </summary>
public record CadastrarPaciente(
    string NomeCompleto,
    string Cpf,
    string Celular,
    string? Email,
    DateTime DataNascimento,
    Sexo Sexo,
    string? Observacoes) : IRequest<PacienteDto>;

public record AtualizarPaciente(
    int Id,
    string NomeCompleto,
    string Cpf,
    string Celular,
    string? Email,
    DateTime DataNascimento,
    Sexo Sexo,
    string? Observacoes) : IRequest<PacienteDto>;

public record RemoverPaciente(int Id) : IRequest<Unit>;

// ================================= Consultas =================================

public record PesquisarPacientes(string? Termo) : IRequest<List<PacienteDto>>;

public record ObterFichaDoPaciente(int Id) : IRequest<FichaPacienteDto>;

public record BuscarPacientePorCpf(string Cpf) : IRequest<PacienteDto?>;

public record BuscarPacientesPorCelular(string Celular) : IRequest<List<PacienteDto>>;

// ================================ Validadores ================================

public class CadastrarPacienteValidator : AbstractValidator<CadastrarPaciente>
{
    public CadastrarPacienteValidator()
    {
        RuleFor(c => c.NomeCompleto)
            .NotEmpty().WithMessage("O nome completo é obrigatório.")
            .MinimumLength(3).WithMessage("O nome completo deve ter ao menos 3 caracteres.")
            .MaximumLength(200).WithMessage("O nome completo deve ter no máximo 200 caracteres.");

        RuleFor(c => c.Cpf)
            .NotEmpty().WithMessage("O CPF é obrigatório.")
            .Must(cpf => Cpf.EhValido(cpf))
            .WithMessage("O CPF é inválido (confira os dígitos verificadores).");

        RuleFor(c => c.Celular)
            .NotEmpty().WithMessage("O celular é obrigatório.")
            .Must(celular => Documentos.CelularValido(celular))
            .WithMessage("O celular é inválido. Informe DDD + 9 dígitos, ex: (11) 98765-4321.");

        RuleFor(c => c.Email)
            .Must(email => Email.EhValido(email))
            .When(c => !string.IsNullOrWhiteSpace(c.Email))
            .WithMessage("O e-mail é inválido.");

        RuleFor(c => c.DataNascimento)
            .NotEmpty().WithMessage("A data de nascimento é obrigatória.")
            .LessThanOrEqualTo(_ => DateTime.Today)
            .WithMessage("A data de nascimento não pode estar no futuro.")
            .GreaterThan(new DateTime(1900, 1, 1))
            .WithMessage("A data de nascimento informada é inválida.");

        RuleFor(c => c.Sexo).IsInEnum().WithMessage("Sexo inválido.");

        RuleFor(c => c.Observacoes)
            .MaximumLength(1000).WithMessage("As observações devem ter no máximo 1000 caracteres.");
    }
}

public class AtualizarPacienteValidator : AbstractValidator<AtualizarPaciente>
{
    public AtualizarPacienteValidator()
    {
        RuleFor(c => c.Id).GreaterThan(0).WithMessage("Informe o paciente.");

        RuleFor(c => c.NomeCompleto)
            .NotEmpty().WithMessage("O nome completo é obrigatório.")
            .MinimumLength(3).WithMessage("O nome completo deve ter ao menos 3 caracteres.")
            .MaximumLength(200).WithMessage("O nome completo deve ter no máximo 200 caracteres.");

        RuleFor(c => c.Cpf)
            .Must(cpf => Cpf.EhValido(cpf))
            .WithMessage("O CPF é inválido (confira os dígitos verificadores).");

        RuleFor(c => c.Celular)
            .Must(celular => Documentos.CelularValido(celular))
            .WithMessage("O celular é inválido.");

        RuleFor(c => c.Email)
            .Must(email => Email.EhValido(email))
            .When(c => !string.IsNullOrWhiteSpace(c.Email))
            .WithMessage("O e-mail é inválido.");

        RuleFor(c => c.DataNascimento)
            .LessThanOrEqualTo(_ => DateTime.Today)
            .WithMessage("A data de nascimento não pode estar no futuro.");

        RuleFor(c => c.Sexo).IsInEnum().WithMessage("Sexo inválido.");
    }
}

// ================================= Handlers =================================

public class CadastrarPacienteHandler : IRequestHandler<CadastrarPaciente, PacienteDto>
{
    private readonly IPacienteRepositorio _repositorio;
    private readonly IUnitOfWork _unitOfWork;

    public CadastrarPacienteHandler(IPacienteRepositorio repositorio, IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _unitOfWork = unitOfWork;
    }

    public async Task<PacienteDto> Handle(
        CadastrarPaciente comando, CancellationToken cancellationToken)
    {
        var cpf = Cpf.Criar(comando.Cpf);

        // Regra que depende do banco: o CPF identifica o paciente.
        if (await _repositorio.ExisteCpfAsync(cpf, null, cancellationToken))
            throw new RegraNegocioException(
                $"Já existe um paciente cadastrado com o CPF {cpf.Formatado}.");

        var paciente = new Paciente(
            comando.NomeCompleto,
            cpf,
            Celular.Criar(comando.Celular),
            comando.DataNascimento,
            comando.Sexo,
            Email.CriarOpcional(comando.Email),
            comando.Observacoes);

        _repositorio.Adicionar(paciente);
        await _unitOfWork.CommitAsync(cancellationToken);

        return PacienteDto.De(paciente);
    }
}

public class AtualizarPacienteHandler : IRequestHandler<AtualizarPaciente, PacienteDto>
{
    private readonly IPacienteRepositorio _repositorio;
    private readonly IUnitOfWork _unitOfWork;

    public AtualizarPacienteHandler(IPacienteRepositorio repositorio, IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _unitOfWork = unitOfWork;
    }

    public async Task<PacienteDto> Handle(
        AtualizarPaciente comando, CancellationToken cancellationToken)
    {
        var paciente = await _repositorio.ObterPorIdAsync(comando.Id, cancellationToken)
                       ?? throw new RegraNegocioException(
                           $"Paciente {comando.Id} não encontrado.");

        var cpf = Cpf.Criar(comando.Cpf);

        if (await _repositorio.ExisteCpfAsync(cpf, comando.Id, cancellationToken))
            throw new RegraNegocioException(
                $"Já existe outro paciente com o CPF {cpf.Formatado}.");

        paciente.AlterarDados(
            comando.NomeCompleto,
            cpf,
            Celular.Criar(comando.Celular),
            comando.DataNascimento,
            comando.Sexo,
            Email.CriarOpcional(comando.Email),
            comando.Observacoes);

        await _unitOfWork.CommitAsync(cancellationToken);

        return PacienteDto.De(paciente);
    }
}

public class RemoverPacienteHandler : IRequestHandler<RemoverPaciente, Unit>
{
    private readonly IPacienteRepositorio _repositorio;
    private readonly IUnitOfWork _unitOfWork;

    public RemoverPacienteHandler(IPacienteRepositorio repositorio, IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RemoverPaciente comando, CancellationToken cancellationToken)
    {
        var paciente = await _repositorio.ObterPorIdAsync(comando.Id, cancellationToken)
                       ?? throw new RegraNegocioException(
                           $"Paciente {comando.Id} não encontrado.");

        if (await _repositorio.PossuiAtendimentoAbertoAsync(comando.Id, cancellationToken))
            throw new RegraNegocioException(
                $"'{paciente.NomeCompleto}' tem atendimento em aberto. " +
                "Cancele antes de remover o paciente.");

        // Remoção lógica: o histórico clínico é preservado.
        paciente.Deletar();
        await _unitOfWork.CommitAsync(cancellationToken);

        return Unit.Value;
    }
}

public class PesquisarPacientesHandler : IRequestHandler<PesquisarPacientes, List<PacienteDto>>
{
    private readonly IPacienteRepositorio _repositorio;

    public PesquisarPacientesHandler(IPacienteRepositorio repositorio)
        => _repositorio = repositorio;

    public async Task<List<PacienteDto>> Handle(
        PesquisarPacientes consulta, CancellationToken cancellationToken)
    {
        var pacientes = await _repositorio.PesquisarAsync(consulta.Termo, cancellationToken);
        return pacientes.Select(PacienteDto.De).ToList();
    }
}

public class ObterFichaDoPacienteHandler
    : IRequestHandler<ObterFichaDoPaciente, FichaPacienteDto>
{
    private readonly IPacienteRepositorio _repositorio;

    public ObterFichaDoPacienteHandler(IPacienteRepositorio repositorio)
        => _repositorio = repositorio;

    public async Task<FichaPacienteDto> Handle(
        ObterFichaDoPaciente consulta, CancellationToken cancellationToken)
    {
        var paciente = await _repositorio.ObterFichaAsync(consulta.Id, cancellationToken)
                       ?? throw new RegraNegocioException(
                           $"Paciente {consulta.Id} não encontrado.");

        return FichaPacienteDto.De(paciente);
    }
}

public class BuscarPacientePorCpfHandler : IRequestHandler<BuscarPacientePorCpf, PacienteDto?>
{
    private readonly IPacienteRepositorio _repositorio;

    public BuscarPacientePorCpfHandler(IPacienteRepositorio repositorio)
        => _repositorio = repositorio;

    public async Task<PacienteDto?> Handle(
        BuscarPacientePorCpf consulta, CancellationToken cancellationToken)
    {
        // CPF malformado não é erro de servidor: simplesmente não existe
        // paciente com ele.
        if (!Cpf.EhValido(consulta.Cpf)) return null;

        var paciente = await _repositorio.ObterPorCpfAsync(
            Cpf.Criar(consulta.Cpf), cancellationToken);

        return paciente is null ? null : PacienteDto.De(paciente);
    }
}

public class BuscarPacientesPorCelularHandler
    : IRequestHandler<BuscarPacientesPorCelular, List<PacienteDto>>
{
    private readonly IPacienteRepositorio _repositorio;

    public BuscarPacientesPorCelularHandler(IPacienteRepositorio repositorio)
        => _repositorio = repositorio;

    public async Task<List<PacienteDto>> Handle(
        BuscarPacientesPorCelular consulta, CancellationToken cancellationToken)
    {
        var pacientes = await _repositorio.ListarPorCelularAsync(
            Celular.Criar(consulta.Celular), cancellationToken);

        return pacientes.Select(PacienteDto.De).ToList();
    }
}
