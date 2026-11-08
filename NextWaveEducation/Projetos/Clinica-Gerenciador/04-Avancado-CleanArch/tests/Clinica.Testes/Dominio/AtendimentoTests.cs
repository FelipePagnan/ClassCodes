using Clinica.Dominio.Entidades;
using Clinica.Dominio.Enums;
using Clinica.Dominio.Eventos;
using Clinica.Dominio.Excecoes;
using Clinica.Dominio.ObjetosDeValor;
using FluentAssertions;
using Xunit;

namespace Clinica.Testes.Dominio;

/// <summary>
/// Fábrica de entidades para os testes. Centralizar aqui evita repetir
/// CPF válido e horário de funcionamento em cada teste.
/// </summary>
public static class Construtor
{
    /// <summary>Próximo dia útil às 9h — sempre no futuro e dentro do horário.</summary>
    public static DateTime ProximoHorarioValido(int horas = 9, int dias = 1)
    {
        var data = DateTime.Today.AddDays(dias);

        while (data.DayOfWeek == DayOfWeek.Sunday) data = data.AddDays(1);

        return data.AddHours(horas);
    }

    public static Paciente Paciente(
        string nome = "Mariana Lopes Ferreira",
        string cpf = "529.982.247-25",
        string celular = "(11) 98765-4321",
        string? email = "mariana@exemplo.com.br",
        int anosDeIdade = 38)
        => new(
            nome,
            Cpf.Criar(cpf),
            Celular.Criar(celular),
            DateTime.Today.AddYears(-anosDeIdade),
            Sexo.Feminino,
            Email.CriarOpcional(email));

    public static Medico Medico(
        string nome = "Helena Vasconcelos",
        string crm = "123456",
        string uf = "SP")
        => new(nome, Crm.Criar(crm, uf), "Cardiologia");

    public static Servico Servico(
        string nome = "Consulta clínica",
        decimal valor = 250.00m,
        int duracao = 30)
        => new(nome, valor, duracao);

    public static Atendimento Atendimento(
        DateTime? dataHora = null,
        Paciente? paciente = null,
        Medico? medico = null,
        Servico? servico = null)
        => new(
            paciente ?? Paciente(),
            medico ?? Medico(),
            servico ?? Servico(),
            dataHora ?? ProximoHorarioValido());
}

public class AtendimentoConflitoTests
{
    [Fact]
    public void ConflitaCom_QuandoOsHorariosSeSobrepoem_DeveDetectar()
    {
        var inicio = Construtor.ProximoHorarioValido();

        // 9:00–9:30 e 9:15–9:45: há 15 minutos de sobreposição.
        var primeiro = Construtor.Atendimento(inicio);
        var segundo = Construtor.Atendimento(inicio.AddMinutes(15));

        primeiro.ConflitaCom(segundo).Should().BeTrue();

        // A relação é simétrica: se um colide com o outro, o inverso vale.
        segundo.ConflitaCom(primeiro).Should().BeTrue();
    }

    [Fact]
    public void ConflitaCom_QuandoUmComecaExatamenteQuandoOOutroTermina_NaoDeveConflitar()
    {
        var inicio = Construtor.ProximoHorarioValido();

        // 9:00–9:30 e 9:30–10:00: encostam, mas não se sobrepõem. É o caso
        // de borda que importa na prática — é assim que se enfileira a agenda.
        var primeiro = Construtor.Atendimento(inicio);
        var segundo = Construtor.Atendimento(inicio.AddMinutes(30));

        primeiro.ConflitaCom(segundo).Should().BeFalse();
        segundo.ConflitaCom(primeiro).Should().BeFalse();
    }

    [Fact]
    public void ConflitaCom_QuandoUmEstaDentroDoOutro_DeveDetectar()
    {
        var inicio = Construtor.ProximoHorarioValido();

        // 9:00–10:00 contém 9:15–9:35.
        var longo = Construtor.Atendimento(inicio, servico: Construtor.Servico(duracao: 60));
        var curto = Construtor.Atendimento(
            inicio.AddMinutes(15), servico: Construtor.Servico("Rápido", 50m, 20));

        longo.ConflitaCom(curto).Should().BeTrue();
        curto.ConflitaCom(longo).Should().BeTrue();
    }

