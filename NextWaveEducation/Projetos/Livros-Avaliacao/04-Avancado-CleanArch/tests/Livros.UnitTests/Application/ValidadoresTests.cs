using FluentAssertions;
using Livros.Application.Avaliacoes.Commands.CadastrarAvaliacao;
using Livros.Application.Livros.Commands.CadastrarLivro;
using Livros.Application.Usuarios.Commands.CadastrarUsuario;
using Livros.Domain.Enums;
using Xunit;

namespace Livros.UnitTests.Application;

public class CadastrarLivroCommandValidatorTests
{
    private readonly CadastrarLivroCommandValidator _validator = new();

    private static CadastrarLivroCommand Valido() => new()
    {
        Titulo = "Domain-Driven Design",
        Descricao = "Tackling complexity in the heart of software.",
        Isbn = "978-0321125217",
        Autor = "Eric Evans",
        Editora = "Addison-Wesley",
        Genero = GeneroLivroEnum.Tecnico,
        AnoDePublicacao = 2003,
        QuantidadePaginas = 560
    };

    [Fact]
    public void ComandoValido_NaoDeveTerErros()
        => _validator.Validate(Valido()).IsValid.Should().BeTrue();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void TituloVazio_DeveFalhar(string titulo)
        => _validator.Validate(Valido() with { Titulo = titulo }).IsValid.Should().BeFalse();

    [Theory]
    [InlineData("123")]
    [InlineData("12345678901")]
    [InlineData("")]
    public void IsbnInvalido_DeveFalhar(string isbn)
        => _validator.Validate(Valido() with { Isbn = isbn }).IsValid.Should().BeFalse();

    [Theory]
    [InlineData("978-0321125217")]
    [InlineData("9780321125217")]
    [InlineData("0321125217")]
    public void IsbnValido_NaoDeveFalhar(string isbn)
        => _validator.Validate(Valido() with { Isbn = isbn }).IsValid.Should().BeTrue();

    [Fact]
    public void AutorVazio_DeveFalhar()
        => _validator.Validate(Valido() with { Autor = "" }).IsValid.Should().BeFalse();

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(60_000)]
    public void PaginasInvalidas_DeveFalhar(int paginas)
        => _validator.Validate(Valido() with { QuantidadePaginas = paginas })
            .IsValid.Should().BeFalse();

    [Theory]
    [InlineData(1400)]
    [InlineData(3000)]
    public void AnoDePublicacaoFora_DeveFalhar(int ano)
        => _validator.Validate(Valido() with { AnoDePublicacao = ano })
            .IsValid.Should().BeFalse();

    [Fact]
    public void GeneroInvalido_DeveFalhar()
        => _validator.Validate(Valido() with { Genero = (GeneroLivroEnum)999 })
            .IsValid.Should().BeFalse();
}

public class CadastrarAvaliacaoCommandValidatorTests
{
    private readonly CadastrarAvaliacaoCommandValidator _validator = new();

    private static CadastrarAvaliacaoCommand Valido(int nota = 4) => new()
    {
        IdLivro = 1,
        IdUsuario = 1,
        Nota = nota,
        Descricao = "Bom livro.",
        DataInicioLeitura = DateTime.Today.AddDays(-30),
        DataFimLeitura = DateTime.Today.AddDays(-10)
    };

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void NotaNaFaixa_NaoDeveFalhar(int nota)
        => _validator.Validate(Valido(nota)).IsValid.Should().BeTrue();

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    [InlineData(100)]
    public void NotaForaDaFaixa_DeveFalhar(int nota)
        => _validator.Validate(Valido(nota)).IsValid.Should().BeFalse();

    [Fact]
    public void IdUsuarioZero_DeveFalhar()
        => _validator.Validate(Valido() with { IdUsuario = 0 }).IsValid.Should().BeFalse();

    [Fact]
    public void IdLivroZero_DeveFalhar()
        => _validator.Validate(Valido() with { IdLivro = 0 }).IsValid.Should().BeFalse();

    [Fact]
    public void InicioMaiorQueOFim_DeveFalhar()
    {
        var comando = Valido() with
        {
            DataInicioLeitura = DateTime.Today.AddDays(-5),
            DataFimLeitura = DateTime.Today.AddDays(-20)
        };

        _validator.Validate(comando).IsValid.Should().BeFalse();
    }

    [Fact]
    public void DatasIguais_NaoDeveFalhar()
    {
        var ontem = DateTime.Today.AddDays(-1);
        var comando = Valido() with { DataInicioLeitura = ontem, DataFimLeitura = ontem };

        _validator.Validate(comando).IsValid.Should().BeTrue();
    }

    [Fact]
    public void ComentarioMuitoLongo_DeveFalhar()
        => _validator.Validate(Valido() with { Descricao = new string('a', 2001) })
            .IsValid.Should().BeFalse();
}

public class CadastrarUsuarioCommandValidatorTests
{
    private readonly CadastrarUsuarioCommandValidator _validator = new();

    private static CadastrarUsuarioCommand Valido() => new()
    {
        Nome = "Ana Souza",
        Email = "ana.souza@example.com"
    };

    [Fact]
    public void ComandoValido_NaoDeveTerErros()
        => _validator.Validate(Valido()).IsValid.Should().BeTrue();

    [Theory]
    [InlineData("")]
    [InlineData("Jo")]
    public void NomeInvalido_DeveFalhar(string nome)
        => _validator.Validate(Valido() with { Nome = nome }).IsValid.Should().BeFalse();

    [Theory]
    [InlineData("")]
    [InlineData("sem-arroba")]
    [InlineData("a@b")]
    public void EmailInvalido_DeveFalhar(string email)
        => _validator.Validate(Valido() with { Email = email }).IsValid.Should().BeFalse();
}
