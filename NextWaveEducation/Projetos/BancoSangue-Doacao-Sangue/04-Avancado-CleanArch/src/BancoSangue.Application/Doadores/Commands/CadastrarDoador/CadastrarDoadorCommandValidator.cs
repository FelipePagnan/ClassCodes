using FluentValidation;

namespace BancoSangue.Application.Doadores.Commands.CadastrarDoador;

public class CadastrarDoadorCommandValidator : AbstractValidator<CadastrarDoadorCommand>
{
    public CadastrarDoadorCommandValidator()
    {
        RuleFor(x => x.NomeCompleto)
            .NotEmpty().WithMessage("O nome completo é obrigatório.")
            .MinimumLength(3).WithMessage("O nome completo deve ter ao menos 3 caracteres.")
            .MaximumLength(200).WithMessage("O nome completo deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("O e-mail informado não é válido.")
            .MaximumLength(150).WithMessage("O e-mail deve ter no máximo 150 caracteres.");

        RuleFor(x => x.DataNascimento)
            .NotEmpty().WithMessage("A data de nascimento é obrigatória.")
            .LessThan(DateTime.Today).WithMessage("A data de nascimento deve ser anterior a hoje.")
            .GreaterThan(DateTime.Today.AddYears(-130)).WithMessage("A data de nascimento informada é inválida.");

        RuleFor(x => x.Genero)
            .IsInEnum().WithMessage("O gênero informado é inválido.");

        RuleFor(x => x.Peso)
            .GreaterThan(0).WithMessage("O peso deve ser maior que zero.")
            .LessThanOrEqualTo(400).WithMessage("O peso informado é inválido.");

        RuleFor(x => x.TipoSanguineo)
            .IsInEnum().WithMessage("O tipo sanguíneo informado é inválido.");

        RuleFor(x => x.FatorRh)
            .IsInEnum().WithMessage("O fator Rh informado é inválido.");

        When(x => !string.IsNullOrWhiteSpace(x.Cep), () =>
        {
            RuleFor(x => x.Cep!)
                .Must(cep => cep.Count(char.IsDigit) == 8)
                .WithMessage("O CEP deve conter 8 dígitos (ex: 01001-000).");
        });
    }
}
