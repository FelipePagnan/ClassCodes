using BancoSangue.Application.Common;
using BancoSangue.Domain.Enums;
using MediatR;

namespace BancoSangue.Application.Doadores.Commands.CadastrarDoador;

/// <summary>
/// Command (lado de escrita do CQRS) para cadastrar um novo doador.
/// Informe o CEP para que o endereço seja buscado na API ViaCEP.
/// </summary>
public record CadastrarDoadorCommand : IRequest<DoadorDto>
{
    public string NomeCompleto { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime DataNascimento { get; init; }
    public Genero Genero { get; init; }
    public double Peso { get; init; }
    public TipoSanguineo TipoSanguineo { get; init; }
    public FatorRh FatorRh { get; init; }
    public string? Cep { get; init; }
    public string? Complemento { get; init; }
}
