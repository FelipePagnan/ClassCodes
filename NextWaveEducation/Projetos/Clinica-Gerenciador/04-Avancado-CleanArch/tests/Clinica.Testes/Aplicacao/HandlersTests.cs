using Clinica.Aplicacao.Atendimentos;
using Clinica.Aplicacao.Contratos;
using Clinica.Aplicacao.Pacientes;
using Clinica.Aplicacao.Usuarios;
using Clinica.Dominio.Entidades;
using Clinica.Dominio.Enums;
using Clinica.Dominio.Excecoes;
using Clinica.Dominio.ObjetosDeValor;
using Clinica.Dominio.Repositorios;
using Clinica.Testes.Dominio;
using FluentAssertions;
using Moq;
using Xunit;

namespace Clinica.Testes.Aplicacao;

/// <summary>
/// Testes dos handlers com os repositórios dublados.
///
/// Isto só é possível porque os contratos de persistência estão no domínio e
/// os handlers dependem das interfaces: nenhum destes testes abre banco, e
/// por isso rodam em milissegundos.
/// </summary>
public class CadastrarPacienteHandlerTests
{
    private readonly Mock<IPacienteRepositorio> _repositorio = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private CadastrarPacienteHandler CriarHandler()
        => new(_repositorio.Object, _unitOfWork.Object);

    private static CadastrarPaciente ComandoValido() => new(
        "Mariana Lopes Ferreira",
        "529.982.247-25",
        "(11) 98765-4321",
        "mariana@exemplo.com.br",
        new DateTime(1988, 4, 17),
        Sexo.Feminino,
        null);

