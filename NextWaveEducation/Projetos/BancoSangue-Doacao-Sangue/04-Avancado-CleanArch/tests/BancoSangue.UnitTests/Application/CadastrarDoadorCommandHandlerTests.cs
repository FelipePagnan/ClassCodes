using BancoSangue.Application.Common.Interfaces;
using BancoSangue.Application.Doadores.Commands.CadastrarDoador;
using BancoSangue.Domain.Entities;
using BancoSangue.Domain.Enums;
using BancoSangue.Domain.Exceptions;
using BancoSangue.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BancoSangue.UnitTests.Application;

public class CadastrarDoadorCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IDoadorRepository> _doadorRepository = new();
    private readonly Mock<ICepService> _cepService = new();
    private readonly CadastrarDoadorCommandHandler _handler;

    public CadastrarDoadorCommandHandlerTests()
    {
        _unitOfWork.Setup(u => u.Doadores).Returns(_doadorRepository.Object);

        _handler = new CadastrarDoadorCommandHandler(
            _unitOfWork.Object,
            _cepService.Object,
            NullLogger<CadastrarDoadorCommandHandler>.Instance);
    }

    private static CadastrarDoadorCommand ComandoValido(string? cep = null) => new()
    {
        NomeCompleto = "Maria Oliveira",
        Email = "maria@example.com",
        DataNascimento = DateTime.Today.AddYears(-28),
        Genero = Genero.Feminino,
        Peso = 62,
        TipoSanguineo = TipoSanguineo.A,
        FatorRh = FatorRh.Positivo,
        Cep = cep
    };

    [Fact]
    public async Task Handle_ComDadosValidos_DeveCadastrarEComitar()
    {
        _doadorRepository
            .Setup(r => r.ExisteEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var resultado = await _handler.Handle(ComandoValido(), CancellationToken.None);

        resultado.NomeCompleto.Should().Be("Maria Oliveira");
        resultado.Email.Should().Be("maria@example.com");
        resultado.TipoSanguineo.Should().Be("A+");
        resultado.AptoParaDoar.Should().BeTrue();

        _doadorRepository.Verify(
            r => r.AdicionarAsync(It.IsAny<Doador>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- Regra: não deixar cadastrar um doador com o mesmo e-mail -----

    [Fact]
    public async Task Handle_ComEmailJaCadastrado_DeveLancarExcecao()
    {
        _doadorRepository
            .Setup(r => r.ExisteEmailAsync("maria@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var acao = async () => await _handler.Handle(ComandoValido(), CancellationToken.None);

        await acao.Should().ThrowAsync<RegraNegocioException>()
            .WithMessage("*Já existe um doador cadastrado com o e-mail*");

        _doadorRepository.Verify(
            r => r.AdicionarAsync(It.IsAny<Doador>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeveVerificarOEmailNormalizadoEmMinusculas()
    {
        _doadorRepository
            .Setup(r => r.ExisteEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var comando = ComandoValido() with { Email = "MARIA@Example.COM" };

        await _handler.Handle(comando, CancellationToken.None);

        _doadorRepository.Verify(
            r => r.ExisteEmailAsync("maria@example.com", It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- Regra: menor de idade pode ter cadastro -----

    [Fact]
    public async Task Handle_MenorDeIdade_DeveCadastrarComoNaoApto()
    {
        _doadorRepository
            .Setup(r => r.ExisteEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var comando = ComandoValido() with { DataNascimento = DateTime.Today.AddYears(-15) };

        var resultado = await _handler.Handle(comando, CancellationToken.None);

        resultado.Idade.Should().Be(15);
        resultado.AptoParaDoar.Should().BeFalse();
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ComPesoAbaixoDoMinimo_DeveCadastrarComoNaoApto()
    {
        _doadorRepository
            .Setup(r => r.ExisteEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var comando = ComandoValido() with { Peso = 45 };

        var resultado = await _handler.Handle(comando, CancellationToken.None);

        resultado.AptoParaDoar.Should().BeFalse();
    }

    // ----- Integração com a API de CEP -----

    [Fact]
    public async Task Handle_ComCepValido_DeveBuscarEPreencherOEndereco()
    {
        _doadorRepository
            .Setup(r => r.ExisteEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _cepService
            .Setup(c => c.ConsultarAsync("01001-000", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnderecoCepDto("Praça da Sé", "São Paulo", "SP", "01001-000"));

        var resultado = await _handler.Handle(ComandoValido("01001-000"), CancellationToken.None);

        resultado.Endereco.Should().NotBeNull();
        resultado.Endereco!.Cidade.Should().Be("São Paulo");
        resultado.Endereco.Estado.Should().Be("SP");
        resultado.Endereco.Logradouro.Should().Be("Praça da Sé");
    }

    [Fact]
    public async Task Handle_ComComplemento_DeveConcatenarNoLogradouro()
    {
        _doadorRepository
            .Setup(r => r.ExisteEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _cepService
            .Setup(c => c.ConsultarAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnderecoCepDto("Praça da Sé", "São Paulo", "SP", "01001-000"));

        var comando = ComandoValido("01001-000") with { Complemento = "apto 42" };

        var resultado = await _handler.Handle(comando, CancellationToken.None);

        resultado.Endereco!.Logradouro.Should().Be("Praça da Sé, apto 42");
    }

    [Fact]
    public async Task Handle_QuandoOCepNaoEEncontrado_DeveLancarExcecao()
    {
        _doadorRepository
            .Setup(r => r.ExisteEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _cepService
            .Setup(c => c.ConsultarAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EnderecoCepDto?)null);

        var acao = async () => await _handler.Handle(
            ComandoValido("99999-999"), CancellationToken.None);

        await acao.Should().ThrowAsync<RegraNegocioException>()
            .WithMessage("*Não foi possível localizar o endereço*");
    }

    [Fact]
    public async Task Handle_SemCep_NaoDeveChamarOServicoDeCep()
    {
        _doadorRepository
            .Setup(r => r.ExisteEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _handler.Handle(ComandoValido(), CancellationToken.None);

        _cepService.Verify(
            c => c.ConsultarAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