    [Fact]
    public void ConflitaCom_EmDiasDiferentes_NaoDeveConflitar()
    {
        var primeiro = Construtor.Atendimento(Construtor.ProximoHorarioValido(dias: 1));
        var segundo = Construtor.Atendimento(Construtor.ProximoHorarioValido(dias: 8));

        primeiro.ConflitaCom(segundo).Should().BeFalse();
    }

    [Theory]
    [InlineData(0, true)]    // mesmo horário
    [InlineData(29, true)]   // um minuto antes do fim
    [InlineData(30, false)]  // exatamente no fim
    [InlineData(31, false)]  // depois do fim
    [InlineData(-29, true)]  // começa antes e invade
    [InlineData(-30, false)] // termina exatamente no início
    public void ConflitaComIntervalo_DeveRespeitarAsBordas(int deslocamento, bool esperado)
    {
        var inicio = Construtor.ProximoHorarioValido();
        var atendimento = Construtor.Atendimento(inicio);

        atendimento
            .ConflitaComIntervalo(inicio.AddMinutes(deslocamento), 30)
            .Should().Be(esperado);
    }
}

public class AtendimentoHorarioTests
{
    [Fact]
    public void Criar_ComHorarioNoPassado_DeveRecusar()
    {
        var acao = () => Construtor.Atendimento(DateTime.Now.AddHours(-1));

        acao.Should().Throw<RegraNegocioException>().WithMessage("*já passou*");
    }

    [Fact]
    public void Criar_NoDomingo_DeveRecusar()
    {
        var domingo = DateTime.Today.AddDays(1);
        while (domingo.DayOfWeek != DayOfWeek.Sunday) domingo = domingo.AddDays(1);

        var acao = () => Construtor.Atendimento(domingo.AddHours(10));

        acao.Should().Throw<RegraNegocioException>().WithMessage("*não atende aos domingos*");
    }

    [Fact]
    public void Criar_AntesDaAbertura_DeveRecusar()
    {
        var acao = () => Construtor.Atendimento(Construtor.ProximoHorarioValido(horas: 6));

        acao.Should().Throw<RegraNegocioException>().WithMessage("*abre às*");
    }

    [Fact]
    public void Criar_QuandoTerminariaDepoisDoFechamento_DeveRecusar()
    {
        // 19:50 + 30 minutos = 20:20, e a clínica fecha às 20:00.
        var acao = () => Construtor.Atendimento(
            Construtor.ProximoHorarioValido(horas: 19).AddMinutes(50));

        acao.Should().Throw<RegraNegocioException>().WithMessage("*fecha às*");
    }

    [Fact]
    public void Criar_DeveCopiarDuracaoEValorDoServico()
    {
        var servico = Construtor.Servico("Eletrocardiograma", 180.50m, 25);
        var atendimento = Construtor.Atendimento(servico: servico);

        atendimento.DuracaoMinutos.Should().Be(25);
        atendimento.Valor.Should().Be(180.50m);
        atendimento.DataHoraFim.Should().Be(atendimento.DataHora.AddMinutes(25));
    }

    [Fact]
    public void AlterarServico_DepoisDoAgendamento_NaoDeveMexerNoAtendimento()
    {
        var servico = Construtor.Servico("Consulta", 250m, 30);
        var atendimento = Construtor.Atendimento(servico: servico);

        // A clínica reajusta o preço e o tempo de consulta.
        servico.AlterarDados("Consulta", 300m, 45, null);

        // O que já estava marcado não se mexe: é o motivo de o atendimento
        // guardar a própria cópia.
        atendimento.Valor.Should().Be(250m);
        atendimento.DuracaoMinutos.Should().Be(30);
    }
}

public class AtendimentoStatusTests
{
    [Fact]
    public void Confirmar_QuandoAgendado_DeveConfirmarECarimbar()
    {
        var atendimento = Construtor.Atendimento();

        atendimento.Confirmar();

        atendimento.Status.Should().Be(StatusAtendimento.Confirmado);
        atendimento.ConfirmacaoEnviadaEm.Should().NotBeNull();
        atendimento.EstaAberto.Should().BeTrue();
    }

    [Fact]
    public void Confirmar_DuasVezes_DeveRecusarNaSegunda()
    {
        var atendimento = Construtor.Atendimento();
        atendimento.Confirmar();

        var acao = () => atendimento.Confirmar();

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*Só um atendimento agendado pode ser confirmado*");
    }

