using FluentValidation;
using Livros.Domain.Entities;
using Livros.Domain.ValueObjects;

namespace Livros.Application.Livros.Commands.CadastrarLivro;

public class CadastrarLivroCommandValidator : AbstractValidator<CadastrarLivroCommand>
{
    public CadastrarLivroCommandValidator()
    {
        RuleFor(l => l.Titulo)
            .NotEmpty().WithMessage("O título é obrigatório.")
            .MaximumLength(250).WithMessage("O título deve ter no máximo 250 caracteres.");

        RuleFor(l => l.Descricao)
            .MaximumLength(4000).WithMessage("A descrição deve ter no máximo 4000 caracteres.");

        RuleFor(l => l.Isbn)
            .NotEmpty().WithMessage("O ISBN é obrigatório.")
            .Must(Isbn.EhValido).WithMessage("O ISBN deve ter 10 ou 13 dígitos (hífens são aceitos).");

        RuleFor(l => l.Autor)
            .NotEmpty().WithMessage("O autor é obrigatório.")
            .MaximumLength(200).WithMessage("O autor deve ter no máximo 200 caracteres.");

        RuleFor(l => l.Editora)
            .MaximumLength(150).WithMessage("A editora deve ter no máximo 150 caracteres.");

        RuleFor(l => l.Genero)
            .IsInEnum().WithMessage("O gênero informado é inválido.");

        RuleFor(l => l.AnoDePublicacao)
            .InclusiveBetween(Livro.AnoMinimoPublicacao, DateTime.Today.Year + 1)
            .WithMessage($"O ano de publicação deve estar entre {Livro.AnoMinimoPublicacao} e {DateTime.Today.Year + 1}.");

        RuleFor(l => l.QuantidadePaginas)
            .GreaterThan(0).WithMessage("A quantidade de páginas deve ser maior que zero.")
            .LessThanOrEqualTo(50_000).WithMessage("A quantidade de páginas informada é inválida.");
    }
}
