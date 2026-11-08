using FluentValidation;
using Livros.Application.Common;
using Livros.Domain.Entities;
using Livros.Domain.Enums;
using Livros.Domain.Exceptions;
using Livros.Domain.Repositories;
using Livros.Domain.ValueObjects;
using MediatR;

namespace Livros.Application.Livros.Commands.AtualizarLivro;

public record AtualizarLivroCommand : IRequest<LivroDto>
{
    public int Id { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;
    public string Isbn { get; init; } = string.Empty;
    public string Autor { get; init; } = string.Empty;
    public string Editora { get; init; } = string.Empty;
    public GeneroLivroEnum Genero { get; init; }
    public int AnoDePublicacao { get; init; }
    public int QuantidadePaginas { get; init; }
}

public class AtualizarLivroCommandValidator : AbstractValidator<AtualizarLivroCommand>
{
    public AtualizarLivroCommandValidator()
    {
        RuleFor(l => l.Id).GreaterThan(0).WithMessage("O id do livro é obrigatório.");

        RuleFor(l => l.Titulo)
            .NotEmpty().WithMessage("O título é obrigatório.")
            .MaximumLength(250).WithMessage("O título deve ter no máximo 250 caracteres.");

        RuleFor(l => l.Isbn)
            .NotEmpty().WithMessage("O ISBN é obrigatório.")
            .Must(Isbn.EhValido).WithMessage("O ISBN deve ter 10 ou 13 dígitos (hífens são aceitos).");

        RuleFor(l => l.Autor)
            .NotEmpty().WithMessage("O autor é obrigatório.")
            .MaximumLength(200).WithMessage("O autor deve ter no máximo 200 caracteres.");

        RuleFor(l => l.Genero)
            .IsInEnum().WithMessage("O gênero informado é inválido.");

        RuleFor(l => l.AnoDePublicacao)
            .InclusiveBetween(Livro.AnoMinimoPublicacao, DateTime.Today.Year + 1)
            .WithMessage($"O ano de publicação deve estar entre {Livro.AnoMinimoPublicacao} e {DateTime.Today.Year + 1}.");

        RuleFor(l => l.QuantidadePaginas)
            .GreaterThan(0).WithMessage("A quantidade de páginas deve ser maior que zero.");
    }
}

public class AtualizarLivroCommandHandler : IRequestHandler<AtualizarLivroCommand, LivroDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public AtualizarLivroCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<LivroDto> Handle(AtualizarLivroCommand request, CancellationToken cancellationToken)
    {
        var livro = await _unitOfWork.Livros.ObterPorIdComAvaliacoesAsync(request.Id, cancellationToken)
            ?? throw NaoEncontradoException.Para("Livro", request.Id);

        var isbnNormalizado = Isbn.Criar(request.Isbn).Normalizado;

        // Regra de negócio: o ISBN não pode colidir com o de outro livro.
        if (await _unitOfWork.Livros.ExisteIsbnAsync(isbnNormalizado, request.Id, cancellationToken))
            throw new RegraNegocioException($"Já existe outro livro cadastrado com o ISBN '{request.Isbn}'.");

        livro.Atualizar(
            request.Titulo,
            request.Descricao,
            request.Isbn,
            request.Autor,
            request.Editora,
            request.Genero,
            request.AnoDePublicacao,
            request.QuantidadePaginas);

        await _unitOfWork.CommitAsync(cancellationToken);

        return LivroDto.De(livro);
    }
}
