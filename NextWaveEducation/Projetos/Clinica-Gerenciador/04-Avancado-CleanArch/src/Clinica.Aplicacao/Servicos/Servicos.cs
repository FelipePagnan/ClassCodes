using Clinica.Aplicacao.Dtos;
using Clinica.Dominio.Excecoes;
using Clinica.Dominio.Repositorios;
using FluentValidation;
using MediatR;
using Entidades = Clinica.Dominio.Entidades;

namespace Clinica.Aplicacao.Servicos;

// ================================= Comandos =================================

public record CadastrarServico(
    string Nome,
    string? Descricao,
    decimal Valor,
    int DuracaoMinutos) : IRequest<ServicoDto>;

public record AtualizarServico(
    int Id,
    string Nome,
    string? Descricao,
    decimal Valor,
    int DuracaoMinutos) : IRequest<ServicoDto>;

public record RemoverServico(int Id) : IRequest<Unit>;

// ================================= Consultas =================================

public record ListarServicos : IRequest<List<ServicoDto>>;

public record ObterServico(int Id) : IRequest<ServicoDto>;

// ================================ Validadores ================================

public class CadastrarServicoValidator : AbstractValidator<CadastrarServico>
{
    public CadastrarServicoValidator()
    {
        RuleFor(c => c.Nome)
            .NotEmpty().WithMessage("O nome do serviço é obrigatório.")
            .MinimumLength(3).WithMessage("O nome do serviço deve ter ao menos 3 caracteres.")
            .MaximumLength(150).WithMessage("O nome do serviço deve ter no máximo 150 caracteres.");

        RuleFor(c => c.Descricao)
            .MaximumLength(500).WithMessage("A descrição deve ter no máximo 500 caracteres.");

        RuleFor(c => c.Valor)
            .GreaterThanOrEqualTo(0).WithMessage("O valor não pode ser negativo.")
            // Round compara por valor, não por escala: 250.00 passa, 250.005 não.
            .Must(valor => decimal.Round(valor, 2) == valor)
            .WithMessage("O valor deve ter no máximo 2 casas decimais.");

        RuleFor(c => c.DuracaoMinutos)
            .GreaterThan(0).WithMessage("A duração deve ser maior que zero.")
            .LessThanOrEqualTo(Entidades.Servico.DuracaoMaximaMinutos)
            .WithMessage("A duração não pode passar de "
                         + $"{Entidades.Servico.DuracaoMaximaMinutos} minutos.");
    }
}

public class AtualizarServicoValidator : AbstractValidator<AtualizarServico>
{
    public AtualizarServicoValidator()
    {
        RuleFor(c => c.Id).GreaterThan(0).WithMessage("Informe o serviço.");

        RuleFor(c => c.Nome)
            .NotEmpty().WithMessage("O nome do serviço é obrigatório.")
            .MinimumLength(3).WithMessage("O nome do serviço deve ter ao menos 3 caracteres.")
            .MaximumLength(150).WithMessage("O nome do serviço deve ter no máximo 150 caracteres.");

        RuleFor(c => c.Valor)
            .GreaterThanOrEqualTo(0).WithMessage("O valor não pode ser negativo.")
            .Must(valor => decimal.Round(valor, 2) == valor)
            .WithMessage("O valor deve ter no máximo 2 casas decimais.");

        RuleFor(c => c.DuracaoMinutos)
            .GreaterThan(0).WithMessage("A duração deve ser maior que zero.")
            .LessThanOrEqualTo(Entidades.Servico.DuracaoMaximaMinutos)
            .WithMessage("A duração não pode passar de "
                         + $"{Entidades.Servico.DuracaoMaximaMinutos} minutos.");
    }
}

// ================================= Handlers =================================

public class CadastrarServicoHandler : IRequestHandler<CadastrarServico, ServicoDto>
{
    private readonly IServicoRepositorio _repositorio;
    private readonly IUnitOfWork _unitOfWork;

    public CadastrarServicoHandler(IServicoRepositorio repositorio, IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServicoDto> Handle(
        CadastrarServico comando, CancellationToken cancellationToken)
    {
        if (await _repositorio.ExisteNomeAsync(comando.Nome, null, cancellationToken))
            throw new RegraNegocioException(
                $"Já existe um serviço chamado '{comando.Nome.Trim()}'.");

        var servico = new Entidades.Servico(
            comando.Nome, comando.Valor, comando.DuracaoMinutos, comando.Descricao);

        _repositorio.Adicionar(servico);
        await _unitOfWork.CommitAsync(cancellationToken);

        return ServicoDto.De(servico);
    }
}

public class AtualizarServicoHandler : IRequestHandler<AtualizarServico, ServicoDto>
{
    private readonly IServicoRepositorio _repositorio;
    private readonly IUnitOfWork _unitOfWork;

    public AtualizarServicoHandler(IServicoRepositorio repositorio, IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServicoDto> Handle(
        AtualizarServico comando, CancellationToken cancellationToken)
    {
        var servico = await _repositorio.ObterPorIdAsync(comando.Id, cancellationToken)
                      ?? throw new RegraNegocioException($"Serviço {comando.Id} não encontrado.");

        if (await _repositorio.ExisteNomeAsync(comando.Nome, comando.Id, cancellationToken))
            throw new RegraNegocioException(
                $"Já existe outro serviço chamado '{comando.Nome.Trim()}'.");

        // Valor e duração novos valem só para agendamentos futuros: cada
        // atendimento guarda a própria cópia desses dois campos.
        servico.AlterarDados(
            comando.Nome, comando.Valor, comando.DuracaoMinutos, comando.Descricao);

        await _unitOfWork.CommitAsync(cancellationToken);

        return ServicoDto.De(servico);
    }
}

public class RemoverServicoHandler : IRequestHandler<RemoverServico, Unit>
{
    private readonly IServicoRepositorio _repositorio;
    private readonly IUnitOfWork _unitOfWork;

    public RemoverServicoHandler(IServicoRepositorio repositorio, IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RemoverServico comando, CancellationToken cancellationToken)
    {
        var servico = await _repositorio.ObterPorIdAsync(comando.Id, cancellationToken)
                      ?? throw new RegraNegocioException($"Serviço {comando.Id} não encontrado.");

        if (await _repositorio.PossuiAtendimentoAbertoAsync(comando.Id, cancellationToken))
            throw new RegraNegocioException(
                $"O serviço '{servico.Nome}' tem atendimento em aberto.");

        servico.Deletar();
        await _unitOfWork.CommitAsync(cancellationToken);

        return Unit.Value;
    }
}

public class ListarServicosHandler : IRequestHandler<ListarServicos, List<ServicoDto>>
{
    private readonly IServicoRepositorio _repositorio;

    public ListarServicosHandler(IServicoRepositorio repositorio) => _repositorio = repositorio;

    public async Task<List<ServicoDto>> Handle(
        ListarServicos consulta, CancellationToken cancellationToken)
    {
        var servicos = await _repositorio.ListarAsync(cancellationToken);
        return servicos.Select(ServicoDto.De).ToList();
    }
}

public class ObterServicoHandler : IRequestHandler<ObterServico, ServicoDto>
{
    private readonly IServicoRepositorio _repositorio;

    public ObterServicoHandler(IServicoRepositorio repositorio) => _repositorio = repositorio;

    public async Task<ServicoDto> Handle(
        ObterServico consulta, CancellationToken cancellationToken)
        => ServicoDto.De(
            await _repositorio.ObterPorIdAsync(consulta.Id, cancellationToken)
            ?? throw new RegraNegocioException($"Serviço {consulta.Id} não encontrado."));
}
