using Caixinhas.Domain.Common;
using Caixinhas.Domain.Enums;
using Caixinhas.Domain.Exceptions;
using Caixinhas.Domain.ValueObjects;

namespace Caixinhas.Domain.Entities;

public class Transacao : BaseEntity
{
    // Construtor sem parâmetros para o EF Core.
    private Transacao() { }

    /// <summary>
    /// Só o agregado ObjetivoFinanceiro cria transações, para garantir que
    /// o saldo da caixinha seja sempre atualizado junto.
    /// </summary>
    internal Transacao(
        int objetivoFinanceiroId,
        decimal quantidade,
        TipoTransacao tipo,
        DateTime dataTransacao)
    {
        // O Value Object aplica as regras de valor positivo e duas casas decimais.
        Quantidade = Dinheiro.CriarPositivo(quantidade, "valor da transação").Valor;

        if (!Enum.IsDefined(tipo))
            throw new RegraNegocioException($"O tipo de transação '{tipo}' é inválido.");

        // A data pode ser hoje ou passada, nunca futura.
        if (dataTransacao.Date > DateTime.Today)
            throw new RegraNegocioException(
                $"A data da transação ({dataTransacao:dd/MM/yyyy}) não pode estar no futuro.");

        if (dataTransacao.Year < 1900)
            throw new RegraNegocioException("A data da transação é inválida.");

        ObjetivoFinanceiroId = objetivoFinanceiroId;
        Tipo = tipo;
        DataTransacao = dataTransacao.Date;
        DataCriacao = DateTime.Now;
    }

    public decimal Quantidade { get; private set; }
    public TipoTransacao Tipo { get; private set; }

    /// <summary>Data em que a movimentação aconteceu. Pode ser hoje ou passada.</summary>
    public DateTime DataTransacao { get; private set; }

    public int ObjetivoFinanceiroId { get; private set; }
    public ObjetivoFinanceiro Objetivo { get; private set; } = null!;

    /// <summary>Reconstrói o Value Object a partir da coluna persistida.</summary>
    public Dinheiro Valor => Dinheiro.Criar(Quantidade);

    /// <summary>Efeito no saldo: positivo para depósito, negativo para saque.</summary>
    public decimal Efeito => Tipo == TipoTransacao.Deposito ? Quantidade : -Quantidade;

    public bool EhDeposito => Tipo == TipoTransacao.Deposito;

    /// <summary>
    /// Marca a transação como removida. Chamado pelo agregado, que ajusta
    /// o saldo da caixinha na mesma operação.
    /// </summary>
    internal void Remover() => MarcarComoDeletado();
}
