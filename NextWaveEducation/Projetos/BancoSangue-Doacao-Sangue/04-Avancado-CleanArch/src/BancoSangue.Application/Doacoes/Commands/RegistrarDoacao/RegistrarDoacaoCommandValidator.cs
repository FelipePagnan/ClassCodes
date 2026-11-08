using BancoSangue.Domain.Entities;
using FluentValidation;

namespace BancoSangue.Application.Doacoes.Commands.RegistrarDoacao;

public class RegistrarDoacaoCommandValidator : AbstractValidator<RegistrarDoacaoCommand>
{
    public RegistrarDoacaoCommandValidator()
    {
        RuleFor(x => x.DoadorId)
            .GreaterThan(0).WithMessage("O id do doador é obrigatório.");

        RuleFor(x => x.QuantidadeML)
            .InclusiveBetween(Doacao.QuantidadeMinimaML, Doacao.QuantidadeMaximaML)
            .WithMessage($"A quantidade doada deve estar entre {Doacao.QuantidadeMinimaML}ml e {Doacao.QuantidadeMaximaML}ml.");

        When(x => x.DataDoacao.HasValue, () =>
        {
            RuleFor(x => x.DataDoacao!.Value)
                .LessThanOrEqualTo(_ => DateTime.Now.AddMinutes(5))
                .WithMessage("A data da doação não pode estar no futuro.");
        });
    }
}
