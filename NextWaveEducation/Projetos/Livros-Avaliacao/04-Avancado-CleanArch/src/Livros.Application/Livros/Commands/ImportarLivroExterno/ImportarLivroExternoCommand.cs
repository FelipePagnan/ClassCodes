using FluentValidation;
using Livros.Application.Common;
using Livros.Application.Common.Interfaces;
using Livros.Domain.Enums;
using Livros.Domain.Exceptions;
using Livros.Domain.Repositories;
using Livros.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Livros.Application.Livros.Commands.ImportarLivroExterno;

/// <summary>
/// Busca um livro na API externa pelo ISBN e cadastra na base local (PLUS).
/// </summary>
public record ImportarLivroExternoCommand : IRequest<LivroDto>
{
    public string Isbn { get; init; } = string.Empty;

    /// <summary>Gênero a aplicar. Quando omitido, é inferido da categoria da API.</summary>
    public GeneroLivroEnum? Genero { get; init; }
}

public class ImportarLivroExternoCommandValidator : AbstractValidator<ImportarLivroExternoCommand>
{
    public ImportarLivroExternoCommandValidator()
    {
        RuleFor(i => i.Isbn)
            .NotEmpty().WithMessage("O ISBN é obrigatório.")
            .Must(Isbn.EhValido).WithMessage("O ISBN deve ter 10 ou 13 dígitos.");

        When(i => i.Genero.HasValue, () =>
        {
            RuleFor(i => i.Genero!.Value)
                .IsInEnum().WithMessage("O gênero informado é inválido.");
        });
    }
}

public class ImportarLivroExternoCommandHandler
    : IRequestHandler<ImportarLivroExternoCommand, LivroDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConsultaLivroExternoService _consultaExterna;
    private readonly ILogger<ImportarLivroExternoCommandHandler> _logger;

    public ImportarLivroExternoCommandHandler(
        IUnitOfWork unitOfWork,
        IConsultaLivroExternoService consultaExterna,
        ILogger<ImportarLivroExternoCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _consultaExterna = consultaExterna;
        _logger = logger;
    }

    public async Task<LivroDto> Handle(
        ImportarLivroExternoCommand request,
        CancellationToken cancellationToken)
    {
        var isbnNormalizado = Isbn.Criar(request.Isbn).Normalizado;

        if (await _unitOfWork.Livros.ExisteIsbnAsync(isbnNormalizado, ct: cancellationToken))
            throw new RegraNegocioException($"Já existe um livro cadastrado com o ISBN '{request.Isbn}'.");

        var externo = await _consultaExterna.ConsultarPorIsbnAsync(request.Isbn, cancellationToken)
            ?? throw new RegraNegocioException(
                $"Nenhum livro encontrado na API externa para o ISBN '{request.Isbn}'.");

        var isbnFinal = string.IsNullOrWhiteSpace(externo.Isbn) ? request.Isbn : externo.Isbn;

        var livro = new Domain.Entities.Livro(
            externo.Titulo,
            externo.Descricao,
            isbnFinal,
            externo.Autor,
            externo.Editora,
            request.Genero ?? MapeadorDeGenero.Mapear(externo.Categoria),
            externo.AnoDePublicacao,
            externo.QuantidadePaginas);

        await _unitOfWork.Livros.AdicionarAsync(livro, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Livro {LivroId} '{Titulo}' importado da API externa.", livro.Id, livro.Titulo);

        return LivroDto.De(livro);
    }
}

/// <summary>
/// Traduz as categorias livres da API externa para o enum de gênero do sistema.
/// </summary>
public static class MapeadorDeGenero
{
    public static GeneroLivroEnum Mapear(string? categoria)
    {
        if (string.IsNullOrWhiteSpace(categoria)) return GeneroLivroEnum.Outro;

        var texto = categoria.ToLowerInvariant();

        if (texto.Contains("juvenile")) return GeneroLivroEnum.Infantil;
        if (texto.Contains("science fiction")) return GeneroLivroEnum.FiccaoCientifica;
        if (texto.Contains("fantasy")) return GeneroLivroEnum.Fantasia;
        if (texto.Contains("thriller") || texto.Contains("mystery")) return GeneroLivroEnum.Suspense;
        if (texto.Contains("horror")) return GeneroLivroEnum.Terror;
        if (texto.Contains("biography")) return GeneroLivroEnum.Biografia;
        if (texto.Contains("history")) return GeneroLivroEnum.Historia;
        if (texto.Contains("computer") || texto.Contains("technology")) return GeneroLivroEnum.Tecnico;
        if (texto.Contains("self-help")) return GeneroLivroEnum.Autoajuda;
        if (texto.Contains("poetry")) return GeneroLivroEnum.Poesia;
        if (texto.Contains("romance")) return GeneroLivroEnum.Romance;
        if (texto.Contains("fiction")) return GeneroLivroEnum.Ficcao;

        return GeneroLivroEnum.Outro;
    }
}
