using BancoSangue.Intermediario.Aplicacao.InputModels;
using FluentValidation;

namespace BancoSangue.Intermediario.Aplicacao.Validadores;

public class CriarDoadorInputModelValidator : AbstractValidator<CriarDoadorInputModel>
{
    public CriarDoadorInputModelValidator()
    {
        RuleFor(d => d.NomeCompleto)
            .NotEmpty().WithMessage("O nome completo é obrigatório.")
            .MinimumLength(3).WithMessage("O nome completo deve ter ao menos 3 caracteres.")
            .MaximumLength(200).WithMessage("O nome completo deve ter no máximo 200 caracteres.");

        RuleFor(d => d.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("O e-mail informado não é válido.")
            .MaximumLength(150).WithMessage("O e-mail deve ter no máximo 150 caracteres.");

        RuleFor(d => d.DataNascimento)
            .NotEmpty().WithMessage("A data de nascimento é obrigatória.")
            .LessThan(DateTime.Today).WithMessage("A data de nascimento deve ser anterior à data de hoje.")
            .GreaterThan(DateTime.Today.AddYears(-130)).WithMessage("A data de nascimento informada é inválida.");

        RuleFor(d => d.Genero)
            .IsInEnum().WithMessage("O gênero informado é inválido.");

        RuleFor(d => d.Peso)
            .GreaterThan(0).WithMessage("O peso deve ser maior que zero.")
            .LessThanOrEqualTo(400).WithMessage("O peso informado é inválido.");

        RuleFor(d => d.TipoSanguineo)
            .IsInEnum().WithMessage("O tipo sanguíneo informado é inválido.");

        RuleFor(d => d.FatorRh)
            .IsInEnum().WithMessage("O fator Rh informado é inválido.");

        // O CEP é opcional, mas quando informado precisa ter 8 dígitos.
        When(d => !string.IsNullOrWhiteSpace(d.Cep), () =>
        {
            RuleFor(d => d.Cep!)
                .Must(TerOitoDigitos)
                .WithMessage("O CEP deve conter 8 dígitos (ex: 01001-000).");
        });
    }

    private static bool TerOitoDigitos(string cep)
        => cep.Count(char.IsDigit) == 8;
}
