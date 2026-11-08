using Caixinhas.Application.Transacoes.Commands.CadastrarTransacao;
using Caixinhas.Domain.Entities;
using Caixinhas.Domain.Enums;
using Caixinhas.Domain.Exceptions;
using Caixinhas.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Caixinhas.UnitTests.Application;

public class CadastrarTransacaoCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IObjetivoRepository> _objetivoRepository = new();
    private readonly CadastrarTransacaoCommandHandler _handler;

    public CadastrarTransacaoCommandHandlerTests()
    {
        _unitOfWork.Setup(u => u.Objetivos).Returns(_objetivoRepository.Object);

        // ExecutarEmTransacaoAsync precisa realmente rodar a operação para
        // que o handler seja exercitado pelo teste.
        _unitOfWork
            .Setup(u => u.ExecutarEmTransacaoAsync(
                It.IsAny<Func<CancellationToken, Task<Caixinhas.Application.Common.ResultadoTransacaoDto>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task<Caixinhas.Application.Common.ResultadoTransacaoDto>>, CancellationToken>(
                (operacao, ct) => operacao(ct));

        _handler = new CadastrarTransacaoCommandHandler(
            _unitOfWork.Object,
            NullLogger<CadastrarTransacaoCommandHandler>.Instance);
    }

    private static ObjetivoFinanceiro CriarObjetivo(decimal meta = 10_000m, decimal saldo = 0m)
    {
        var objetivo = new ObjetivoFinanceiro("Viagem para o Japão", meta);

        if (saldo > 0)
            objetivo.RegistrarTransacao(saldo, TipoTransacao.Deposito, DateTime.Today);

        return objetivo;
    }

    private void Configurar(ObjetivoFinanceiro objetivo)
        => _objetivoRepository
            .Setup(r => r.ObterPorIdComTransacoesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(objetivo);

    private static CadastrarTransacaoCommand Comando(
        decimal quantidade = 1_000m,
        TipoTransacao tipo = TipoTransacao.Deposito) => new()
    {
        IdObjetivo = 1,
        Quantidade = quantidade,
        Tipo = tipo,
        DataTransacao = DateTime.Today
    };

    [Fact]
    public async Task Handle_ComDepositoValido_DeveAtualizarOValorTotalEComitar()
    {
        Configurar(CriarObjetivo());

        var resultado = await _handler.Handle(Comando(), CancellationToken.None);

        resultado.Transacao.Quantidade.Should().Be(1_000m);
        resultado.Transacao.Tipo.Should().Be("Deposito");
        resultado.ValorTotalAnterior.Should().Be(0m);
        resultado.ValorTotalAtualizado.Should().Be(1_000m);
        resultado.TituloObjetivo.Should().Be("Viagem para o Japão");

        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveRodarDentroDeUmaTransacaoDeBanco()
    {
        Configurar(CriarObjetivo());

        await _handler.Handle(Comando(), CancellationToken.None);

        // A transação nova e o ValorTotal atualizado precisam entrar juntos.
        _unitOfWork.Verify(
            u => u.ExecutarEmTransacaoAsync(
                It.IsAny<Func<CancellationToken, Task<Caixinhas.Application.Common.ResultadoTransacaoDto>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ComSaqueValido_DeveSubtrairDoValorTotal()
    {
        Configurar(CriarObjetivo(saldo: 5_000m));

        var resultado = await _handler.Handle(
            Comando(1_500m, TipoTransacao.Saque), CancellationToken.None);

        resultado.ValorTotalAnterior.Should().Be(5_000m);
        resultado.ValorTotalAtualizado.Should().Be(3_500m);
    }

    [Fact]
    public async Task Handle_ComObjetivoInexistente_DeveLancarNaoEncontrado()
    {
        _objetivoRepository
            .Setup(r => r.ObterPorIdComTransacoesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ObjetivoFinanceiro?)null);

        var acao = async () => await _handler.Handle(Comando(), CancellationToken.None);

        await acao.Should().ThrowAsync<NaoEncontradoException>();
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ----- Regra: quantidade não negativa e com até 2 casas decimais -----

    [Theory]
    [InlineData(-100)]
    [InlineData(0)]
    public async Task Handle_ComQuantidadeInvalida_DeveLancarRegraNegocio(decimal quantidade)
    {
        Configurar(CriarObjetivo());

        var acao = async () => await _handler.Handle(
            Comando(quantidade), CancellationToken.None);

        await acao.Should().ThrowAsync<RegraNegocioException>();
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComTresCasasDecimais_DeveLancarRegraNegocio()
    {
        Configurar(CriarObjetivo());

        var acao = async () => await _handler.Handle(
            Comando(100.555m), CancellationToken.None);

        await acao.Should().ThrowAsync<RegraNegocioException>()
            .WithMessage("*no máximo 2 casas decimais*");
    }

    // ----- Regra: saque não pode deixar o saldo negativo -----

    [Fact]
    public async Task Handle_ComSaqueAcimaDoSaldo_DeveLancarRegraNegocio()
    {
        Configurar(CriarObjetivo(saldo: 500m));

        var acao = async () => await _handler.Handle(
            Comando(1_000m, TipoTransacao.Saque), CancellationToken.None);

        await acao.Should().ThrowAsync<RegraNegocioException>()
            .WithMessage("*Saldo insuficiente*");
    }

    // ----- Meta alcançada -----

    [Fact]
    public async Task Handle_AoBaterAMeta_DeveDevolverMensagemEStatusCompleto()
    {
        Configurar(CriarObjetivo(meta: 1_000m));

        var resultado = await _handler.Handle(Comando(1_000m), CancellationToken.None);

        resultado.MetaAlcancada.Should().BeTrue();
        resultado.StatusObjetivo.Should().Be("Completo");
        resultado.PercentualConcluido.Should().Be(100m);
        resultado.Mensagem.Should().NotBeNull();
        resultado.Mensagem.Should().Contain("Meta alcançada");
    }

    [Fact]
    public async Task Handle_SemBaterAMeta_NaoDeveDevolverMensagem()
    {
        Configurar(CriarObjetivo(meta: 10_000m));

        var resultado = await _handler.Handle(Comando(1_000m), CancellationToken.None);

        resultado.MetaAlcancada.Should().BeFalse();
        resultado.StatusObjetivo.Should().Be("EmProgresso");
        resultado.Mensagem.Should().BeNull();
    }

    [Fact]
    public async Task Handle_DeveCalcularOValorRestante()
    {
        Configurar(CriarObjetivo(meta: 10_000m));

        var resultado = await _handler.Handle(Comando(2_500m), CancellationToken.None);

        resultado.ValorRestante.Should().Be(7_500m);
        resultado.PercentualConcluido.Should().Be(25m);
    }

    [Fact]
    public async Task Handle_ComDataFutura_DeveLancarRegraNegocio()
    {
        Configurar(CriarObjetivo());

        var comando = Comando() with { DataTransacao = DateTime.Today.AddDays(5) };

        var acao = async () => await _handler.Handle(comando, CancellationToken.None);

        await acao.Should().ThrowAsync<RegraNegocioException>()
            .WithMessage("*futuro*");
    }

    [Fact]
    public async Task Handle_SemDataInformada_DeveUsarHoje()
    {
        Configurar(CriarObjetivo());

        var comando = Comando() with { DataTransacao = null };

        var resultado = await _handler.Handle(comando, CancellationToken.None);

        resultado.Transacao.DataTransacao.Should().Be(DateTime.Today);
    }
}
