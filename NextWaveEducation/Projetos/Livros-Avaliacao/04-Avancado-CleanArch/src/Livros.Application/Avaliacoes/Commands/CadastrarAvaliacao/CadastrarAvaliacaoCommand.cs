using FluentValidation;
using Livros.Application.Common;
using Livros.Domain.Entities;
using Livros.Domain.Exceptions;
using Livros.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Livros.Application.Avaliacoes.Commands.CadastrarAvaliacao;

public record CadastrarAvaliacaoCommand : IRequest<ResultadoAvaliacaoDto>
{
    public int IdLivro { get; init; }
    public int IdUsuario { get; init; }
    public int Nota { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public DateTime DataInicioLeitura { get; init; }
    public DateTime DataFimLeitura { get; init; }
}

public class CadastrarAvaliacaoCommandValidator : AbstractValidator<CadastrarAvaliacaoCommand>
{
    public CadastrarAvaliacaoCommandValidator()
    {
        RuleFor(a => a.IdLivro)
            .GreaterThan(0).WithMessage("O id do livro é obrigatório.");

        RuleFor(a => a.IdUsuario)
            .GreaterThan(0).WithMessage("O id do usuário é obrigatório.");

        // Regra de negócio: a nota deve ser de 1 a 5.
        RuleFor(a => a.Nota)
            .InclusiveBetween(Avaliacao.NotaMinima, Avaliacao.NotaMaxima)
            .WithMessage($"A nota deve ser de {Avaliacao.NotaMinima} a {Avaliacao.NotaMaxima}.");

        RuleFor(a => a.Descricao)
            .MaximumLength(2000).WithMessage("O comentário deve ter no máximo 2000 caracteres.");

        RuleFor(a => a.DataInicioLeitura)
            .NotEmpty().WithMessage("A data de início da leitura é obrigatória.");

        RuleFor(a => a.DataFimLeitura)
            .NotEmpty().WithMessage("A data de fim da leitura é obrigatória.");

        // Regra de negócio: a data de início não pode ser maior que a de fim.
        RuleFor(a => a)
            .Must(a => a.DataInicioLeitura.Date <= a.DataFimLeitura.Date)
            .WithMessage("A data de início da leitura não pode ser maior que a data de fim.")
            .OverridePropertyName(nameof(CadastrarAvaliacaoCommand.DataInicioLeitura));
    }
}

/// <summary>
/// Cadastra a avaliação. O recálculo da nota média acontece no handler do
/// evento AvaliacaoCadastradaEvent, despachado no commit da Unit of Work.
/// </summary>
public class CadastrarAvaliacaoCommandHandler
    : IRequestHandler<CadastrarAvaliacaoCommand, ResultadoAvaliacaoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CadastrarAvaliacaoCommandHandler> _logger;

    public CadastrarAvaliacaoCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CadastrarAvaliacaoCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ResultadoAvaliacaoDto> Handle(
        CadastrarAvaliacaoCommand request,
        CancellationToken cancellationToken)
    {
        var livro = await _unitOfWork.Livros.ObterPorIdComAvaliacoesAsync(request.IdLivro, cancellationToken)
            ?? throw NaoEncontradoException.Para("Livro", request.IdLivro);

        var usuario = await _unitOfWork.Usuarios.ObterPorIdAsync(request.IdUsuario, cancellationToken)
            ?? throw NaoEncontradoException.Para("Usuário", request.IdUsuario);

        // A entidade aplica as regras de nota (1 a 5), de período de leitura
        // e de avaliação duplicada, e dispara o evento de domínio.
        var avaliacao = livro.AdicionarAvaliacao(
            usuario.Id,
            request.Nota,
            request.Descricao,
            request.DataInicioLeitura,
            request.DataFimLeitura);

        // O commit salva a avaliação e despacha AvaliacaoCadastradaEvent,
        // cujo handler recalcula e persiste a nota média do livro.
        await _unitOfWork.CommitAsync(cancellationToken);

        // Relê o livro para devolver a nota média já atualizada pelo handler.
        var livroAtualizado = await _unitOfWork.Livros
            .ObterPorIdComAvaliacoesAsync(request.IdLivro, cancellationToken) ?? livro;

        _logger.LogInformation(
            "Avaliação {AvaliacaoId} de nota {Nota} cadastrada para o livro {LivroId}. Média: {Media}",
            avaliacao.Id, avaliacao.Nota, livro.Id, livroAtualizado.NotaMedia);

        var dto = new AvaliacaoDto(
            avaliacao.Id,
            avaliacao.Nota,
            avaliacao.Descricao,
            avaliacao.IdUsuario,
            usuario.Nome,
            avaliacao.DataInicioLeitura,
            avaliacao.DataFimLeitura,
            avaliacao.DiasDeLeitura,
            avaliacao.DataCriacao);

        return new ResultadoAvaliacaoDto(
            dto,
            livroAtualizado.Titulo,
            livroAtualizado.NotaMedia,
            livroAtualizado.TotalAvaliacoes);
    }
}
