using BancoSangue.Domain.Entities;
using BancoSangue.Domain.Enums;
using BancoSangue.Domain.Events;
using BancoSangue.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace BancoSangue.UnitTests.Domain;

public class DoadorTests
{
    private static Doador CriarDoador(
        Genero genero = Genero.Masculino,
        double peso = 70,
        int idade = 30,
        string email = "teste@example.com")
        => new(
            "Fulano de Teste",
            email,
            DateTime.Today.AddYears(-idade),
            genero,
            peso,
            TipoSanguineo.O,
            FatorRh.Negativo);

    // ----- Criação e validação de dados -----

    [Fact]
    public void Criar_ComDadosValidos_DevePreencherAsPropriedades()
    {
        var doador = CriarDoador();

        doador.NomeCompleto.Should().Be("Fulano de Teste");
        doador.EmailValor.Should().Be("teste@example.com");
        doador.TipoSanguineoCompleto.Should().Be("O-");
        doador.Doacoes.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("ab")]
    public void Criar_ComNomeInvalido_DeveLancarExcecao(string nome)
    {
        var acao = () => new Doador(
            nome, "a@b.com", DateTime.Today.AddYears(-30),
            Genero.Masculino, 70, TipoSanguineo.A, FatorRh.Positivo);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*nome completo*");
    }

    [Theory]
    [InlineData("sem-arroba")]
    [InlineData("sem@dominio")]
    [InlineData("")]
    public void Criar_ComEmailInvalido_DeveLancarExcecao(string email)
    {
        var acao = () => CriarDoador(email: email);

        acao.Should().Throw<RegraNegocioException>();
    }

    [Fact]
    public void Criar_ComEmailEmMaiusculas_DeveNormalizarParaMinusculas()
    {
        var doador = CriarDoador(email: "TESTE@Example.COM");

        doador.EmailValor.Should().Be("teste@example.com");
    }

    [Fact]
    public void Criar_ComPesoZeroOuNegativo_DeveLancarExcecao()
    {
        var acao = () => CriarDoador(peso: 0);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*peso*");
    }

    [Fact]
    public void Criar_DeveDispararEventoDoadorCadastrado()
    {
        var doador = CriarDoador();

        doador.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<DoadorCadastradoEvent>();
    }

    // ----- Regra: menor de idade pode ter cadastro, mas não pode doar -----

    [Fact]
    public void Criar_MenorDeIdade_DevePermitirCadastro()
    {
        var acao = () => CriarDoador(idade: 16);

        acao.Should().NotThrow();
    }

    [Fact]
    public void GarantirQuePodeDoar_MenorDeIdade_DeveLancarExcecao()
    {
        var doador = CriarDoador(idade: 17);

        var acao = () => doador.GarantirQuePodeDoar(DateTime.Now);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*Menores de idade não podem doar*");
    }

    [Fact]
    public void GarantirQuePodeDoar_ComDezoitoAnos_NaoDeveLancarExcecao()
    {
        var doador = CriarDoador(idade: 18);

        var acao = () => doador.GarantirQuePodeDoar(DateTime.Now);

        acao.Should().NotThrow();
    }

    // ----- Regra: peso mínimo de 50kg -----

    [Theory]
    [InlineData(49.9)]
    [InlineData(45)]
    [InlineData(30)]
    public void GarantirQuePodeDoar_ComPesoAbaixoDoMinimo_DeveLancarExcecao(double peso)
    {
        var doador = CriarDoador(peso: peso);

        var acao = () => doador.GarantirQuePodeDoar(DateTime.Now);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*peso mínimo*");
    }

    [Theory]
    [InlineData(50)]
    [InlineData(50.1)]
    [InlineData(90)]
    public void GarantirQuePodeDoar_ComPesoNoMinimoOuAcima_NaoDeveLancarExcecao(double peso)
    {
        var doador = CriarDoador(peso: peso);

        var acao = () => doador.GarantirQuePodeDoar(DateTime.Now);

        acao.Should().NotThrow();
    }

    // ----- Regra: intervalo entre doações (60 dias homens / 90 dias mulheres) -----

    [Fact]
    public void IntervaloMinimoDias_Homem_DeveSerSessenta()
        => CriarDoador(Genero.Masculino).IntervaloMinimoDias.Should().Be(60);

    [Fact]
    public void IntervaloMinimoDias_Mulher_DeveSerNoventa()
        => CriarDoador(Genero.Feminino).IntervaloMinimoDias.Should().Be(90);

