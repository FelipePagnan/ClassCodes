using Caixinhas.Application.Objetivos.Commands.CadastrarObjetivo;
using Caixinhas.Application.Objetivos.Queries.SimularEvolucao;
using Caixinhas.Application.Transacoes.Commands.CadastrarTransacao;
using Caixinhas.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Caixinhas.UnitTests.Application;

public class CadastrarObjetivoCommandValidatorTests
{
    private readonly CadastrarObjetivoCommandValidator _validator = new();

    private static CadastrarObjetivoCommand Valido() => new()
    {
        Titulo = "Viagem para o Japão",
        QuantidadeAlvo = 20_000m,
        Prazo = DateTime.Today.AddMonths(18),
        QuantidadeIdealAporteMensal = 1_200m
    };

    [Fact]
    public void ComandoValido_NaoDeveTerErros()
        => _validator.Validate(Valido()).IsValid.Should().BeTrue();

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("ab")]
    public void TituloInvalido_DeveFalhar(string titulo)
        => _validator.Validate(Valido() with { Titulo = titulo }).IsValid.Should().BeFalse();

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void QuantidadeAlvoNaoPositiva_DeveFalhar(decimal valor)
        => _validator.Validate(Valido() with { QuantidadeAlvo = valor })
            .IsValid.Should().BeFalse();

    [Fact]
    public void QuantidadeAlvoComTresCasasDecimais_DeveFalhar()
        => _validator.Validate(Valido() with { QuantidadeAlvo = 1000.555m })
            .IsValid.Should().BeFalse();

    [Theory]
    [InlineData(1000)]
    [InlineData(1000.5)]
    [InlineData(1000.55)]
    public void QuantidadeAlvoComAteDuasCasas_NaoDeveFalhar(decimal valor)
        => _validator.Validate(Valido() with { QuantidadeAlvo = valor })
            .IsValid.Should().BeTrue();

    [Fact]
    public void PrazoNoPassado_DeveFalhar()
        => _validator.Validate(Valido() with { Prazo = DateTime.Today.AddDays(-1) })
            .IsValid.Should().BeFalse();

    [Fact]
    public void PrazoHoje_DeveFalhar()
        => _validator.Validate(Valido() with { Prazo = DateTime.Today })
            .IsValid.Should().BeFalse();

    [Fact]
    public void SemPrazo_NaoDeveFalhar()
        => _validator.Validate(Valido() with { Prazo = null }).IsValid.Should().BeTrue();

    [Fact]
    public void SemAporteIdeal_NaoDeveFalhar()
        => _validator.Validate(Valido() with { QuantidadeIdealAporteMensal = null })
            .IsValid.Should().BeTrue();

    [Fact]
    public void AporteIdealZero_DeveFalhar()
        => _validator.Validate(Valido() with { QuantidadeIdealAporteMensal = 0m })
            .IsValid.Should().BeFalse();
}

public class CadastrarTransacaoCommandValidatorTests
{
    private readonly CadastrarTransacaoCommandValidator _validator = new();

    private static CadastrarTransacaoCommand Valido(decimal quantidade = 1_000m) => new()
    {
        IdObjetivo = 1,
        Quantidade = quantidade,
        Tipo = TipoTransacao.Deposito,
        DataTransacao = DateTime.Today
    };

    // ----- Regra: não pode ser negativa -----

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-100)]
    [InlineData(0)]
    public void QuantidadeNaoPositiva_DeveFalhar(decimal quantidade)
        => _validator.Validate(Valido(quantidade)).IsValid.Should().BeFalse();

    // ----- Regra: até duas casas decimais -----

    [Theory]
    [InlineData(100)]
    [InlineData(100.5)]
    [InlineData(100.55)]
    [InlineData(0.01)]
    public void QuantidadeComAteDuasCasas_NaoDeveFalhar(decimal quantidade)
        => _validator.Validate(Valido(quantidade)).IsValid.Should().BeTrue();

    [Theory]
    [InlineData(100.555)]
    [InlineData(0.001)]
    [InlineData(10.1234)]
    public void QuantidadeComMaisDeDuasCasas_DeveFalhar(decimal quantidade)
        => _validator.Validate(Valido(quantidade)).IsValid.Should().BeFalse();

    [Fact]
    public void IdObjetivoZero_DeveFalhar()
        => _validator.Validate(Valido() with { IdObjetivo = 0 }).IsValid.Should().BeFalse();

    [Fact]
    public void TipoInvalido_DeveFalhar()
        => _validator.Validate(Valido() with { Tipo = (TipoTransacao)99 })
            .IsValid.Should().BeFalse();

    [Fact]
    public void DataNoFuturo_DeveFalhar()
        => _validator.Validate(Valido() with { DataTransacao = DateTime.Today.AddDays(5) })
            .IsValid.Should().BeFalse();

    [Fact]
    public void DataNoPassado_NaoDeveFalhar()
        => _validator.Validate(Valido() with { DataTransacao = DateTime.Today.AddDays(-30) })
            .IsValid.Should().BeTrue();

    [Fact]
    public void SemData_NaoDeveFalhar()
        => _validator.Validate(Valido() with { DataTransacao = null }).IsValid.Should().BeTrue();
}

public class SimularEvolucaoQueryValidatorTests
{
    private readonly SimularEvolucaoQueryValidator _validator = new();

    private static SimularEvolucaoQuery Valida() => new()
    {
        IdObjetivo = 1,
        AporteMensal = 1_000m,
        QuantidadeMeses = 12,
        TaxaJurosMensalPercentual = 0.80m
    };

    [Fact]
    public void QueryValida_NaoDeveTerErros()
        => _validator.Validate(Valida()).IsValid.Should().BeTrue();

    [Fact]
    public void AporteZero_NaoDeveFalhar()
        => _validator.Validate(Valida() with { AporteMensal = 0m }).IsValid.Should().BeTrue();

    [Fact]
    public void AporteNegativo_DeveFalhar()
        => _validator.Validate(Valida() with { AporteMensal = -100m }).IsValid.Should().BeFalse();

    [Theory]
    [InlineData(0)]
    [InlineData(601)]
    public void MesesFora_DeveFalhar(int meses)
        => _validator.Validate(Valida() with { QuantidadeMeses = meses })
            .IsValid.Should().BeFalse();

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void TaxaFora_DeveFalhar(decimal taxa)
        => _validator.Validate(Valida() with { TaxaJurosMensalPercentual = taxa })
            .IsValid.Should().BeFalse();

    [Theory]
    [InlineData(0)]
    [InlineData(0.5)]
    [InlineData(100)]
    public void TaxaNaFaixa_NaoDeveFalhar(decimal taxa)
        => _validator.Validate(Valida() with { TaxaJurosMensalPercentual = taxa })
            .IsValid.Should().BeTrue();
}
