namespace EduPlatform.Api.Models;

public class PagamentoAssinatura
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime DataProcessamento { get; set; } = DateTime.UtcNow;

    public StatusPagamento Status { get; set; } = StatusPagamento.Pendente;

    public string? Mensagem { get; set; }

    public decimal Valor { get; set; }

    public Guid IdUsuarioAssinatura { get; set; }
    public UsuarioAssinatura? UsuarioAssinatura { get; set; }

    public string? LinkPagamento { get; set; }

    /// <summary>Id da cobrança no gateway externo (Asaas).</summary>
    public string? IdExternoPagamento { get; set; }

    public DateTime DataVencimento { get; set; }
}
