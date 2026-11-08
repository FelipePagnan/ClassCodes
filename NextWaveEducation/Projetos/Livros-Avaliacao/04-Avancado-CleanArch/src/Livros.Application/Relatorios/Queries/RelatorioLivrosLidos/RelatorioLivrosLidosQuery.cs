using FluentValidation;
using Livros.Application.Common;
using Livros.Domain.Repositories;
using MediatR;

namespace Livros.Application.Relatorios.Queries.RelatorioLivrosLidos;

/// <summary>
/// Relatório da quantidade de livros lidos num determinado ano (PLUS),
/// com base na data de fim de leitura informada nas avaliações.
/// </summary>
public record RelatorioLivrosLidosQuery(int Ano) : IRequest<RelatorioLivrosLidosDto>;

public class RelatorioLivrosLidosQueryValidator : AbstractValidator<RelatorioLivrosLidosQuery>
{
    public RelatorioLivrosLidosQueryValidator()
    {
        RuleFor(r => r.Ano)
            .InclusiveBetween(1900, DateTime.Today.Year + 1)
            .WithMessage($"O ano deve estar entre 1900 e {DateTime.Today.Year + 1}.");
    }
}

public class RelatorioLivrosLidosQueryHandler
    : IRequestHandler<RelatorioLivrosLidosQuery, RelatorioLivrosLidosDto>
{
    private static readonly string[] NomesDosMeses =
    [
        "janeiro", "fevereiro", "março", "abril", "maio", "junho",
        "julho", "agosto", "setembro", "outubro", "novembro", "dezembro"
    ];

    private readonly IAvaliacaoRepository _repository;

    public RelatorioLivrosLidosQueryHandler(IAvaliacaoRepository repository)
        => _repository = repository;

    public async Task<RelatorioLivrosLidosDto> Handle(
        RelatorioLivrosLidosQuery request,
        CancellationToken cancellationToken)
    {
        var avaliacoes = await _repository.ObterPorAnoDeLeituraAsync(request.Ano, cancellationToken);

        var comLivro = avaliacoes.Where(a => a.Livro is not null).ToList();

        var porLivro = comLivro
            .GroupBy(a => a.IdLivro)
            .Select(g =>
            {
                var livro = g.First().Livro!;
                return new ItemRelatorioAnualDto(
                    g.Key,
                    livro.Titulo,
                    livro.Autor,
                    livro.Genero.ToString(),
                    livro.QuantidadePaginas,
                    g.Count(),
                    Math.Round(g.Average(a => (double)a.Nota), 2));
            })
            .OrderByDescending(i => i.NotaMedia)
            .ThenBy(i => i.Titulo)
            .ToList();

        // Leituras concluídas por mês, útil para montar um gráfico no front.
        // Os nomes são fixos porque a aplicação roda com InvariantGlobalization.
        var leiturasPorMes = Enumerable.Range(1, 12)
            .ToDictionary(
                mes => NomesDosMeses[mes - 1],
                mes => comLivro.Count(a => a.DataFimLeitura.Month == mes));

        return new RelatorioLivrosLidosDto(
            request.Ano,
            porLivro.Count,
            comLivro.Count,
            porLivro.Sum(i => i.QuantidadePaginas),
            comLivro.Select(a => a.IdUsuario).Distinct().Count(),
            comLivro.Count == 0 ? null : Math.Round(comLivro.Average(a => (double)a.Nota), 2),
            comLivro.Count == 0 ? null : Math.Round(comLivro.Average(a => (double)a.DiasDeLeitura), 1),
            porLivro,
            leiturasPorMes);
    }
}
