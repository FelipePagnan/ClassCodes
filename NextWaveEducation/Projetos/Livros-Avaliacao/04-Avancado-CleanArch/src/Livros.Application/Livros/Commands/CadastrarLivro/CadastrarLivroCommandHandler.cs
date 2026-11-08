using Livros.Application.Common;
using Livros.Domain.Exceptions;
using Livros.Domain.Repositories;
using Livros.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Livros.Application.Livros.Commands.CadastrarLivro;

public class CadastrarLivroCommandHandler : IRequestHandler<CadastrarLivroCommand, LivroDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CadastrarLivroCommandHandler> _logger;

    public CadastrarLivroCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CadastrarLivroCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<LivroDto> Handle(CadastrarLivroCommand request, CancellationToken cancellationToken)
    {
        var isbnNormalizado = Isbn.Criar(request.Isbn).Normalizado;

        // Regra de negócio: livros não podem ser cadastrados com o mesmo ISBN.
        if (await _unitOfWork.Livros.ExisteIsbnAsync(isbnNormalizado, ct: cancellationToken))
            throw new RegraNegocioException($"Já existe um livro cadastrado com o ISBN '{request.Isbn}'.");

        var livro = new Domain.Entities.Livro(
            request.Titulo,
            request.Descricao,
            request.Isbn,
            request.Autor,
            request.Editora,
            request.Genero,
            request.AnoDePublicacao,
            request.QuantidadePaginas);

        await _unitOfWork.Livros.AdicionarAsync(livro, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Livro {LivroId} '{Titulo}' cadastrado.", livro.Id, livro.Titulo);

        return LivroDto.De(livro);
    }
}