    [Fact]
    public void MarcarComoRealizado_AntesDaHora_DeveRecusar()
    {
        var atendimento = Construtor.Atendimento(Construtor.ProximoHorarioValido(dias: 2));

        var acao = () => atendimento.MarcarComoRealizado();

        acao.Should().Throw<RegraNegocioException>().WithMessage("*ainda não aconteceu*");
    }

    [Fact]
    public void Cancelar_QuandoRealizado_DeveRecusar()
    {
        var atendimento = AtendimentoRealizado();

        var acao = () => atendimento.Cancelar("desistiu");

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*realizado não pode ser cancelado*");
    }

    [Fact]
    public void Cancelar_ComMotivo_DeveAnexarOMotivoNasObservacoes()
    {
        var atendimento = Construtor.Atendimento();

        atendimento.Cancelar("paciente viajou");

        atendimento.Status.Should().Be(StatusAtendimento.Cancelado);
        atendimento.Observacoes.Should().Contain("paciente viajou");
        atendimento.EstaEncerrado.Should().BeTrue();
    }

    [Fact]
    public void Remarcar_DeveZerarConfirmacaoELembrete()
    {
        var atendimento = Construtor.Atendimento();
        atendimento.Confirmar();
        atendimento.RegistrarLembreteEnviado();

        atendimento.Remarcar(Construtor.ProximoHorarioValido(horas: 14, dias: 3));

        // O paciente foi avisado de um horário que já não vale: tem que ser
        // avisado de novo.
        atendimento.Status.Should().Be(StatusAtendimento.Agendado);
        atendimento.ConfirmacaoEnviadaEm.Should().BeNull();
        atendimento.LembreteEnviadoEm.Should().BeNull();
    }

    [Fact]
    public void Remarcar_QuandoJaEncerrado_DeveRecusar()
    {
        var atendimento = Construtor.Atendimento();
        atendimento.Cancelar();

        var acao = () => atendimento.Remarcar(Construtor.ProximoHorarioValido(dias: 3));

        acao.Should().Throw<RegraNegocioException>().WithMessage("*não pode ser remarcado*");
    }

    [Fact]
    public void MarcarComoNaoCompareceu_AntesDaHora_DeveRecusar()
    {
        var atendimento = Construtor.Atendimento(Construtor.ProximoHorarioValido(dias: 2));

        var acao = () => atendimento.MarcarComoNaoCompareceu();

        acao.Should().Throw<RegraNegocioException>().WithMessage("*ainda não aconteceu*");
    }

    /// <summary>
    /// Devolve um atendimento já realizado.
    ///
    /// Chegar a esse estado exige que o horário tenha passado, e o construtor
    /// recusa data no passado — então o caminho honesto é agendar para dentro
    /// de um segundo e esperar o relógio virar. Num sistema de produção isso
    /// se resolve injetando TimeProvider na entidade; aqui o custo de um
    /// segundo por teste é menor do que o de abrir o domínio para o teste.
    /// </summary>
    internal static Atendimento AtendimentoRealizado()
    {
        var atendimento = Construtor.Atendimento(DateTime.Now.AddSeconds(1));

        Thread.Sleep(1100);

        atendimento.MarcarComoRealizado();
        return atendimento;
    }
}

public class AtendimentoAnexoTests
{
    [Fact]
    public void AnexarDocumento_QuandoNaoRealizado_DeveRecusar()
    {
        var atendimento = Construtor.Atendimento();

        var acao = () => atendimento.AnexarDocumento(
            TipoAnexo.Atestado, "atestado.txt", "text/plain", new byte[] { 1, 2, 3 });

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*só podem ser adicionados a atendimentos realizados*");
    }

    [Fact]
    public void AnexarDocumento_QuandoRealizado_DeveAnexar()
    {
        var atendimento = AtendimentoStatusTests.AtendimentoRealizado();

        var anexo = atendimento.AnexarDocumento(
            TipoAnexo.Receita,
            "receita.txt",
            "text/plain; charset=utf-8",
            System.Text.Encoding.UTF8.GetBytes("Dipirona 500mg, 1 comprimido a cada 8h"),
            "Receita simples");

        anexo.Tipo.Should().Be(TipoAnexo.Receita);
        anexo.TamanhoBytes.Should().BeGreaterThan(0);
        atendimento.Anexos.Should().ContainSingle();
    }

