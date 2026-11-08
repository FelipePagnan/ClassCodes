using BancoSangue.Application.Common;
using MediatR;

namespace BancoSangue.Application.Doacoes.Commands.RegistrarDoacao;

public record RegistrarDoacaoCommand : IRequest<ResultadoDoacaoDto>
{
    public int DoadorId { get; init; }
    public int QuantidadeML { get; init; }

    /// <summary>Data da doação. Quando omitida, usa a data/hora atual.</summary>
    public DateTime? DataDoacao { get; init; }
}
