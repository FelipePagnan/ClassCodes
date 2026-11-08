using Clinica.Aplicacao.Dtos;
using Clinica.Dominio.Entidades;
using Clinica.Dominio.Excecoes;
using Clinica.Dominio.ObjetosDeValor;
using Clinica.Dominio.Repositorios;
using Clinica.Dominio.Servicos;
using FluentValidation;
using MediatR;

namespace Clinica.Aplicacao.Medicos;

// ================================= Comandos =================================

public record CadastrarMedico(
    string NomeCompleto,
    string Crm,
    string UfCrm,
    string Especialidade,
    string? Email,
    string? Celular) : IRequest<MedicoDto>;

public record AtualizarMedico(
    int Id,
    string NomeCompleto,
    string Crm,
    string UfCrm,
    string Especialidade,
    string? Email,
    string? Celular) : IRequest<MedicoDto>;

public record RemoverMedico(int Id) : IRequest<Unit>;

// ================================= Consultas =================================

public record ListarMedicos(string? Especialidade) : IRequest<List<MedicoDto>>;

public record ObterMedico(int Id) : IRequest<MedicoDto>;

public record BuscarMedicoPorCrm(string Crm, string Uf) : IRequest<MedicoDto?>;

// ================================ Validadores ================================

public class CadastrarMedicoValidator : AbstractValidator<CadastrarMedico>
{
    public CadastrarMedicoValidator()
    {
        RuleFor(c => c.NomeCompleto)
            .NotEmpty().WithMessage("O nome completo é obrigatório.")
            .MinimumLength(3).WithMessage("O nome completo deve ter ao menos 3 caracteres.")
            .MaximumLength(200).WithMessage("O nome completo deve ter no máximo 200 caracteres.");

        RuleFor(c => c.Crm)
            .NotEmpty().WithMessage("O CRM é obrigatório.")
            .Must(crm => Documentos.CrmValido(crm))
            .WithMessage("O CRM é inválido. Informe de 4 a 7 dígitos.");

        RuleFor(c => c.UfCrm)
            .NotEmpty().WithMessage("A UF do CRM é obrigatória.")
            .Must(uf => Documentos.UfValida(uf))
            .WithMessage("A UF é inválida. Use a sigla do estado, ex: SP.");

        RuleFor(c => c.Especialidade)
            .NotEmpty().WithMessage("A especialidade é obrigatória.")
            .MaximumLength(120).WithMessage("A especialidade deve ter no máximo 120 caracteres.");

        RuleFor(c => c.Email)
            .Must(email => Email.EhValido(email))
            .When(c => !string.IsNullOrWhiteSpace(c.Email))
            .WithMessage("O e-mail é inválido.");

        RuleFor(c => c.Celular)
            .Must(celular => Documentos.CelularValido(celular!))
            .When(c => !string.IsNullOrWhiteSpace(c.Celular))
            .WithMessage("O celular é inválido.");
    }
}

public class AtualizarMedicoValidator : AbstractValidator<AtualizarMedico>
{
    public AtualizarMedicoValidator()
    {
        RuleFor(c => c.Id).GreaterThan(0).WithMessage("Informe o médico.");

        RuleFor(c => c.NomeCompleto)
            .NotEmpty().WithMessage("O nome completo é obrigatório.")
            .MinimumLength(3).WithMessage("O nome completo deve ter ao menos 3 caracteres.");

        RuleFor(c => c.Crm)
            .Must(crm => Documentos.CrmValido(crm))
            .WithMessage("O CRM é inválido. Informe de 4 a 7 dígitos.");

        RuleFor(c => c.UfCrm)
            .Must(uf => Documentos.UfValida(uf))
            .WithMessage("A UF é inválida. Use a sigla do estado, ex: SP.");

        RuleFor(c => c.Especialidade)
            .NotEmpty().WithMessage("A especialidade é obrigatória.");

        RuleFor(c => c.Email)
            .Must(email => Email.EhValido(email))
            .When(c => !string.IsNullOrWhiteSpace(c.Email))
            .WithMessage("O e-mail é inválido.");
    }
}

// ================================= Handlers =================================

public class CadastrarMedicoHandler : IRequestHandler<CadastrarMedico, MedicoDto>
{
    private readonly IMedicoRepositorio _repositorio;
    private readonly IUnitOfWork _unitOfWork;

    public CadastrarMedicoHandler(IMedicoRepositorio repositorio, IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _unitOfWork = unitOfWork;
    }