    [Fact]
    public void IntervaloMinimoDias_Outro_DeveUsarSessentaDias()
        => CriarDoador(Genero.Outro).IntervaloMinimoDias.Should().Be(60);

    [Theory]
    [InlineData(59)]
    [InlineData(30)]
    [InlineData(1)]
    public void RegistrarDoacao_HomemAntesDeSessentaDias_DeveLancarExcecao(int diasDesdeUltima)
    {
        var doador = CriarDoador(Genero.Masculino);
        doador.RegistrarDoacao(450, DateTime.Now.AddDays(-diasDesdeUltima));

        var acao = () => doador.RegistrarDoacao(450, DateTime.Now);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*intervalo mínimo de 60 dias*");
    }

    [Fact]
    public void RegistrarDoacao_HomemApos60Dias_DevePermitir()
    {
        var doador = CriarDoador(Genero.Masculino);
        doador.RegistrarDoacao(450, DateTime.Now.AddDays(-60));

        var acao = () => doador.RegistrarDoacao(450, DateTime.Now);

        acao.Should().NotThrow();
        doador.Doacoes.Should().HaveCount(2);
    }

    [Theory]
    [InlineData(89)]
    [InlineData(60)]
    public void RegistrarDoacao_MulherAntesDeNoventaDias_DeveLancarExcecao(int diasDesdeUltima)
    {
        var doadora = CriarDoador(Genero.Feminino);
        doadora.RegistrarDoacao(450, DateTime.Now.AddDays(-diasDesdeUltima));

        var acao = () => doadora.RegistrarDoacao(450, DateTime.Now);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*intervalo mínimo de 90 dias*");
    }

    [Fact]
    public void RegistrarDoacao_MulherApos90Dias_DevePermitir()
    {
        var doadora = CriarDoador(Genero.Feminino);
        doadora.RegistrarDoacao(450, DateTime.Now.AddDays(-90));

        var acao = () => doadora.RegistrarDoacao(450, DateTime.Now);

        acao.Should().NotThrow();
    }

    // ----- Registro de doação e eventos -----

    [Fact]
    public void RegistrarDoacao_DeveAdicionarNaColecaoEDispararEvento()
    {
        var doador = CriarDoador();

        var doacao = doador.RegistrarDoacao(450, DateTime.Now);

        doacao.QuantidadeML.Should().Be(450);
        doador.Doacoes.Should().ContainSingle();
        doador.DomainEvents.Should().ContainItemsAssignableTo<DoacaoRegistradaEvent>();
    }

    [Fact]
    public void UltimaDoacao_ComVariasDoacoes_DeveRetornarAMaisRecente()
    {
        var doador = CriarDoador();
        var antiga = DateTime.Now.AddDays(-200);
        var recente = DateTime.Now.AddDays(-10);

        doador.RegistrarDoacao(450, antiga);
        doador.RegistrarDoacao(430, recente);

        doador.UltimaDoacao.Should().BeCloseTo(recente, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ProximaDoacaoPermitida_DeveSerUltimaDoacaoMaisIntervalo()
    {
        var doador = CriarDoador(Genero.Feminino);
        var dataDoacao = DateTime.Now.AddDays(-10);

        doador.RegistrarDoacao(450, dataDoacao);

        doador.ProximaDoacaoPermitida.Should()
            .BeCloseTo(dataDoacao.AddDays(90), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void EstaAptoParaDoar_MaiorDeIdadeComPesoSuficiente_DeveSerTrue()
        => CriarDoador(idade: 25, peso: 70).EstaAptoParaDoar.Should().BeTrue();

    [Fact]
    public void EstaAptoParaDoar_MenorDeIdade_DeveSerFalse()
        => CriarDoador(idade: 15, peso: 70).EstaAptoParaDoar.Should().BeFalse();

    [Fact]
    public void EstaAptoParaDoar_PesoAbaixoDoMinimo_DeveSerFalse()
        => CriarDoador(idade: 25, peso: 48).EstaAptoParaDoar.Should().BeFalse();

    // ----- Endereço -----

    [Fact]
    public void DefinirEndereco_DeveVincularOsDoisLados()
    {
        var doador = CriarDoador();
        var endereco = new Endereco("Praça da Sé", "São Paulo", "sp", "01001000");

        doador.DefinirEndereco(endereco);

        doador.Endereco.Should().BeSameAs(endereco);
        endereco.Doador.Should().BeSameAs(doador);
        endereco.Estado.Should().Be("SP");
        endereco.CEP.Should().Be("01001-000");
    }
}