    [Fact]
    public void AnexarDocumento_ComConteudoVazio_DeveRecusar()
    {
        var atendimento = AtendimentoStatusTests.AtendimentoRealizado();

        var acao = () => atendimento.AnexarDocumento(
            TipoAnexo.Evolucao, "evolucao.txt", "text/plain", Array.Empty<byte>());

        acao.Should().Throw<RegraNegocioException>().WithMessage("*está vazio*");
    }

    [Fact]
    public void AnexarDocumento_AcimaDoTamanhoMaximo_DeveRecusar()
    {
        var atendimento = AtendimentoStatusTests.AtendimentoRealizado();
        var grande = new byte[Anexo.TamanhoMaximoBytes + 1];

        var acao = () => atendimento.AnexarDocumento(
            TipoAnexo.Atestado, "grande.pdf", "application/pdf", grande);

        acao.Should().Throw<RegraNegocioException>().WithMessage("*no máximo*MB*");
    }

    [Fact]
    public void DeletarComAnexos_QuandoRealizado_DeveRecusar()
    {
        var atendimento = AtendimentoStatusTests.AtendimentoRealizado();

        var acao = () => atendimento.DeletarComAnexos();

        acao.Should().Throw<RegraNegocioException>().WithMessage("*histórico clínico*");
    }

    [Fact]
    public void DeletarComAnexos_DeveMarcarOsAnexosComoDeletados()
    {
        var realizado = AtendimentoStatusTests.AtendimentoRealizado();
        realizado.AnexarDocumento(
            TipoAnexo.Atestado, "a.txt", "text/plain", new byte[] { 1 });

        // Cancela para poder remover: realizado não se remove.
        realizado.Cancelar();
        realizado.DeletarComAnexos();

        realizado.EstaDeletado.Should().BeTrue();
        realizado.Anexos.Should().OnlyContain(a => a.EstaDeletado);
    }
}

public class AtendimentoEventosTests
{
    [Fact]
    public void Criar_DeveRegistrarEventoDeAgendamento()
    {
        var atendimento = Construtor.Atendimento();

        atendimento.Eventos.Should().ContainSingle()
            .Which.Should().BeOfType<AtendimentoAgendado>();
    }

    [Fact]
    public void Confirmar_DeveRegistrarEventoDeConfirmacao()
    {
        var atendimento = Construtor.Atendimento();
        atendimento.LimparEventos();

        atendimento.Confirmar();

        atendimento.Eventos.Should().ContainSingle()
            .Which.Should().BeOfType<AtendimentoConfirmado>();
    }

    [Fact]
    public void Remarcar_DeveRegistrarEventoComADataAnterior()
    {
        var original = Construtor.ProximoHorarioValido();
        var atendimento = Construtor.Atendimento(original);
        atendimento.LimparEventos();

        var nova = Construtor.ProximoHorarioValido(horas: 15, dias: 4);
        atendimento.Remarcar(nova);

        var evento = atendimento.Eventos.Should().ContainSingle()
            .Which.Should().BeOfType<AtendimentoRemarcado>().Subject;

        evento.DataAnterior.Should().Be(original);
        evento.Atendimento.DataHora.Should().Be(nova);
    }

    [Fact]
    public void Cancelar_DeveRegistrarEventoComOMotivo()
    {
        var atendimento = Construtor.Atendimento();
        atendimento.LimparEventos();

        atendimento.Cancelar("paciente desmarcou");

        atendimento.Eventos.Should().ContainSingle()
            .Which.Should().BeOfType<AtendimentoCancelado>()
            .Which.Motivo.Should().Be("paciente desmarcou");
    }

    [Fact]
    public void LimparEventos_DeveEsvaziarALista()
    {
        var atendimento = Construtor.Atendimento();
        atendimento.Confirmar();

        atendimento.Eventos.Should().HaveCount(2);

        atendimento.LimparEventos();

        // É o que impede o Unit of Work de publicar o mesmo evento duas vezes
        // quando há dois commits no mesmo escopo.
        atendimento.Eventos.Should().BeEmpty();
    }
}
