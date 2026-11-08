using BancoSangue.Intermediario.Aplicacao.InputModels;
using BancoSangue.Intermediario.Dominio.Entidades;
using FluentValidation;

namespace BancoSangue.Intermediario.Aplicacao.Validadores;

public class CriarDoacaoInputModelValidator : AbstractValidator<CriarDoacaoInputModel>
{
    public CriarDoacaoInputModelValidator()
    {
        RuleFor(d => d.DoadorId)
            .GreaterThan(0).WithMessage("O id do doador é obrigatório.");

        RuleFor(d => d.QuantidadeML)
            .InclusiveBetween(Doacao.QuantidadeMinimaML, Doacao.QuantidadeMaximaML)
            .WithMessage($"A quantidade doada deve estar entre {Doacao.QuantidadeMinimaML}ml e {Doacao.QuantidadeMaximaML}ml.");

        When(d => d.DataDoacao.HasValue, () =>
        {
            RuleFor(d => d.DataDoacao!.Value)
                .LessThanOrEqualTo(_ => DateTime.Now.AddMinutes(5))
                .WithMessage("A data da doação não pode estar no futuro.");
        });
    }
}
