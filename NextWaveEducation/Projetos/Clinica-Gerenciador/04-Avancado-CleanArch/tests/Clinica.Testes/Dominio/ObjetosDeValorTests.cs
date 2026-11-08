using Clinica.Dominio.Entidades;
using Clinica.Dominio.Enums;
using Clinica.Dominio.Excecoes;
using Clinica.Dominio.ObjetosDeValor;
using Clinica.Dominio.Servicos;
using FluentAssertions;
using Xunit;

namespace Clinica.Testes.Dominio;

public class CpfTests
{
    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    [InlineData("111.444.777-35")]
    [InlineData("168.995.350-09")]
    public void Criar_ComCpfValido_DeveAceitarEGuardarSoOsDigitos(string entrada)
    {
        var cpf = Cpf.Criar(entrada);

        cpf.Valor.Should().HaveLength(11).And.MatchRegex("^[0-9]+$");
    }

    [Theory]
    [InlineData("529.982.247-26")]   // último dígito errado
    [InlineData("111.444.777-30")]   // dígitos verificadores errados
    [InlineData("111.111.111-11")]   // todos iguais
    [InlineData("000.000.000-00")]
    [InlineData("1234567890")]       // 10 dígitos
    [InlineData("123456789012")]     // 12 dígitos
    [InlineData("abc")]
    [InlineData("")]
    [InlineData((string?)null)]
    public void Criar_ComCpfInvalido_DeveRecusar(string? entrada)
    {
        var acao = () => Cpf.Criar(entrada);

        acao.Should().Throw<RegraNegocioException>();
    }

    [Fact]
    public void Formatado_DeveDevolverComPontuacao()
    {
        Cpf.Criar("52998224725").Formatado.Should().Be("529.982.247-25");
    }

    [Fact]
    public void Igualdade_DeveSerPorValor()
    {
        // É record: dois CPFs com os mesmos dígitos são o mesmo valor,
        // mesmo tendo sido criados a partir de textos diferentes.
        Cpf.Criar("529.982.247-25").Should().Be(Cpf.Criar("52998224725"));
    }
}

public class CelularTests
{
    [Theory]
    [InlineData("(11) 98765-4321")]
    [InlineData("11987654321")]
    [InlineData("21 9 1234-5678")]
    public void Criar_ComCelularValido_DeveAceitar(string entrada)
    {
        Celular.Criar(entrada).Valor.Should().HaveLength(11);
    }

    [Theory]
    [InlineData("1187654321")]    // 10 dígitos (fixo, não celular)
    [InlineData("11887654321")]   // terceiro dígito não é 9
    [InlineData("00987654321")]   // DDD inexistente
    [InlineData("23987654321")]   // DDD inexistente
    [InlineData("")]
    [InlineData((string?)null)]
    public void Criar_ComCelularInvalido_DeveRecusar(string? entrada)
    {
        var acao = () => Celular.Criar(entrada);

        acao.Should().Throw<RegraNegocioException>();
    }

    [Fact]
    public void Formatado_DeveDevolverComMascara()
    {
        Celular.Criar("11987654321").Formatado.Should().Be("(11) 98765-4321");
    }

    [Fact]
    public void CriarOpcional_SemEntrada_DeveDevolverNulo()
    {
        Celular.CriarOpcional(null).Should().BeNull();
        Celular.CriarOpcional("  ").Should().BeNull();
    }

    [Fact]
    public void Ddd_DeveDevolverOsDoisPrimeirosDigitos()
    {
        Celular.Criar("(21) 91234-5678").Ddd.Should().Be("21");
    }
}

public class EmailTests
{
    [Theory]
    [InlineData("maria@exemplo.com.br")]
    [InlineData("a.b+c@dominio.co")]
    public void Criar_ComEmailValido_DeveAceitar(string entrada)
    {
        Email.Criar(entrada).Valor.Should().Be(entrada.ToLowerInvariant());
    }

    [Fact]
    public void Criar_DeveNormalizarParaMinusculas()
    {
        // Evita cadastro duplicado por diferença de caixa.
        Email.Criar("  Maria@Exemplo.COM  ").Valor.Should().Be("maria@exemplo.com");
    }

    [Theory]
    [InlineData("sem-arroba")]
    [InlineData("@sem-usuario.com")]
    [InlineData("sem-dominio@")]
    [InlineData("com espaco@exemplo.com")]
    [InlineData("duplo@@exemplo.com")]
    [InlineData("sem-ponto@dominio")]
    [InlineData("")]
    [InlineData((string?)null)]
    public void Criar_ComEmailInvalido_DeveRecusar(string? entrada)
    {
        var acao = () => Email.Criar(entrada);

        acao.Should().Throw<RegraNegocioException>();
    }
}

public class CrmTests
{
    [Fact]
    public void Criar_DeveNormalizarUfParaMaiusculas()
    {
        var crm = Crm.Criar("123456", "sp");

        crm.Uf.Should().Be("SP");
        crm.Completo.Should().Be("CRM/SP 123456");
    }

