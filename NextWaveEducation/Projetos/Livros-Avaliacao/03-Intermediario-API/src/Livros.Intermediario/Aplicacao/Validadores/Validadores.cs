using FluentValidation;
using Livros.Intermediario.Aplicacao.InputModels;
using Livros.Intermediario.Dominio.Entidades;

namespace Livros.Intermediario.Aplicacao.Validadores;

public class CriarLivroInputModelValidator : AbstractValidator<CriarLivroInputModel>
{
    public CriarLivroInputModelValidator()
    {
        RuleFor(l => l.Titulo)
            .NotEmpty().WithMessage("O título é obrigatório.")
            .MaximumLength(250).WithMessage("O título deve ter no máximo 250 caracteres.");

        RuleFor(l => l.Descricao)
            .MaximumLength(4000).WithMessage("A descrição deve ter no máximo 4000 caracteres.");

        RuleFor(l => l.ISBN)
            .NotEmpty().WithMessage("O ISBN é obrigatório.")
            .Must(Livro.IsbnValido).WithMessage("O ISBN deve ter 10 ou 13 dígitos (hífens são aceitos).");

        RuleFor(l => l.Autor)
            .NotEmpty().WithMessage("O autor é obrigatório.")
            .MaximumLength(200).WithMessage("O autor deve ter no máximo 200 caracteres.");

        RuleFor(l => l.Editora)
            .MaximumLength(150).WithMessage("A editora deve ter no máximo 150 caracteres.");

        RuleFor(l => l.Genero)
            .IsInEnum().WithMessage("O gênero informado é inválido.");

        RuleFor(l => l.AnoDePublicacao)
            .InclusiveBetween(1450, DateTime.Today.Year + 1)
            .WithMessage($"O ano de publicação deve estar entre 1450 e {DateTime.Today.Year + 1}.");

        RuleFor(l => l.QuantidadePaginas)
            .GreaterThan(0).WithMessage("A quantidade de páginas deve ser maior que zero.")
            .LessThanOrEqualTo(50_000).WithMessage("A quantidade de páginas informada é inválida.");
    }
}

public class AtualizarLivroInputModelValidator : AbstractValidator<AtualizarLivroInputModel>
{
    public AtualizarLivroInputModelValidator()
    {
        RuleFor(l => l.Titulo)
            .NotEmpty().WithMessage("O título é obrigatório.")
            .MaximumLength(250).WithMessage("O título deve ter no máximo 250 caracteres.");

        RuleFor(l => l.ISBN)
            .NotEmpty().WithMessage("O ISBN é obrigatório.")
            .Must(Livro.IsbnValido).WithMessage("O ISBN deve ter 10 ou 13 dígitos (hífens são aceitos).");

        RuleFor(l => l.Autor)
            .NotEmpty().WithMessage("O autor é obrigatório.")
            .MaximumLength(200).WithMessage("O autor deve ter no máximo 200 caracteres.");

        RuleFor(l => l.Genero)
            .IsInEnum().WithMessage("O gênero informado é inválido.");

        RuleFor(l => l.AnoDePublicacao)
            .InclusiveBetween(1450, DateTime.Today.Year + 1)
            .WithMessage($"O ano de publicação deve estar entre 1450 e {DateTime.Today.Year + 1}.");

        RuleFor(l => l.QuantidadePaginas)
            .GreaterThan(0).WithMessage("A quantidade de páginas deve ser maior que zero.");
    }
}

public class CriarUsuarioInputModelValidator : AbstractValidator<CriarUsuarioInputModel>
{
    public CriarUsuarioInputModelValidator()
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

public class AtualizarUsuarioInputModelValidator : AbstractValidator<AtualizarUsuarioInputModel>
{
    public AtualizarUsuarioInputModelValidator()
    {
        RuleFor(u => u.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MinimumLength(3).WithMessage("O nome deve ter ao menos 3 caracteres.")
            .MaximumLength(200).WithMessage("O nome deve ter no máximo 200 caracteres.");

        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("O e-mail informado não é válido.");
    }
}

public class CriarAvaliacaoInputModelValidator : AbstractValidator<CriarAvaliacaoInputModel>
{
    public CriarAvaliacaoInputModelValidator()
    {
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
            .NotEmpty().WithMessage("A data de fim da leitura é obrigatória.")
            .LessThanOrEqualTo(_ => DateTime.Today.AddDays(1))
            .WithMessage("A data de fim da leitura não pode estar no futuro.");

        // Regra de negócio: a data de início não pode ser maior que a de fim.
        RuleFor(a => a)
            .Must(a => a.DataInicioLeitura.Date <= a.DataFimLeitura.Date)
            .WithMessage("A data de início da leitura não pode ser maior que a data de fim.")
            .OverridePropertyName(nameof(CriarAvaliacaoInputModel.DataInicioLeitura));
    }
}

public class ImportarLivroInputModelValidator : AbstractValidator<ImportarLivroInputModel>
{
    public ImportarLivroInputModelValidator()
    {
        RuleFor(i => i.Isbn)
            .NotEmpty().WithMessage("O ISBN é obrigatório.")
            .Must(Livro.IsbnValido).WithMessage("O ISBN deve ter 10 ou 13 dígitos.");

        When(i => i.Genero.HasValue, () =>
        {
            RuleFor(i => i.Genero!.Value)
                .IsInEnum().WithMessage("O gênero informado é inválido.");
        });
    }
}