    [Fact]
    public async Task Handle_ComDadosValidos_DeveAdicionarEComitar()
    {
        _repositorio
            .Setup(r => r.ExisteCpfAsync(It.IsAny<Cpf>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var resultado = await CriarHandler().Handle(ComandoValido(), CancellationToken.None);

        resultado.NomeCompleto.Should().Be("Mariana Lopes Ferreira");
        resultado.Cpf.Should().Be("529.982.247-25");

        _repositorio.Verify(r => r.Adicionar(It.IsAny<Paciente>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ComCpfJaCadastrado_DeveRecusarSemComitar()
    {
        _repositorio
            .Setup(r => r.ExisteCpfAsync(It.IsAny<Cpf>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var acao = async () => await CriarHandler().Handle(
            ComandoValido(), CancellationToken.None);

        await acao.Should().ThrowAsync<RegraNegocioException>()
            .WithMessage("*Já existe um paciente cadastrado com o CPF*");

        // O ponto do teste: nada é gravado quando a regra falha.
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComCpfInvalido_DeveRecusarAntesDeConsultarOBanco()
    {
        var comando = ComandoValido() with { Cpf = "111.111.111-11" };

        var acao = async () => await CriarHandler().Handle(comando, CancellationToken.None);

        await acao.Should().ThrowAsync<RegraNegocioException>();

        _repositorio.Verify(
            r => r.ExisteCpfAsync(It.IsAny<Cpf>(), It.IsAny<int?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

public class RemoverPacienteHandlerTests
{
    private readonly Mock<IPacienteRepositorio> _repositorio = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task Handle_ComAtendimentoEmAberto_DeveRecusar()
    {
        var paciente = Construtor.Paciente();

        _repositorio
            .Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paciente);

        _repositorio
            .Setup(r => r.PossuiAtendimentoAbertoAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new RemoverPacienteHandler(_repositorio.Object, _unitOfWork.Object);

        var acao = async () => await handler.Handle(
            new RemoverPaciente(1), CancellationToken.None);

        await acao.Should().ThrowAsync<RegraNegocioException>()
            .WithMessage("*atendimento em aberto*");

        paciente.EstaDeletado.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_SemAtendimentoEmAberto_DeveFazerRemocaoLogica()
    {
        var paciente = Construtor.Paciente();

        _repositorio
            .Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paciente);

        _repositorio
            .Setup(r => r.PossuiAtendimentoAbertoAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new RemoverPacienteHandler(_repositorio.Object, _unitOfWork.Object);

        await handler.Handle(new RemoverPaciente(1), CancellationToken.None);

        // Remoção lógica: o registro continua lá, só sai das consultas.
        paciente.EstaDeletado.Should().BeTrue();
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ComPacienteInexistente_DeveRecusar()
    {
        _repositorio
            .Setup(r => r.ObterPorIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Paciente?)null);

        var handler = new RemoverPacienteHandler(_repositorio.Object, _unitOfWork.Object);

        var acao = async () => await handler.Handle(
            new RemoverPaciente(99), CancellationToken.None);

        await acao.Should().ThrowAsync<RegraNegocioException>()
            .WithMessage("*Paciente 99 não encontrado*");
    }
}

public class VerificadorDeConflitoDeAgendaTests
{
    private readonly Mock<IAtendimentoRepositorio> _repositorio = new();

    private VerificadorDeConflitoDeAgenda CriarVerificador() => new(_repositorio.Object);

    private void ConfigurarAgenda(params Atendimento[] atendimentos)
        => _repositorio
            .Setup(r => r.ListarDoDiaParaConflitoAsync(
                It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(atendimentos.ToList());

    [Fact]
    public async Task GarantirQueEstaLivre_ComAgendaVazia_DevePassar()
    {
        ConfigurarAgenda();

        var acao = async () => await CriarVerificador().GarantirQueEstaLivreAsync(
            Construtor.ProximoHorarioValido(), 30,
            Construtor.Medico(), Construtor.Paciente());

        await acao.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GarantirQueEstaLivre_ComMedicoOcupado_DeveRecusar()
    {
        var inicio = Construtor.ProximoHorarioValido();
        var medico = Construtor.Medico();
        var outroPaciente = Construtor.Paciente("Bruno Tavares", "111.444.777-35");

        // Médico já atende outro paciente de 9:00 a 9:30.
        var existente = Construtor.Atendimento(inicio, outroPaciente, medico);
        ConfigurarAgenda(existente);

        // Tentativa de encaixar alguém às 9:15 com o mesmo médico.
        var acao = async () => await CriarVerificador().GarantirQueEstaLivreAsync(
            inicio.AddMinutes(15), 30, medico, Construtor.Paciente());

        await acao.Should().ThrowAsync<RegraNegocioException>()
            .WithMessage("*Helena Vasconcelos já tem atendimento*");
    }

    [Fact]
    public async Task GarantirQueEstaLivre_ComPacienteOcupado_DeveRecusar()
    {
        var inicio = Construtor.ProximoHorarioValido();
        var paciente = Construtor.Paciente();
        var outroMedico = Construtor.Medico("Rafael Queiroz", "98765");

        // O paciente já tem consulta com outro médico no mesmo horário —
        // não dá para estar em dois consultórios ao mesmo tempo.
        var existente = Construtor.Atendimento(inicio, paciente, outroMedico);
        ConfigurarAgenda(existente);

        var acao = async () => await CriarVerificador().GarantirQueEstaLivreAsync(
            inicio.AddMinutes(10), 30, Construtor.Medico(), paciente);

        await acao.Should().ThrowAsync<RegraNegocioException>()
            .WithMessage("*Mariana Lopes Ferreira já tem atendimento*");
    }

    [Fact]
    public async Task GarantirQueEstaLivre_ComHorarioEncostado_DevePassar()
    {
        var inicio = Construtor.ProximoHorarioValido();
        var medico = Construtor.Medico();

        // Existente de 9:00 a 9:30; novo começa exatamente às 9:30.
        ConfigurarAgenda(Construtor.Atendimento(
            inicio, Construtor.Paciente("Bruno Tavares", "111.444.777-35"), medico));

        var acao = async () => await CriarVerificador().GarantirQueEstaLivreAsync(
            inicio.AddMinutes(30), 30, medico, Construtor.Paciente());

        await acao.Should().NotThrowAsync();
    }
}

public class AutenticarUsuarioHandlerTests
{
    private readonly Mock<IUsuarioRepositorio> _repositorio = new();
    private readonly Mock<IGeradorDeToken> _gerador = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    public AutenticarUsuarioHandlerTests()
        => _gerador
            .Setup(g => g.Gerar(It.IsAny<Usuario>()))
            .Returns(("token-de-teste", DateTime.UtcNow.AddHours(8)));

    private AutenticarUsuarioHandler CriarHandler()
        => new(_repositorio.Object, _gerador.Object, _unitOfWork.Object);

    private static Usuario UsuarioAdministrador() => new(
        "Administração",
        Email.Criar("admin@clinicavida.com.br"),
        "Clinica123",
        PerfilUsuario.Administrador);

    [Fact]
    public async Task Handle_ComCredenciaisCorretas_DeveDevolverToken()
    {
        _repositorio
            .Setup(r => r.ObterPorEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(UsuarioAdministrador());

        var resultado = await CriarHandler().Handle(
            new AutenticarUsuario("admin@clinicavida.com.br", "Clinica123"),
            CancellationToken.None);

        resultado.Token.Should().Be("token-de-teste");
        resultado.Usuario.Perfil.Should().Be("Administrador");
    }

    [Fact]
    public async Task Handle_ComSenhaErrada_DeveDarAMesmaMensagemDeEmailInexistente()
    {
        _repositorio
            .Setup(r => r.ObterPorEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(UsuarioAdministrador());

        var comSenhaErrada = async () => await CriarHandler().Handle(
            new AutenticarUsuario("admin@clinicavida.com.br", "errada"),
            CancellationToken.None);

        await comSenhaErrada.Should().ThrowAsync<RegraNegocioException>()
            .WithMessage("E-mail ou senha inválidos.");

        _repositorio
            .Setup(r => r.ObterPorEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        var comEmailInexistente = async () => await CriarHandler().Handle(
            new AutenticarUsuario("ninguem@clinicavida.com.br", "Clinica123"),
            CancellationToken.None);

        // A mensagem é idêntica de propósito: dizer qual dos dois está errado
        // entregaria ao atacante quais contas existem.
        await comEmailInexistente.Should().ThrowAsync<RegraNegocioException>()
            .WithMessage("E-mail ou senha inválidos.");
    }

    [Fact]
    public async Task Handle_ComUsuarioInativo_DeveRecusar()
    {
        var usuario = UsuarioAdministrador();
        usuario.Desativar();

        _repositorio
            .Setup(r => r.ObterPorEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var acao = async () => await CriarHandler().Handle(
            new AutenticarUsuario("admin@clinicavida.com.br", "Clinica123"),
            CancellationToken.None);

        await acao.Should().ThrowAsync<RegraNegocioException>().WithMessage("*inativo*");
    }

    [Fact]
    public async Task Handle_ComEmailMalformado_DeveRecusarSemConsultarOBanco()
    {
        var acao = async () => await CriarHandler().Handle(
            new AutenticarUsuario("nao-e-email", "Clinica123"), CancellationToken.None);

        await acao.Should().ThrowAsync<RegraNegocioException>()
            .WithMessage("E-mail ou senha inválidos.");

        _repositorio.Verify(
            r => r.ObterPorEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

public class DesativarUsuarioHandlerTests
{
    private readonly Mock<IUsuarioRepositorio> _repositorio = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private DesativarUsuarioHandler CriarHandler()
        => new(_repositorio.Object, _unitOfWork.Object);

    private static Usuario Administrador() => new(
        "Administração",
        Email.Criar("admin@clinicavida.com.br"),
        "Clinica123",
        PerfilUsuario.Administrador);

    [Fact]
    public async Task Handle_ComOUltimoAdministradorAtivo_DeveRecusar()
    {
        var usuario = Administrador();

        _repositorio
            .Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        _repositorio
            .Setup(r => r.ContarAdministradoresAtivosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var acao = async () => await CriarHandler().Handle(
            new DesativarUsuario(1), CancellationToken.None);

        // Desativar o último administrador deixaria o sistema sem ninguém
        // capaz de reativá-lo.
        await acao.Should().ThrowAsync<RegraNegocioException>()
            .WithMessage("*último administrador*");

        usuario.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ComOutrosAdministradoresAtivos_DeveDesativar()
    {
        var usuario = Administrador();

        _repositorio
            .Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        _repositorio
            .Setup(r => r.ContarAdministradoresAtivosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        await CriarHandler().Handle(new DesativarUsuario(1), CancellationToken.None);

        usuario.Ativo.Should().BeFalse();
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
