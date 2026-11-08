using Livros.Application.Common;
using Livros.Application.Common.Interfaces;
using Livros.Domain.Exceptions;
using Livros.Domain.Repositories;
using Livros.Domain.ValueObjects;
using MediatR;

namespace Livros.Application.Livros.Queries.ConsultarApiExterna;

/// <summary>
/// Consulta livros na API externa. Informe o ISBN ou um termo livre (PLUS).
/// </summary>
public record ConsultarApiExternaQuery(string? Isbn, string? Termo)
    : IRequest<List<LivroExternoResultadoDto>>;

public class ConsultarApiExternaQueryHandler
    : IRequestHandler<ConsultarApiExternaQuery, List<LivroExternoResultadoDto>>
{
    private readonly IConsultaLivroExternoService _consultaExterna;
    private readonly ILivroRepository _livroRepository;

    public ConsultarApiExternaQueryHandler(
        IConsultaLivroExternoService consultaExterna,
        ILivroRepository livroRepository)
    {
        _consultaExterna = consultaExterna;
        _livroRepository = livroRepository;
    }

    public async Task<List<LivroExternoResultadoDto>> Handle(
        ConsultarApiExternaQuery request,
        CancellationToken cancellationToken)
    {
        var temIsbn = !string.IsNullOrWhiteSpace(request.Isbn);
        var temTermo = !string.IsNullOrWhiteSpace(request.Termo);

        if (!temIsbn && !temTermo)
            throw new RegraNegocioException("Informe um ISBN ou um termo de busca.");

        List<LivroExternoDto> resultados;

        if (temIsbn)
        {
            var unico = await _consultaExterna.ConsultarPorIsbnAsync(request.Isbn!, cancellationToken);
            resultados = unico is null ? new List<LivroExternoDto>() : new List<LivroExternoDto> { unico };
        }
        else
        {
            resultados = await _consultaExterna.ConsultarPorTermoAsync(request.Termo!, cancellationToken);
        }

        var saida = new List<LivroExternoResultadoDto>(resultados.Count);

        foreach (var dto in resultados)
        {
            // Marca os resultados que já existem na base local.
            var jaCadastrado = !string.IsNullOrWhiteSpace(dto.Isbn)
                && Isbn.EhValido(dto.Isbn)
                && await _livroRepository.ExisteIsbnAsync(
                    Isbn.Criar(dto.Isbn).Normalizado, ct: cancellationToken);

            saida.Add(new LivroExternoResultadoDto(
                dto.Titulo, dto.Descricao, dto.Isbn, dto.Autor, dto.Editora,
                dto.Categoria, dto.AnoDePublicacao, dto.QuantidadePaginas, jaCadastrado));
        }

        return saida;
    }
}
