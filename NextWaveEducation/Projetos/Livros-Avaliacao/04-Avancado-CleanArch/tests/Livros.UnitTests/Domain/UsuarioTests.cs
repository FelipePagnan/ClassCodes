using FluentAssertions;
using Livros.Domain.Entities;
using Livros.Domain.Enums;
using Livros.Domain.Events;
using Livros.Domain.Exceptions;
using Xunit;

namespace Livros.UnitTests.Domain;

public class UsuarioTests
{
    [Fact]
    public void Criar_ComDadosValidos_DevePreencherAsPropriedades()
    {
        var usuario = new Usuario("Ana Souza", "ana.souza@example.com");

        usuario.Nome.Should().Be("Ana Souza");
        usuario.EmailValor.Should().Be("ana.souza@example.com");
        usuario.Avaliacoes.Should().BeEmpty();
        usuario.TotalAvaliacoes.Should().Be(0);
        usuario.NotaMediaAtribuida.Should().BeNull();
    }

    [Fact]
    public void Criar_DeveNormalizarOEmailParaMinusculas()
    {
        var usuario = new Usuario("Ana Souza", "Ana.Souza@EXAMPLE.com");

        usuario.EmailValor.Should().Be("ana.souza@example.com");
    }

    [Fact]
    public void Criar_DeveDispararEventoUsuarioCadastrado()
    {
        var usuario = new Usuario("Ana Souza", "ana@example.com");

        usuario.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UsuarioCadastradoEvent>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("Jo")]
    public void Criar_ComNomeInvalido_DeveLancarExcecao(string nome)
    {
        var acao = () => new Usuario(nome, "ana@example.com");

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*nome*");
    }

    [Theory]
    [InlineData("sem-arroba")]
    [InlineData("")]
    public void Criar_ComEmailInvalido_DeveLancarExcecao(string email)
    {
        var acao = () => new Usuario("Ana Souza", email);

        acao.Should().Throw<RegraNegocioException>();
    }

    [Fact]
    public void Atualizar_DeveTrocarNomeEEmail()
    {
        var usuario = new Usuario("Ana Souza", "ana@example.com");

        usuario.Atualizar("Ana Paula Souza", "ana.paula@example.com");

        usuario.Nome.Should().Be("Ana Paula Souza");
        usuario.EmailValor.Should().Be("ana.paula@example.com");
    }

    // ----- Remoção -----

    [Fact]
    public void GarantirQuePodeSerRemovido_SemAvaliacoes_NaoDeveLancarExcecao()
    {
        var usuario = new Usuario("Ana Souza", "ana@example.com");

        var acao = () => usuario.GarantirQuePodeSerRemovido();

        acao.Should().NotThrow();
    }

    // ----- Estatísticas de leitura -----

    [Fact]
    public void LivrosLidosNoAno_SemAvaliacoes_DeveSerZero()
    {
        var usuario = new Usuario("Ana Souza", "ana@example.com");

        usuario.LivrosLidosNoAno(DateTime.Today.Year).Should().Be(0);
    }

    [Fact]
    public void TotalPaginasLidas_SemAvaliacoes_DeveSerZero()
    {
        var usuario = new Usuario("Ana Souza", "ana@example.com");

        usuario.TotalPaginasLidas.Should().Be(0);
    }

    [Fact]
    public void Email_DeveExporOValueObject()
    {
        var usuario = new Usuario("Ana Souza", "ana@example.com");

        usuario.Email.Valor.Should().Be("ana@example.com");
    }
}
