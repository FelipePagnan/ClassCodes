using Caixinhas.Domain.Entities;
using Caixinhas.Domain.Enums;
using Caixinhas.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Caixinhas.UnitTests.Domain;

public class TransacaoTests
{
    private static ObjetivoFinanceiro CriarObjetivoComSaldo(decimal saldo = 10_000m)
    {
        var objetivo = new ObjetivoFinanceiro("Caixinha de teste", 100_000m);

        if (saldo > 0)
            objetivo.RegistrarTransacao(saldo, TipoTransacao.Deposito, DateTime.Today);

        return objetivo;
    }

    // ----- Regra: a quantidade não pode ser negativa -----

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-100)]
    [InlineData(-1500.50)]
    public void RegistrarTransacao_ComQuantidadeNegativa_DeveLancarExcecao(decimal quantidade)
    {
        var objetivo = CriarObjetivoComSaldo();

        var acao = () => objetivo.RegistrarTransacao(
            quantidade, TipoTransacao.Deposito, DateTime.Today);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*não pode ser negativo*");
    }

    [Fact]
    public void RegistrarTransacao_ComQuantidadeZero_DeveLancarExcecao()
    {
        var objetivo = CriarObjetivoComSaldo();

        var acao = () => objetivo.RegistrarTransacao(0m, TipoTransacao.Deposito, DateTime.Today);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*maior que zero*");
    }

    // ----- Regra: até duas casas decimais de precisão -----

    [Theory]
    [InlineData(100)]
    [InlineData(100.5)]
    [InlineData(100.55)]
    [InlineData(0.01)]
    [InlineData(1500.99)]
    public void RegistrarTransacao_ComAteDuasCasasDecimais_DevePermitir(decimal quantidade)
    {
        var objetivo = CriarObjetivoComSaldo();

        var transacao = objetivo.RegistrarTransacao(
            quantidade, TipoTransacao.Deposito, DateTime.Today);

        transacao.Quantidade.Should().Be(quantidade);
    }

    [Theory]
    [InlineData(100.555)]
    [InlineData(0.001)]
    [InlineData(1500.9999)]
    [InlineData(10.123)]
    public void RegistrarTransacao_ComMaisDeDuasCasasDecimais_DeveLancarExcecao(decimal quantidade)
    {
        var objetivo = CriarObjetivoComSaldo();

        var acao = () => objetivo.RegistrarTransacao(
            quantidade, TipoTransacao.Deposito, DateTime.Today);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*no máximo 2 casas decimais*");
    }

    [Fact]
    public void RegistrarTransacao_ComUmCentavo_DevePermitir()
    {
        var objetivo = CriarObjetivoComSaldo();

        var transacao = objetivo.RegistrarTransacao(
            0.01m, TipoTransacao.Deposito, DateTime.Today);

        transacao.Quantidade.Should().Be(0.01m);
    }

    // ----- Regra: a data pode ser hoje ou passada, nunca futura -----

    [Fact]
    public void RegistrarTransacao_ComDataDeHoje_DevePermitir()
    {
        var objetivo = CriarObjetivoComSaldo();

        var acao = () => objetivo.RegistrarTransacao(
            100m, TipoTransacao.Deposito, DateTime.Today);

        acao.Should().NotThrow();
    }

    [Fact]
    public void RegistrarTransacao_ComDataFutura_DeveLancarExcecao()
    {
        var objetivo = CriarObjetivoComSaldo();

        var acao = () => objetivo.RegistrarTransacao(
            100m, TipoTransacao.Deposito, DateTime.Today.AddDays(1));

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*futuro*");
    }

    [Fact]
    public void RegistrarTransacao_ComDataAnteriorAoObjetivo_DeveLancarExcecao()
    {
        var objetivo = CriarObjetivoComSaldo();

        var acao = () => objetivo.RegistrarTransacao(
            100m, TipoTransacao.Deposito, DateTime.Today.AddDays(-30));

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*anterior*criação do objetivo*");
    }

    // ----- Tipo e efeito -----

    [Fact]
    public void Deposito_DeveTerEfeitoPositivo()
    {
        var objetivo = CriarObjetivoComSaldo(0);

        var transacao = objetivo.RegistrarTransacao(
            500m, TipoTransacao.Deposito, DateTime.Today);

        transacao.Tipo.Should().Be(TipoTransacao.Deposito);
        transacao.Efeito.Should().Be(500m);
        transacao.EhDeposito.Should().BeTrue();
    }

    [Fact]
    public void Saque_DeveTerEfeitoNegativo()
    {
        var objetivo = CriarObjetivoComSaldo(1_000m);

        var transacao = objetivo.RegistrarTransacao(
            300m, TipoTransacao.Saque, DateTime.Today);

        transacao.Tipo.Should().Be(TipoTransacao.Saque);
        transacao.Efeito.Should().Be(-300m);
        transacao.EhDeposito.Should().BeFalse();
    }

    [Fact]
    public void RegistrarTransacao_ComTipoInvalido_DeveLancarExcecao()
    {
        var objetivo = CriarObjetivoComSaldo();

        var acao = () => objetivo.RegistrarTransacao(
            100m, (TipoTransacao)99, DateTime.Today);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*tipo de transação*");
    }

    // ----- Propriedades -----

    [Fact]
    public void Transacao_DeveGuardarOIdDoObjetivo()
    {
        var objetivo = CriarObjetivoComSaldo(0);

        var transacao = objetivo.RegistrarTransacao(
            100m, TipoTransacao.Deposito, DateTime.Today);

        transacao.ObjetivoFinanceiroId.Should().Be(objetivo.Id);
    }

    [Fact]
    public void Transacao_DeveNascerAtiva()
    {
        var objetivo = CriarObjetivoComSaldo(0);

        var transacao = objetivo.RegistrarTransacao(
            100m, TipoTransacao.Deposito, DateTime.Today);

        transacao.EstaDeletado.Should().BeFalse();
    }

    [Fact]
    public void Valor_DeveExporOValueObjectDinheiro()
    {
        var objetivo = CriarObjetivoComSaldo(0);

        var transacao = objetivo.RegistrarTransacao(
            1_234.56m, TipoTransacao.Deposito, DateTime.Today);

        transacao.Valor.Valor.Should().Be(1_234.56m);
    }
}
