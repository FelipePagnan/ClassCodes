using Caixinhas.Application.Common;
using Caixinhas.Application.Common.Interfaces;
using Caixinhas.Domain.Entities;
using Caixinhas.Domain.Exceptions;
using Caixinhas.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Caixinhas.Application.Objetivos.Commands.EnviarCapa;

/// <summary>
/// Recebe a imagem de capa já lida em memória (PLUS). A API é responsável
/// por extrair os bytes do multipart/form-data.
/// </summary>
public record EnviarCapaCommand(int IdObjetivo, byte[] Conteudo, string NomeArquivo)
    : IRequest<ObjetivoDto>;

public class EnviarCapaCommandHandler : IRequestHandler<EnviarCapaCommand, ObjetivoDto>
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

    public async Task<ObjetivoDto> Handle(
        EnviarCapaCommand request,
        CancellationToken cancellationToken)
    {
        var objetivo = await _unitOfWork.Objetivos
            .ObterPorIdComTransacoesAsync(request.IdObjetivo, cancellationToken)
            ?? throw NaoEncontradoException.Para("Objetivo financeiro", request.IdObjetivo);

        if (request.Conteudo is null || request.Conteudo.Length == 0)
            throw new RegraNegocioException("O arquivo enviado está vazio.");

        if (request.Conteudo.Length > ObjetivoFinanceiro.TamanhoMaximoCapaBytes)
            throw new RegraNegocioException(
                $"A imagem de capa deve ter no máximo " +
                $"{ObjetivoFinanceiro.TamanhoMaximoCapaBytes / 1024 / 1024}MB.");

        if (!_validadorDeImagem.ExtensaoPermitida(request.NomeArquivo))
            throw new RegraNegocioException(
                $"Extensão não permitida. Use: {string.Join(", ", _validadorDeImagem.ExtensoesPermitidas)}");

        // Confere a assinatura binária, não apenas a extensão do arquivo.
        if (!_validadorDeImagem.EhImagemValida(request.Conteudo))
            throw new RegraNegocioException("O arquivo enviado não parece ser uma imagem válida.");

        objetivo.DefinirCapa(request.Conteudo);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Capa do objetivo {ObjetivoId} atualizada ({Bytes} bytes).",
            objetivo.Id, request.Conteudo.Length);

        return ObjetivoDto.De(objetivo);
    }
}
