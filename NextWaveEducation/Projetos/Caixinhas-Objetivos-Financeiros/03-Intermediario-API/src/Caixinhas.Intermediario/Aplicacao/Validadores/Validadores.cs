using Caixinhas.Intermediario.Aplicacao.InputModels;
using Caixinhas.Intermediario.Dominio.ValueObjects;
using FluentValidation;

namespace Caixinhas.Intermediario.Aplicacao.Validadores;

public class CriarObjetivoInputModelValidator : AbstractValidator<CriarObjetivoInputModel>
{
    public CriarObjetivoInputModelValidator()
    {
        RuleFor(o => o.Titulo)
            .NotEmpty().WithMessage("O título é obrigatório.")
            .MinimumLength(3).WithMessage("O título deve ter ao menos 3 caracteres.")
            .MaximumLength(120).WithMessage("O título deve ter no máximo 120 caracteres.");

        RuleFor(o => o.QuantidadeAlvo)
            .GreaterThan(0).WithMessage("A quantidade alvo deve ser maior que zero.")
            .Must(Dinheiro.EhValido)
            .WithMessage("A quantidade alvo deve ter no máximo 2 casas decimais e não pode ser negativa.");

        When(o => o.Prazo.HasValue, () =>
        {
            RuleFor(o => o.Prazo!.Value)
                .GreaterThan(_ => DateTime.Today)
                .WithMessage("O prazo, quando informado, deve ser uma data futura.");
        });

        When(o => o.QuantidadeIdealAporteMensal.HasValue, () =>
        {
            RuleFor(o => o.QuantidadeIdealAporteMensal!.Value)
                .GreaterThan(0).WithMessage("O aporte mensal ideal deve ser maior que zero.")
                .Must(Dinheiro.EhValido)
                .WithMessage("O aporte mensal ideal deve ter no máximo 2 casas decimais.");
        });
    }
}

public class AtualizarObjetivoInputModelValidator : AbstractValidator<AtualizarObjetivoInputModel>
{
    public AtualizarObjetivoInputModelValidator()
    {
        RuleFor(o => o.Titulo)
            .NotEmpty().WithMessage("O título é obrigatório.")
            .MinimumLength(3).WithMessage("O título deve ter ao menos 3 caracteres.")
            .MaximumLength(120).WithMessage("O título deve ter no máximo 120 caracteres.");

        RuleFor(o => o.QuantidadeAlvo)
            .GreaterThan(0).WithMessage("A quantidade alvo deve ser maior que zero.")
            .Must(Dinheiro.EhValido)
            .WithMessage("A quantidade alvo deve ter no máximo 2 casas decimais e não pode ser negativa.");

        When(o => o.Prazo.HasValue, () =>
        {
            RuleFor(o => o.Prazo!.Value)
                .GreaterThan(_ => DateTime.Today)
                .WithMessage("O prazo, quando informado, deve ser uma data futura.");
        });

        When(o => o.QuantidadeIdealAporteMensal.HasValue, () =>
        {
            RuleFor(o => o.QuantidadeIdealAporteMensal!.Value)
                .GreaterThan(0).WithMessage("O aporte mensal ideal deve ser maior que zero.")
                .Must(Dinheiro.EhValido)
                .WithMessage("O aporte mensal ideal deve ter no máximo 2 casas decimais.");
        });
    }
}

public class CriarTransacaoInputModelValidator : AbstractValidator<CriarTransacaoInputModel>
{
    public CriarTransacaoInputModelValidator()
    {
        // Regra de negócio: a quantidade não pode ser negativa e deve ter
        // no máximo duas casas decimais.
        RuleFor(t => t.Quantidade)
            .GreaterThan(0).WithMessage("A quantidade da transação deve ser maior que zero.")
            .Must(Dinheiro.EhValido)
            .WithMessage("A quantidade deve ter no máximo 2 casas decimais e não pode ser negativa.");

        RuleFor(t => t.Tipo)
            .IsInEnum().WithMessage("O tipo de transação deve ser Deposito ou Saque.");

        When(t => t.DataTransacao.HasValue, () =>
        {
            RuleFor(t => t.DataTransacao!.Value)
                .LessThanOrEqualTo(_ => DateTime.Today.AddDays(1))
                .WithMessage("A data da transação não pode estar no futuro.");
        });
    }
}

public class SimularEvolucaoInputModelValidator : AbstractValidator<SimularEvolucaoInputModel>
{
    public SimularEvolucaoInputModelValidator()
    {
        RuleFor(s => s.AporteMensal)
            .GreaterThanOrEqualTo(0).WithMessage("O aporte mensal não pode ser negativo.")
            .Must(Dinheiro.EhValido)
            .WithMessage("O aporte mensal deve ter no máximo 2 casas decimais.");

        RuleFor(s => s.QuantidadeMeses)
            .InclusiveBetween(1, 600)
            .WithMessage("A quantidade de meses deve estar entre 1 e 600.");

        RuleFor(s => s.TaxaJurosMensalPercentual)
            .InclusiveBetween(0m, 100m)
            .WithMessage("A taxa de juros mensal deve estar entre 0% e 100%.");
    }
}
