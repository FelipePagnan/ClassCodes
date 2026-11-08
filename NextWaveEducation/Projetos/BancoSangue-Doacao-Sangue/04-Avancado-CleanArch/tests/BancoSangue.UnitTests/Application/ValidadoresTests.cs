using BancoSangue.Application.Doacoes.Commands.RegistrarDoacao;
using BancoSangue.Application.Doadores.Commands.CadastrarDoador;
using BancoSangue.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace BancoSangue.UnitTests.Application;

public class CadastrarDoadorCommandValidatorTests
{
    private readonly CadastrarDoadorCommandValidator _validator = new();

    private static CadastrarDoadorCommand Valido() => new()
    {
        NomeCompleto = "João da Silva",
        Email = "joao@example.com",
        DataNascimento = new DateTime(1990, 5, 10),
        Genero = Genero.Masculino,
        Peso = 78,
        TipoSanguineo = TipoSanguineo.O,
        FatorRh = FatorRh.Negativo
    };

    [Fact]
    public void ComandoValido_NaoDeveTerErros()
        => _validator.Validate(Valido()).IsValid.Should().BeTrue();

    [Theory]
    [InlineData("")]
    [InlineData("Jo")]
    public void NomeInvalido_DeveFalhar(string nome)
    {
        var resultado = _validator.Validate(Valido() with { NomeCompleto = nome });

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == nameof(CadastrarDoadorCommand.NomeCompleto));
    }

    [Theory]
    [InlineData("")]
    [InlineData("sem-arroba")]
    [InlineData("a@b")]
    public void EmailInvalido_DeveFalhar(string email)
        => _validator.Validate(Valido() with { Email = email }).IsValid.Should().BeFalse();

    [Fact]
    public void DataNascimentoNoFuturo_DeveFalhar()
        => _validator.Validate(Valido() with { DataNascimento = DateTime.Today.AddDays(1) })
            .IsValid.Should().BeFalse();

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(500)]
    public void PesoInvalido_DeveFalhar(double peso)
        => _validator.Validate(Valido() with { Peso = peso }).IsValid.Should().BeFalse();

    [Theory]
    [InlineData("01001-000")]
    [InlineData("01001000")]
    public void CepValido_NaoDeveFalhar(string cep)
        => _validator.Validate(Valido() with { Cep = cep }).IsValid.Should().BeTrue();

    [Theory]
    [InlineData("123")]
    [InlineData("0100100012")]
    public void CepInvalido_DeveFalhar(string cep)
        => _validator.Validate(Valido() with { Cep = cep }).IsValid.Should().BeFalse();

    [Fact]
    public void SemCep_NaoDeveFalhar()
        => _validator.Validate(Valido() with { Cep = null }).IsValid.Should().BeTrue();
}

public class RegistrarDoacaoCommandValidatorTests
{
    private readonly RegistrarDoacaoCommandValidator _validator = new();

    [Theory]
    [InlineData(420)]
    [InlineData(450)]
    [InlineData(470)]
    public void QuantidadeNaFaixa_NaoDeveFalhar(int quantidade)
        => _validator
            .Validate(new RegistrarDoacaoCommand { DoadorId = 1, QuantidadeML = quantidade })
            .IsValid.Should().BeTrue();

    [Theory]
    [InlineData(419)]
    [InlineData(471)]
    [InlineData(0)]
    public void QuantidadeForaDaFaixa_DeveFalhar(int quantidade)
        => _validator
            .Validate(new RegistrarDoacaoCommand { DoadorId = 1, QuantidadeML = quantidade })
            .IsValid.Should().BeFalse();

    [Fact]
    public void DoadorIdZero_DeveFalhar()
        => _validator
            .Validate(new RegistrarDoacaoCommand { DoadorId = 0, QuantidadeML = 450 })
            .IsValid.Should().BeFalse();

    [Fact]
    public void DataNoFuturo_DeveFalhar()
        => _validator
            .Validate(new RegistrarDoacaoCommand
            {
                DoadorId = 1,
                QuantidadeML = 450,
                DataDoacao = DateTime.Now.AddDays(1)
            })
            .IsValid.Should().BeFalse();
}
