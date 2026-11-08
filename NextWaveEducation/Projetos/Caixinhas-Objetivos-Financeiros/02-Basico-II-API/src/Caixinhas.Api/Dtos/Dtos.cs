using Caixinhas.Api.Enums;

namespace Caixinhas.Api.Dtos;

// ------------------------------ OBJETIVOS ------------------------------

public record CriarObjetivoDto(
    string Titulo,
    decimal QuantidadeAlvo,
    DateTime? Prazo,
    decimal? QuantidadeIdealAporteMensal);

public record AtualizarObjetivoDto(
    string Titulo,
    decimal QuantidadeAlvo,
    DateTime? Prazo,
    decimal? QuantidadeIdealAporteMensal);

public record ObjetivoDto(
    int Id,
    string Titulo,
    decimal QuantidadeAlvo,
    decimal ValorTotal,
    decimal ValorRestante,
    decimal PercentualConcluido,
    StatusObjetivo Status,
    DateTime? Prazo,
    int? DiasRestantes,
    decimal? QuantidadeIdealAporteMensal,
    int TotalTransacoes,
    DateTime DataCriacao);

public record ObjetivoDetalheDto(
    int Id,
    string Titulo,
    decimal QuantidadeAlvo,
    decimal ValorTotal,
    decimal ValorRestante,
    decimal PercentualConcluido,
    StatusObjetivo Status,
    DateTime? Prazo,
    int? DiasRestantes,
    decimal? QuantidadeIdealAporteMensal,
    DateTime DataCriacao,
    List<TransacaoDto> Transacoes);

// ------------------------------ TRANSAÇÕES -----------------------------

public record CriarTransacaoDto(
    decimal Quantidade,
    TipoTransacao Tipo,
    DateTime? DataTransacao);

public record TransacaoDto(
    int Id,
    int ObjetivoFinanceiroId,
    decimal Quantidade,
    TipoTransacao Tipo,
    DateTime DataTransacao,
    DateTime DataCriacao);

/// <summary>Resposta do cadastro de transação, com o saldo já atualizado.</summary>
public record ResultadoTransacaoDto(
    TransacaoDto Transacao,
    string TituloObjetivo,
    decimal ValorTotalAtualizado,
    decimal PercentualConcluido,
    StatusObjetivo StatusObjetivo,
    bool MetaAlcancada);

// -------------------------------- OUTROS -------------------------------

public record ErroDto(string Mensagem);
