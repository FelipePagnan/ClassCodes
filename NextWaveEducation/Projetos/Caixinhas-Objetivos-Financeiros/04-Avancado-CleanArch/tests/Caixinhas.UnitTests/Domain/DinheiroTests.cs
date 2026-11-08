using Caixinhas.Domain.Exceptions;
using Caixinhas.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Caixinhas.UnitTests.Domain;

/// <summary>
/// Testes do Value Object onde moram as duas regras de dinheiro:
/// não pode ser negativo e tem no máximo duas casas decimais.
/// </summary>
public class DinheiroTests
{
    // ----- Regra: não pode ser negativo -----

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-1)]
    [InlineData(-1000.50)]
    public void Criar_ComValorNegativo_DeveLancarExcecao(decimal valor)
    {
        var acao = () => Dinheiro.Criar(valor);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*não pode ser negativo*");
    }

    [Fact]
    public void Criar_ComZero_DevePermitir()
    {
        var dinheiro = Dinheiro.Criar(0m);

        dinheiro.Valor.Should().Be(0m);
        dinheiro.EhZero.Should().BeTrue();
    }

    [Fact]
    public void CriarPositivo_ComZero_DeveLancarExcecao()
    {
        var acao = () => Dinheiro.CriarPositivo(0m);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*maior que zero*");
    }

    // ----- Regra: até duas casas decimais -----

    [Theory]
    [InlineData(100)]
    [InlineData(100.5)]
    [InlineData(100.55)]
    [InlineData(0.01)]
    [InlineData(1500.99)]
    public void Criar_ComAteDuasCasasDecimais_DevePermitir(decimal valor)
    {
        var dinheiro = Dinheiro.Criar(valor);

        dinheiro.Valor.Should().Be(valor);
    }

    [Theory]
    [InlineData(100.555)]
    [InlineData(0.001)]
    [InlineData(1500.9999)]
    [InlineData(10.123)]
    public void Criar_ComMaisDeDuasCasasDecimais_DeveLancarExcecao(decimal valor)
    {
        var acao = () => Dinheiro.Criar(valor);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*no máximo 2 casas decimais*");
    }

    [Fact]
    public void Criar_ComZerosAoFinal_DevePermitir()
    {
        // 10.500 tem escala 3 mas o valor é 10.50, então passa.
        var acao = () => Dinheiro.Criar(10.500m);

        acao.Should().NotThrow();
    }

    [Fact]
    public void Criar_AcimaDoValorMaximo_DeveLancarExcecao()
    {
        var acao = () => Dinheiro.Criar(Dinheiro.ValorMaximo + 1m);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*excede o limite*");
    }

    [Fact]
    public void Criar_ComMensagemDeCampo_DeveUsarONomeInformado()
    {
        var acao = () => Dinheiro.Criar(-5m, "valor da transação");

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*valor da transação*");
    }

    // ----- EhValido (usado pelos validadores) -----

    [Theory]
    [InlineData(0, true)]
    [InlineData(100.55, true)]
    [InlineData(100.555, false)]
    [InlineData(-1, false)]
    public void EhValido_DeveRefletirAsRegras(decimal valor, bool esperado)
        => Dinheiro.EhValido(valor).Should().Be(esperado);

    // ----- Operações -----

    [Fact]
    public void Somar_DeveDevolverASoma()
        => Dinheiro.Criar(100.50m).Somar(Dinheiro.Criar(50.25m)).Valor.Should().Be(150.75m);

    [Fact]
    public void Subtrair_ComResultadoPositivo_DeveFuncionar()
        => Dinheiro.Criar(100m).Subtrair(Dinheiro.Criar(40.50m)).Valor.Should().Be(59.50m);

    [Fact]
    public void Subtrair_ComResultadoNegativo_DeveLancarExcecao()
    {
        var acao = () => Dinheiro.Criar(40m).Subtrair(Dinheiro.Criar(100m));

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*negativo*");
    }

    [Fact]
    public void Comparacoes_DevemFuncionar()
    {
        var cem = Dinheiro.Criar(100m);
        var cinquenta = Dinheiro.Criar(50m);

        (cem > cinquenta).Should().BeTrue();
        (cinquenta < cem).Should().BeTrue();
        (cem >= Dinheiro.Criar(100m)).Should().BeTrue();
    }

    [Fact]
    public void Zero_DeveSerSempreZero()
        => Dinheiro.Zero.Valor.Should().Be(0m);
}
