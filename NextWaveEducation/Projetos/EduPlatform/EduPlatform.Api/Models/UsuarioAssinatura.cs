namespace EduPlatform.Api.Models;

public class UsuarioAssinatura
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid IdUsuario { get; set; }
    public Usuario? Usuario { get; set; }

    public Guid IdAssinatura { get; set; }
    public Assinatura? Assinatura { get; set; }

    public StatusUsuarioAssinatura Status { get; set; } = StatusUsuarioAssinatura.Pendente;

    public DateTime DataInicio { get; set; }
    public DateTime DataExpiracao { get; set; }

    // Integração Asaas
    public string? AsaasCustomerId { get; set; }

    public List<PagamentoAssinatura> Pagamentos { get; set; } = new();
}
