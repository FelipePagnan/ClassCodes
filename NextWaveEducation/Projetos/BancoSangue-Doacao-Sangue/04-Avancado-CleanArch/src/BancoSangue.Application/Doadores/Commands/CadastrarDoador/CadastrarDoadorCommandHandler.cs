using BancoSangue.Application.Common;
using BancoSangue.Application.Common.Interfaces;
using BancoSangue.Domain.Entities;
using BancoSangue.Domain.Exceptions;
using BancoSangue.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BancoSangue.Application.Doadores.Commands.CadastrarDoador;

public class CadastrarDoadorCommandHandler : IRequestHandler<CadastrarDoadorCommand, DoadorDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICepService _cepService;
    private readonly ILogger<CadastrarDoadorCommandHandler> _logger;

    public CadastrarDoadorCommandHandler(
        IUnitOfWork unitOfWork,
        ICepService cepService,
        ILogger<CadastrarDoadorCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _cepService = cepService;
        _logger = logger;
    }

    public async Task<DoadorDto> Handle(CadastrarDoadorCommand request, CancellationToken cancellationToken)
    {
        var emailNormalizado = request.Email.Trim().ToLowerInvariant();

        // Regra de negócio: não permitir cadastrar um doador com o mesmo e-mail.
        if (await _unitOfWork.Doadores.ExisteEmailAsync(emailNormalizado, cancellationToken))
            throw new RegraNegocioException($"Já existe um doador cadastrado com o e-mail '{emailNormalizado}'.");

        var doador = new Doador(
            request.NomeCompleto,
            emailNormalizado,
            request.DataNascimento,
            request.Genero,
            request.Peso,
            request.TipoSanguineo,
            request.FatorRh);

        // Integração com API externa de CEP (PLUS).
        if (!string.IsNullOrWhiteSpace(request.Cep))
        {
            var dadosCep = await _cepService.ConsultarAsync(request.Cep, cancellationToken)
                ?? throw new RegraNegocioException(
                    $"Não foi possível localizar o endereço para o CEP '{request.Cep}'.");

            var endereco = new Endereco(
                dadosCep.Logradouro,
                dadosCep.Cidade,
                dadosCep.Estado,
                dadosCep.Cep);

            endereco.AdicionarComplemento(request.Complemento);
            doador.DefinirEndereco(endereco);
        }

        await _unitOfWork.Doadores.AdicionarAsync(doador, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Doador {DoadorId} ({Nome}) cadastrado. Apto para doar: {Apto}",
            doador.Id, doador.NomeCompleto, doador.EstaAptoParaDoar);

        return DoadorDto.De(doador);
    }
}