    [Theory]
    [InlineData("123", "SP")]      // menos de 4 dígitos
    [InlineData("12345678", "SP")] // mais de 7 dígitos
    [InlineData("123456", "XX")]   // UF inexistente
    [InlineData("123456", "")]
    [InlineData("", "SP")]
    public void Criar_ComDadosInvalidos_DeveRecusar(string crm, string uf)
    {
        var acao = () => Crm.Criar(crm, uf);

        acao.Should().Throw<RegraNegocioException>();
    }

    [Fact]
    public void Igualdade_MesmoNumeroEmUfDiferente_DeveSerDiferente()
    {
        // O CRM só é único dentro do estado: existe CRM/SP 123456 e
        // CRM/RJ 123456, de dois médicos diferentes.
        Crm.Criar("123456", "SP").Should().NotBe(Crm.Criar("123456", "RJ"));
    }
}

public class SenhasTests
{
    [Fact]
    public void Gerar_DeveProduzirHashESaltDiferentesParaAMesmaSenha()
    {
        var primeira = Senhas.Gerar("Clinica123");
        var segunda = Senhas.Gerar("Clinica123");

        // Cada senha recebe salt próprio, então o hash difere. É o que
        // impede descobrir, olhando o banco, quem usa a mesma senha.
        primeira.Hash.Should().NotBe(segunda.Hash);
        primeira.Salt.Should().NotBe(segunda.Salt);
    }

    [Fact]
    public void Conferir_ComASenhaCorreta_DeveAceitar()
    {
        var (hash, salt) = Senhas.Gerar("Clinica123");

        Senhas.Conferir("Clinica123", hash, salt).Should().BeTrue();
    }

    [Theory]
    [InlineData("clinica123")]   // caixa diferente
    [InlineData("Clinica124")]
    [InlineData("")]
    public void Conferir_ComSenhaErrada_DeveRecusar(string tentativa)
    {
        var (hash, salt) = Senhas.Gerar("Clinica123");

        Senhas.Conferir(tentativa, hash, salt).Should().BeFalse();
    }

    [Fact]
    public void Conferir_ComHashCorrompido_DeveRecusarSemExplodir()
    {
        Senhas.Conferir("Clinica123", "nao-e-base64!!", "nem-isso!!").Should().BeFalse();
    }

    [Theory]
    [InlineData("Clinica123", true)]
    [InlineData("abc12345", true)]
    [InlineData("curta1", false)]      // menos de 8
    [InlineData("semnumeros", false)]
    [InlineData("12345678", false)]    // sem letra
    [InlineData("", false)]
    [InlineData((string?)null, false)]
    public void SenhaForte_DeveExigirOitoCaracteresComLetraENumero(
        string? senha, bool esperado)
    {
        Senhas.SenhaForte(senha, out var motivo).Should().Be(esperado);

        if (!esperado) motivo.Should().NotBeEmpty();
    }
}

public class DocumentosTests
{
    [Theory]
    [InlineData("SP", true)]
    [InlineData("sp", true)]
    [InlineData("RJ", true)]
    [InlineData("DF", true)]
    [InlineData("XX", false)]
    [InlineData("S", false)]
    [InlineData("", false)]
    public void UfValida_DeveAceitarAsVinteESeteUnidadesFederativas(string uf, bool esperado)
    {
        Documentos.UfValida(uf).Should().Be(esperado);
    }

    [Fact]
    public void Ufs_DeveTerVinteESeteSiglas()
    {
        Documentos.Ufs.Should().HaveCount(27);
    }

    [Fact]
    public void SomenteDigitos_DeveRemoverPontuacaoEEspacos()
    {
        Documentos.SomenteDigitos(" (11) 98765-4321 ").Should().Be("11987654321");
    }
}

public class UsuarioTests
{
    private static Medico Medico() => Construtor.Medico();

    [Fact]
    public void Criar_ComPerfilMedicoSemVinculo_DeveRecusar()
    {
        var acao = () => new Usuario(
            "Helena", Email.Criar("helena@clinica.com"), "Clinica123", PerfilUsuario.Medico);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*precisa estar vinculado a um cadastro de médico*");
    }

    [Fact]
    public void Criar_ComPerfilNaoMedicoEVinculo_DeveRecusar()
    {
        var acao = () => new Usuario(
            "Recepção",
            Email.Criar("recepcao@clinica.com"),
            "Clinica123",
            PerfilUsuario.Recepcionista,
            Medico());

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*Só o perfil Médico aceita vínculo*");
    }

    [Fact]
    public void Criar_NuncaDeveGuardarASenhaEmTextoClaro()
    {
        var usuario = new Usuario(
            "Admin", Email.Criar("admin@clinica.com"), "Clinica123",
            PerfilUsuario.Administrador);

        usuario.SenhaHash.Should().NotContain("Clinica123");
        usuario.SenhaSalt.Should().NotBeEmpty();
        usuario.ConferirSenha("Clinica123").Should().BeTrue();
    }

