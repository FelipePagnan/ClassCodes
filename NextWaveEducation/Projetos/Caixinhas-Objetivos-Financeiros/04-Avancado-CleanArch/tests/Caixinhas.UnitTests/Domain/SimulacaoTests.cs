using Caixinhas.Domain.Entities;
using Caixinhas.Domain.Enums;
using Caixinhas.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Caixinhas.UnitTests.Domain;

/// <summary>
/// Testes da simulação de evolução da caixinha (requisito PLUS).
/// </summary>
public class SimulacaoTests
{
    private static ObjetivoFinanceiro CriarObjetivo(decimal meta = 10_000m, decimal saldo = 0m)
    {
        var objetivo = new ObjetivoFinanceiro("Caixinha de teste", meta);

        if (saldo > 0)
            objetivo.RegistrarTransacao(saldo, TipoTransacao.Deposito, DateTime.Today);

        return objetivo;
    }

    [Fact]
    public void Simular_SemJuros_DeveSomarApenasOsAportes()
    {
        var objetivo = CriarObjetivo(meta: 10_000m);

        var simulacao = objetivo.Simular(1_000m, 10, 0m);

        simulacao.SaldoFinal.Should().Be(10_000m);
        simulacao.TotalAportado.Should().Be(10_000m);
        simulacao.TotalRendimentos.Should().Be(0m);
        simulacao.Meses.Should().HaveCount(10);
    }

    [Fact]
    public void Simular_ComJuros_DeveRenderSobreOSaldoAcumulado()
    {
        var objetivo = CriarObjetivo(meta: 100_000m);

        var simulacao = objetivo.Simular(1_000m, 2, 1m);

        // Mês 1: 0 + 1000 = 1000, juros 1% = 10, saldo 1010.
        simulacao.Meses[0].Aporte.Should().Be(1_000m);
        simulacao.Meses[0].Rendimento.Should().Be(10m);
        simulacao.Meses[0].SaldoAcumulado.Should().Be(1_010m);

        // Mês 2: 1010 + 1000 = 2010, juros 1% = 20.10, saldo 2030.10.
        simulacao.Meses[1].Rendimento.Should().Be(20.10m);
        simulacao.Meses[1].SaldoAcumulado.Should().Be(2_030.10m);

        simulacao.SaldoFinal.Should().Be(2_030.10m);
        simulacao.TotalRendimentos.Should().Be(30.10m);
    }

    [Fact]
    public void Simular_DevePartirDoSaldoAtual()
    {
        var objetivo = CriarObjetivo(meta: 100_000m, saldo: 5_000m);

        var simulacao = objetivo.Simular(1_000m, 1, 0m);

        simulacao.SaldoInicial.Should().Be(5_000m);
        simulacao.SaldoFinal.Should().Be(6_000m);
    }

    [Fact]
    public void Simular_DeveInformarOMesQueAtingeAMeta()
    {
        var objetivo = CriarObjetivo(meta: 3_000m);

        var simulacao = objetivo.Simular(1_000m, 12, 0m);

        simulacao.MesQueAtingeMeta.Should().Be(3);
        simulacao.ValorFaltanteAoFim.Should().BeNull();
    }

    [Fact]
    public void Simular_QuandoNaoAtingeAMeta_DeveInformarOQueFalta()
    {
        var objetivo = CriarObjetivo(meta: 10_000m);

        var simulacao = objetivo.Simular(1_000m, 5, 0m);

        simulacao.MesQueAtingeMeta.Should().BeNull();
        simulacao.SaldoFinal.Should().Be(5_000m);
        simulacao.ValorFaltanteAoFim.Should().Be(5_000m);
    }

    [Fact]
    public void Simular_ComMetaJaAlcancada_DeveIndicarMesZero()
    {
        var objetivo = CriarObjetivo(meta: 1_000m, saldo: 1_000m);

        var simulacao = objetivo.Simular(100m, 6, 0m);

        simulacao.MesQueAtingeMeta.Should().Be(0);
    }

    [Fact]
    public void Simular_DeveCalcularOPercentualDaMetaEmCadaMes()
    {
        var objetivo = CriarObjetivo(meta: 4_000m);

        var simulacao = objetivo.Simular(1_000m, 4, 0m);

        simulacao.Meses[0].PercentualDaMeta.Should().Be(25m);
        simulacao.Meses[1].PercentualDaMeta.Should().Be(50m);
        simulacao.Meses[3].PercentualDaMeta.Should().Be(100m);
    }

    [Fact]
    public void Simular_ComAporteZero_DeveRenderApenasSobreOSaldo()
    {
        var objetivo = CriarObjetivo(meta: 100_000m, saldo: 1_000m);

        var simulacao = objetivo.Simular(0m, 1, 10m);

        simulacao.TotalAportado.Should().Be(0m);
        simulacao.Meses[0].Rendimento.Should().Be(100m);
        simulacao.SaldoFinal.Should().Be(1_100m);
    }

    // ----- Validações dos parâmetros -----

    [Fact]
    public void Simular_ComAporteNegativo_DeveLancarExcecao()
    {
        var objetivo = CriarObjetivo();

        var acao = () => objetivo.Simular(-100m, 12, 1m);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*não pode ser negativo*");
    }

    [Fact]
    public void Simular_ComAporteDeTresCasasDecimais_DeveLancarExcecao()
    {
        var objetivo = CriarObjetivo();

        var acao = () => objetivo.Simular(100.555m, 12, 1m);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*casas decimais*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(601)]
    public void Simular_ComMesesInvalidos_DeveLancarExcecao(int meses)
    {
        var objetivo = CriarObjetivo();

        var acao = () => objetivo.Simular(1_000m, meses, 1m);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*entre 1 e 600*");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Simular_ComTaxaInvalida_DeveLancarExcecao(decimal taxa)
    {
        var objetivo = CriarObjetivo();

        var acao = () => objetivo.Simular(1_000m, 12, taxa);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*entre 0% e 100%*");
    }

    [Fact]
    public void Simular_NaoDeveAlterarOSaldoReal()
    {
        var objetivo = CriarObjetivo(meta: 10_000m, saldo: 1_000m);

        objetivo.Simular(5_000m, 12, 5m);

        // A simulação é apenas projeção: não mexe no estado da caixinha.
        objetivo.ValorTotal.Should().Be(1_000m);
    }
}
