using Livros.Application.Common;
using Livros.Application.Common.Interfaces;
using Livros.Domain.Entities;
using Livros.Domain.Exceptions;
using Livros.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Livros.Application.Livros.Commands.EnviarCapa;

/// <summary>
/// Recebe o arquivo de imagem de capa já lido em memória (PLUS).
/// A API é responsável por extrair os bytes do multipart/form-data.
/// </summary>
public record EnviarCapaCommand(int IdLivro, byte[] Conteudo, string NomeArquivo) : IRequest<LivroDto>;

public class EnviarCapaCommandHandler : IRequestHandler<EnviarCapaCommand, LivroDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidadorDeImagem _validadorDeImagem;
    private readonly ILogger<EnviarCapaCommandHandler> _logger;

    public EnviarCapaCommandHandler(
        IUnitOfWork unitOfWork,
        IValidadorDeImagem validadorDeImagem,
        ILogger<EnviarCapaCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _validadorDeImagem = validadorDeImagem;
        _logger = logger;
    }

    public async Task<LivroDto> Handle(EnviarCapaCommand request, CancellationToken cancellationToken)
    {
        var livro = await _unitOfWork.Livros.ObterPorIdComAvaliacoesAsync(request.IdLivro, cancellationToken)
            ?? throw NaoEncontradoException.Para("Livro", request.IdLivro);

        if (request.Conteudo is null || request.Conteudo.Length == 0)
            throw new RegraNegocioException("O arquivo enviado está vazio.");

        if (request.Conteudo.Length > Livro.TamanhoMaximoCapaBytes)
            throw new RegraNegocioException(
                $"A imagem de capa deve ter no máximo {Livro.TamanhoMaximoCapaBytes / 1024 / 1024}MB.");

        if (!_validadorDeImagem.ExtensaoPermitida(request.NomeArquivo))
            throw new RegraNegocioException(
                $"Extensão não permitida. Use: {string.Join(", ", _validadorDeImagem.ExtensoesPermitidas)}");

        // Confere a assinatura binária, não apenas a extensão do arquivo.
        if (!_validadorDeImagem.EhImagemValida(request.Conteudo))
            throw new RegraNegocioException("O arquivo enviado não parece ser uma imagem válida.");

        livro.DefinirCapa(request.Conteudo);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Capa do livro {LivroId} atualizada ({Bytes} bytes).", livro.Id, request.Conteudo.Length);

        return LivroDto.De(livro);
    }
}