    [Fact]
    public void Criar_ComSenhaFraca_DeveRecusar()
    {
        var acao = () => new Usuario(
            "Admin", Email.Criar("admin@clinica.com"), "123",
            PerfilUsuario.Administrador);

        acao.Should().Throw<RegraNegocioException>().WithMessage("*8 caracteres*");
    }

    [Fact]
    public void AlterarSenha_ComSenhaAtualErrada_DeveRecusar()
    {
        var usuario = new Usuario(
            "Admin", Email.Criar("admin@clinica.com"), "Clinica123",
            PerfilUsuario.Administrador);

        var acao = () => usuario.AlterarSenha("errada", "NovaSenha123");

        acao.Should().Throw<RegraNegocioException>().WithMessage("*não confere*");
    }

    [Fact]
    public void AlterarSenha_ComAMesmaSenha_DeveRecusar()
    {
        var usuario = new Usuario(
            "Admin", Email.Criar("admin@clinica.com"), "Clinica123",
            PerfilUsuario.Administrador);

        var acao = () => usuario.AlterarSenha("Clinica123", "Clinica123");

        acao.Should().Throw<RegraNegocioException>().WithMessage("*diferente da atual*");
    }

    [Fact]
    public void AlterarSenha_ComDadosCorretos_DeveTrocar()
    {
        var usuario = new Usuario(
            "Admin", Email.Criar("admin@clinica.com"), "Clinica123",
            PerfilUsuario.Administrador);

        usuario.AlterarSenha("Clinica123", "NovaSenha456");

        usuario.ConferirSenha("NovaSenha456").Should().BeTrue();
        usuario.ConferirSenha("Clinica123").Should().BeFalse();
    }
}

public class PacienteTests
{
    [Fact]
    public void Idade_DeveDescontarAniversarioQueAindaNaoChegou()
    {
        var paciente = new Paciente(
            "Teste de Idade",
            Cpf.Criar("52998224725"),
            Celular.Criar("11987654321"),
            DateTime.Today.AddYears(-30).AddDays(1),
            Sexo.Masculino);

        // Faz 30 amanhã, então hoje tem 29.
        paciente.Idade.Should().Be(29);
    }

    [Fact]
    public void EhMenorDeIdade_ComMenosDeDezoito_DeveSerVerdadeiro()
    {
        var paciente = new Paciente(
            "Lívia Tavares",
            Cpf.Criar("16899535009"),
            Celular.Criar("11912345678"),
            DateTime.Today.AddYears(-9),
            Sexo.Feminino);

        paciente.EhMenorDeIdade.Should().BeTrue();
    }

    [Fact]
    public void Criar_ComDataDeNascimentoNoFuturo_DeveRecusar()
    {
        var acao = () => new Paciente(
            "Teste",
            Cpf.Criar("52998224725"),
            Celular.Criar("11987654321"),
            DateTime.Today.AddDays(1),
            Sexo.Outro);

        acao.Should().Throw<RegraNegocioException>().WithMessage("*futuro*");
    }

    [Fact]
    public void PrimeiroNome_DeveDevolverSoOPrimeiro()
    {
        Construtor.Paciente("Mariana Lopes Ferreira").PrimeiroNome.Should().Be("Mariana");
    }
}

public class ServicoTests
{
    [Theory]
    [InlineData(250.00)]
    [InlineData(250.5)]
    [InlineData(0)]
    public void Criar_ComValorDeAteDuasCasas_DeveAceitar(decimal valor)
    {
        new Servico("Consulta", valor, 30).Valor.Should().Be(valor);
    }

    [Fact]
    public void Criar_ComMaisDeDuasCasasDecimais_DeveRecusar()
    {
        var acao = () => new Servico("Consulta", 250.005m, 30);

        acao.Should().Throw<RegraNegocioException>().WithMessage("*2 casas decimais*");
    }

    [Fact]
    public void Criar_ComValorDeEscalaMaiorMasMesmoValor_DeveAceitar()
    {
        // 250.500 é igual a 250.50: a comparação é por valor, não por escala.
        new Servico("Consulta", 250.500m, 30).Valor.Should().Be(250.50m);
    }

    [Fact]
    public void Criar_ComValorNegativo_DeveRecusar()
    {
        var acao = () => new Servico("Consulta", -1m, 30);

        acao.Should().Throw<RegraNegocioException>().WithMessage("*negativo*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(481)]
    public void Criar_ComDuracaoInvalida_DeveRecusar(int duracao)
    {
        var acao = () => new Servico("Consulta", 250m, duracao);

        acao.Should().Throw<RegraNegocioException>();
    }

    [Fact]
    public void Duracao_DeveRefletirOsMinutos()
    {
        new Servico("Consulta", 250m, 45).Duracao.Should().Be(TimeSpan.FromMinutes(45));
    }
}
