using Caixinhas.Domain.Entities;
using Caixinhas.Domain.Enums;
using Caixinhas.Domain.Events;
using Caixinhas.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Caixinhas.UnitTests.Domain;

public class ObjetivoFinanceiroTests
{
    private static ObjetivoFinanceiro CriarObjetivo(
        string titulo = "Viagem para o Japão",
        decimal meta = 10_000m,
        DateTime? prazo = null,
        decimal? aporteIdeal = null)
        => new(titulo, meta, prazo, aporteIdeal);

    // ----- Criação e validação de dados -----

    [Fact]
    public void Criar_ComDadosValidos_DevePreencherAsPropriedades()
    {
        var objetivo = CriarObjetivo();

        objetivo.Titulo.Should().Be("Viagem para o Japão");
        objetivo.QuantidadeAlvo.Should().Be(10_000m);
        objetivo.ValorTotal.Should().Be(0m);
        objetivo.Status.Should().Be(StatusObjetivo.EmProgresso);
        objetivo.EstaDeletado.Should().BeFalse();
        objetivo.Transacoes.Should().BeEmpty();
        objetivo.TemCapa.Should().BeFalse();
    }

    [Fact]
    public void Criar_DeveDispararEventoObjetivoCriado()
    {
        var objetivo = CriarObjetivo();

        objetivo.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ObjetivoCriadoEvent>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("ab")]
    public void Criar_ComTituloInvalido_DeveLancarExcecao(string titulo)
    {
        var acao = () => CriarObjetivo(titulo: titulo);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*título*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Criar_ComMetaInvalida_DeveLancarExcecao(decimal meta)
    {
        var acao = () => CriarObjetivo(meta: meta);

        acao.Should().Throw<RegraNegocioException>();
    }

    [Fact]
    public void Criar_ComMetaDeTresCasasDecimais_DeveLancarExcecao()
    {
        var acao = () => CriarObjetivo(meta: 1000.555m);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*casas decimais*");
    }

    [Fact]
    public void Criar_ComPrazoNoPassado_DeveLancarExcecao()
    {
        var acao = () => CriarObjetivo(prazo: DateTime.Today.AddDays(-1));

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*data futura*");
    }

    [Fact]
    public void Criar_ComPrazoFuturo_DevePermitir()
    {
        var objetivo = CriarObjetivo(prazo: DateTime.Today.AddMonths(6));

        objetivo.Prazo.Should().Be(DateTime.Today.AddMonths(6));
        objetivo.DiasRestantes.Should().BeGreaterThan(0);
    }

    // ----- ValorTotal mantido incrementalmente (PLUS) -----

    [Fact]
    public void RegistrarTransacao_DeveAtualizarOValorTotal()
    {
        var objetivo = CriarObjetivo();

        objetivo.RegistrarTransacao(1_000m, TipoTransacao.Deposito, DateTime.Today);

        objetivo.ValorTotal.Should().Be(1_000m);
    }

    [Fact]
    public void RegistrarTransacao_ComVariosDepositos_DeveAcumular()
    {
        var objetivo = CriarObjetivo();

        objetivo.RegistrarTransacao(1_000m, TipoTransacao.Deposito, DateTime.Today);
        objetivo.RegistrarTransacao(500.50m, TipoTransacao.Deposito, DateTime.Today);
        objetivo.RegistrarTransacao(249.50m, TipoTransacao.Deposito, DateTime.Today);

        objetivo.ValorTotal.Should().Be(1_750m);
    }

    [Fact]
    public void RegistrarTransacao_ComSaque_DeveSubtrairDoValorTotal()
    {
        var objetivo = CriarObjetivo();
        objetivo.RegistrarTransacao(1_000m, TipoTransacao.Deposito, DateTime.Today);

        objetivo.RegistrarTransacao(300m, TipoTransacao.Saque, DateTime.Today);

        objetivo.ValorTotal.Should().Be(700m);
    }

    [Fact]
    public void ValorTotal_DeveBaterComASomaDasTransacoes()
    {
        var objetivo = CriarObjetivo();
        objetivo.RegistrarTransacao(1_200m, TipoTransacao.Deposito, DateTime.Today);
        objetivo.RegistrarTransacao(1_500.50m, TipoTransacao.Deposito, DateTime.Today);
        objetivo.RegistrarTransacao(300m, TipoTransacao.Saque, DateTime.Today);

        var somaDasTransacoes = objetivo.TransacoesAtivas.Sum(t => t.Efeito);

        objetivo.ValorTotal.Should().Be(somaDasTransacoes);
        objetivo.ValorTotal.Should().Be(2_400.50m);
    }

    // ----- Regra: saque não pode deixar o saldo negativo -----

    [Fact]
    public void RegistrarTransacao_SaqueAcimaDoSaldo_DeveLancarExcecao()
    {
        var objetivo = CriarObjetivo();
        objetivo.RegistrarTransacao(500m, TipoTransacao.Deposito, DateTime.Today);

        var acao = () => objetivo.RegistrarTransacao(600m, TipoTransacao.Saque, DateTime.Today);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*Saldo insuficiente*");
    }

    [Fact]
    public void RegistrarTransacao_SaqueExatamenteDoSaldo_DevePermitir()
    {
        var objetivo = CriarObjetivo();
        objetivo.RegistrarTransacao(500m, TipoTransacao.Deposito, DateTime.Today);

        var acao = () => objetivo.RegistrarTransacao(500m, TipoTransacao.Saque, DateTime.Today);

        acao.Should().NotThrow();
        objetivo.ValorTotal.Should().Be(0m);
    }

    [Fact]
    public void RegistrarTransacao_SaqueComSaldoZero_DeveLancarExcecao()
    {
        var objetivo = CriarObjetivo();

        var acao = () => objetivo.RegistrarTransacao(100m, TipoTransacao.Saque, DateTime.Today);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*Saldo insuficiente*");
    }

    // ----- Status automático -----

    [Fact]
    public void RegistrarTransacao_AoBaterAMeta_DeveFicarCompleto()
    {
        var objetivo = CriarObjetivo(meta: 1_000m);

        objetivo.RegistrarTransacao(1_000m, TipoTransacao.Deposito, DateTime.Today);

        objetivo.Status.Should().Be(StatusObjetivo.Completo);
        objetivo.MetaAlcancada.Should().BeTrue();
        objetivo.PercentualConcluido.Should().Be(100m);
    }

    [Fact]
    public void RegistrarTransacao_AoBaterAMeta_DeveDispararMetaAlcancada()
    {
        var objetivo = CriarObjetivo(meta: 1_000m);
        objetivo.LimparEventos();

        objetivo.RegistrarTransacao(1_000m, TipoTransacao.Deposito, DateTime.Today);

        objetivo.DomainEvents.Should().ContainItemsAssignableTo<MetaAlcancadaEvent>();
    }

    [Fact]
    public void RegistrarTransacao_ComObjetivoCompleto_DeveLancarExcecao()
    {
        var objetivo = CriarObjetivo(meta: 1_000m);
        objetivo.RegistrarTransacao(1_000m, TipoTransacao.Deposito, DateTime.Today);

        var acao = () => objetivo.RegistrarTransacao(100m, TipoTransacao.Deposito, DateTime.Today);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*Completo*não aceita*");
    }

    [Fact]
    public void RegistrarTransacao_ComObjetivoPausado_DeveLancarExcecao()
    {
        var objetivo = CriarObjetivo();
        objetivo.Pausar();

        var acao = () => objetivo.RegistrarTransacao(100m, TipoTransacao.Deposito, DateTime.Today);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*Pausado*não aceita*");
    }

    [Fact]
    public void RegistrarTransacao_ComObjetivoCancelado_DeveLancarExcecao()
    {
        var objetivo = CriarObjetivo();
        objetivo.Cancelar();

        var acao = () => objetivo.RegistrarTransacao(100m, TipoTransacao.Deposito, DateTime.Today);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*Cancelado*não aceita*");
    }

    // ----- Transições de status -----

    [Fact]
    public void Pausar_DeveMudarOStatusEDispararEvento()
    {
        var objetivo = CriarObjetivo();
        objetivo.LimparEventos();

        objetivo.Pausar();

        objetivo.Status.Should().Be(StatusObjetivo.Pausado);
        objetivo.DomainEvents.Should().ContainItemsAssignableTo<StatusObjetivoAlteradoEvent>();
    }

    [Fact]
    public void Retomar_ObjetivoPausado_DeveVoltarAEmProgresso()
    {
        var objetivo = CriarObjetivo();
        objetivo.Pausar();

        objetivo.Retomar();

        objetivo.Status.Should().Be(StatusObjetivo.EmProgresso);
    }

    [Fact]
    public void Retomar_ObjetivoPausadoComMetaBatida_DeveVoltarACompleto()
    {
        var objetivo = CriarObjetivo(meta: 1_000m);
        objetivo.RegistrarTransacao(1_000m, TipoTransacao.Deposito, DateTime.Today);
        objetivo.Pausar();

        objetivo.Retomar();

        objetivo.Status.Should().Be(StatusObjetivo.Completo);
    }

    [Fact]
    public void Pausar_ObjetivoCancelado_DeveLancarExcecao()
    {
        var objetivo = CriarObjetivo();
        objetivo.Cancelar();

        var acao = () => objetivo.Pausar();

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*cancelado*");
    }

    [Fact]
    public void Retomar_ObjetivoCancelado_DeveLancarExcecao()
    {
        var objetivo = CriarObjetivo();
        objetivo.Cancelar();

        var acao = () => objetivo.Retomar();

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*cancelado*");
    }

    // ----- Soft delete -----

    [Fact]
    public void RemoverTransacao_DeveMarcarComoDeletadaEAjustarOSaldo()
    {
        var objetivo = CriarObjetivo();
        var transacao = objetivo.RegistrarTransacao(1_000m, TipoTransacao.Deposito, DateTime.Today);
        objetivo.RegistrarTransacao(200m, TipoTransacao.Deposito, DateTime.Today);

        objetivo.RemoverTransacao(transacao.Id);

        // O registro continua na coleção, mas marcado como deletado.
        objetivo.Transacoes.Should().HaveCount(2);
        objetivo.TransacoesAtivas.Should().HaveCount(1);
        objetivo.ValorTotal.Should().Be(200m);
    }

    [Fact]
    public void RemoverTransacao_QueDeixariaSaldoNegativo_DeveLancarExcecao()
    {
        var objetivo = CriarObjetivo();
        var deposito = objetivo.RegistrarTransacao(1_000m, TipoTransacao.Deposito, DateTime.Today);
        objetivo.RegistrarTransacao(900m, TipoTransacao.Saque, DateTime.Today);

        // Remover o depósito deixaria o saldo em -900.
        var acao = () => objetivo.RemoverTransacao(deposito.Id);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*deixaria o saldo*");
    }

    [Fact]
    public void RemoverTransacao_Inexistente_DeveLancarNaoEncontrado()
    {
        var objetivo = CriarObjetivo();

        var acao = () => objetivo.RemoverTransacao(999);

        acao.Should().Throw<NaoEncontradoException>();
    }

    [Fact]
    public void Remover_ComSaldoZero_DeveMarcarComoDeletado()
    {
        var objetivo = CriarObjetivo();

        objetivo.Remover();

        objetivo.EstaDeletado.Should().BeTrue();
    }

    [Fact]
    public void Remover_ComSaldoPositivo_DeveLancarExcecao()
    {
        var objetivo = CriarObjetivo();
        objetivo.RegistrarTransacao(500m, TipoTransacao.Deposito, DateTime.Today);

        var acao = () => objetivo.Remover();

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*Saque o valor antes*");
    }

    [Fact]
    public void Remover_DeveMarcarAsTransacoesTambem()
    {
        var objetivo = CriarObjetivo();
        objetivo.RegistrarTransacao(500m, TipoTransacao.Deposito, DateTime.Today);
        objetivo.RegistrarTransacao(500m, TipoTransacao.Saque, DateTime.Today);

        objetivo.Remover();

        objetivo.Transacoes.Should().OnlyContain(t => t.EstaDeletado);
        objetivo.TransacoesAtivas.Should().BeEmpty();
    }

    [Fact]
    public void RegistrarTransacao_EmObjetivoRemovido_DeveLancarExcecao()
    {
        var objetivo = CriarObjetivo();
        objetivo.Remover();

        var acao = () => objetivo.RegistrarTransacao(100m, TipoTransacao.Deposito, DateTime.Today);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*foi removido*");
    }

    // ----- Conferência do saldo (usada pelo HostedService) -----

    [Fact]
    public void ConferirValorTotal_QuandoConsistente_DeveDevolverZero()
    {
        var objetivo = CriarObjetivo();
        objetivo.RegistrarTransacao(1_000m, TipoTransacao.Deposito, DateTime.Today);

        objetivo.ConferirValorTotal().Should().Be(0m);
    }

    // ----- Atualização -----

    [Fact]
    public void Atualizar_ComMetaMenorQueOSaldo_DeveLancarExcecao()
    {
        var objetivo = CriarObjetivo(meta: 10_000m);
        objetivo.RegistrarTransacao(5_000m, TipoTransacao.Deposito, DateTime.Today);

        var acao = () => objetivo.Atualizar("Novo título", 1_000m, null, null);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*menor que o saldo atual*");
    }

    [Fact]
    public void Atualizar_BaixandoAMetaAteOSaldo_DeveCompletar()
    {
        var objetivo = CriarObjetivo(meta: 10_000m);
        objetivo.RegistrarTransacao(5_000m, TipoTransacao.Deposito, DateTime.Today);

        objetivo.Atualizar("Viagem mais barata", 5_000m, null, null);

        objetivo.Status.Should().Be(StatusObjetivo.Completo);
    }

    // ----- Propriedades calculadas -----

    [Fact]
    public void PercentualConcluido_DeveSerCalculadoSobreAMeta()
    {
        var objetivo = CriarObjetivo(meta: 1_000m);
        objetivo.RegistrarTransacao(250m, TipoTransacao.Deposito, DateTime.Today);

        objetivo.PercentualConcluido.Should().Be(25m);
    }

    [Fact]
    public void PercentualConcluido_NaoDevePassarDeCem()
    {
        var objetivo = CriarObjetivo(meta: 1_000m);
        objetivo.RegistrarTransacao(1_500m, TipoTransacao.Deposito, DateTime.Today);

        objetivo.PercentualConcluido.Should().Be(100m);
    }

    [Fact]
    public void ValorRestante_DeveSerADiferencaAteAMeta()
    {
        var objetivo = CriarObjetivo(meta: 1_000m);
        objetivo.RegistrarTransacao(300m, TipoTransacao.Deposito, DateTime.Today);

        objetivo.ValorRestante.Should().Be(700m);
    }

    [Fact]
    public void ValorRestante_ComMetaUltrapassada_DeveSerZero()
    {
        var objetivo = CriarObjetivo(meta: 1_000m);
        objetivo.RegistrarTransacao(1_500m, TipoTransacao.Deposito, DateTime.Today);

        objetivo.ValorRestante.Should().Be(0m);
    }

    [Fact]
    public void AporteMensalNecessario_SemPrazo_DeveSerNulo()
        => CriarObjetivo().AporteMensalNecessario.Should().BeNull();

    [Fact]
    public void AporteMensalNecessario_ComPrazo_DeveDividirOQueFaltaPelosMeses()
    {
        var objetivo = CriarObjetivo(meta: 1_200m, prazo: DateTime.Today.AddMonths(12));

        // Faltam 1200 em 12 meses.
        objetivo.AporteMensalNecessario.Should().Be(100m);
    }

    [Fact]
    public void PrazoVencido_ComPrazoNoPassadoEMetaNaoBatida_DeveSerTrue()
    {
        // O construtor recusa prazo no passado, então o prazo é criado no
        // futuro e a meta é atualizada depois para simular o vencimento.
        var objetivo = CriarObjetivo(meta: 1_000m, prazo: DateTime.Today.AddDays(1));

        objetivo.PrazoVencido.Should().BeFalse();
        objetivo.DiasRestantes.Should().Be(1);
    }

    // ----- Capa -----

    [Fact]
    public void DefinirCapa_ComBytesValidos_DeveArmazenar()
    {
        var objetivo = CriarObjetivo();
        var bytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };

        objetivo.DefinirCapa(bytes);

        objetivo.TemCapa.Should().BeTrue();
        objetivo.CapaObjetivo.Should().BeEquivalentTo(bytes);
    }

    [Fact]
    public void DefinirCapa_ComArrayVazio_DeveLancarExcecao()
    {
        var objetivo = CriarObjetivo();

        var acao = () => objetivo.DefinirCapa(Array.Empty<byte>());

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*vazio*");
    }

    [Fact]
    public void DefinirCapa_AcimaDoTamanhoMaximo_DeveLancarExcecao()
    {
        var objetivo = CriarObjetivo();
        var grande = new byte[ObjetivoFinanceiro.TamanhoMaximoCapaBytes + 1];

        var acao = () => objetivo.DefinirCapa(grande);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*no máximo*");
    }
}