    public async Task<MedicoDto> Handle(
        CadastrarMedico comando, CancellationToken cancellationToken)
    {
        var crm = Crm.Criar(comando.Crm, comando.UfCrm);

        // O par CRM + UF identifica o médico: o número só é único no estado.
        if (await _repositorio.ExisteCrmAsync(crm, null, cancellationToken))
            throw new RegraNegocioException($"Já existe um médico cadastrado com {crm.Completo}.");

        var medico = new Medico(
            comando.NomeCompleto,
            crm,
            comando.Especialidade,
            Email.CriarOpcional(comando.Email),
            Celular.CriarOpcional(comando.Celular));

        _repositorio.Adicionar(medico);
        await _unitOfWork.CommitAsync(cancellationToken);

        return MedicoDto.De(medico);
    }
}

public class AtualizarMedicoHandler : IRequestHandler<AtualizarMedico, MedicoDto>
{
    private readonly IMedicoRepositorio _repositorio;
    private readonly IUnitOfWork _unitOfWork;

    public AtualizarMedicoHandler(IMedicoRepositorio repositorio, IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _unitOfWork = unitOfWork;
    }

    public async Task<MedicoDto> Handle(
        AtualizarMedico comando, CancellationToken cancellationToken)
    {
        var medico = await _repositorio.ObterPorIdAsync(comando.Id, cancellationToken)
                     ?? throw new RegraNegocioException($"Médico {comando.Id} não encontrado.");

        var crm = Crm.Criar(comando.Crm, comando.UfCrm);

        if (await _repositorio.ExisteCrmAsync(crm, comando.Id, cancellationToken))
            throw new RegraNegocioException($"Já existe outro médico com {crm.Completo}.");

        medico.AlterarDados(
            comando.NomeCompleto,
            crm,
            comando.Especialidade,
            Email.CriarOpcional(comando.Email),
            Celular.CriarOpcional(comando.Celular));

        await _unitOfWork.CommitAsync(cancellationToken);

        return MedicoDto.De(medico);
    }
}

public class RemoverMedicoHandler : IRequestHandler<RemoverMedico, Unit>
{
    private readonly IMedicoRepositorio _repositorio;
    private readonly IUnitOfWork _unitOfWork;

    public RemoverMedicoHandler(IMedicoRepositorio repositorio, IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RemoverMedico comando, CancellationToken cancellationToken)
    {
        var medico = await _repositorio.ObterPorIdAsync(comando.Id, cancellationToken)
                     ?? throw new RegraNegocioException($"Médico {comando.Id} não encontrado.");

        if (await _repositorio.PossuiAtendimentoAbertoAsync(comando.Id, cancellationToken))
            throw new RegraNegocioException(
                $"Dr(a). {medico.NomeCompleto} tem atendimento em aberto. " +
                "Cancele ou remarque antes de remover o médico.");

        medico.Deletar();
        await _unitOfWork.CommitAsync(cancellationToken);

        return Unit.Value;
    }
}

public class ListarMedicosHandler : IRequestHandler<ListarMedicos, List<MedicoDto>>
{
    private readonly IMedicoRepositorio _repositorio;

    public ListarMedicosHandler(IMedicoRepositorio repositorio) => _repositorio = repositorio;

    public async Task<List<MedicoDto>> Handle(
        ListarMedicos consulta, CancellationToken cancellationToken)
    {
        var medicos = await _repositorio.ListarAsync(consulta.Especialidade, cancellationToken);
        return medicos.Select(MedicoDto.De).ToList();
    }
}

public class ObterMedicoHandler : IRequestHandler<ObterMedico, MedicoDto>
{
    private readonly IMedicoRepositorio _repositorio;

    public ObterMedicoHandler(IMedicoRepositorio repositorio) => _repositorio = repositorio;

    public async Task<MedicoDto> Handle(ObterMedico consulta, CancellationToken cancellationToken)
        => MedicoDto.De(
            await _repositorio.ObterPorIdAsync(consulta.Id, cancellationToken)
            ?? throw new RegraNegocioException($"Médico {consulta.Id} não encontrado."));
}

public class BuscarMedicoPorCrmHandler : IRequestHandler<BuscarMedicoPorCrm, MedicoDto?>
{
    private readonly IMedicoRepositorio _repositorio;

    public BuscarMedicoPorCrmHandler(IMedicoRepositorio repositorio) => _repositorio = repositorio;

    public async Task<MedicoDto?> Handle(
        BuscarMedicoPorCrm consulta, CancellationToken cancellationToken)
    {
        var medico = await _repositorio.ObterPorCrmAsync(
            Crm.Criar(consulta.Crm, consulta.Uf), cancellationToken);

        return medico is null ? null : MedicoDto.De(medico);
    }
}
