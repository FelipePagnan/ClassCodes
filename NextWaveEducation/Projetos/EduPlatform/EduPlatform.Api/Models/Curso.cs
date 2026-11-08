using System.ComponentModel.DataAnnotations;

namespace EduPlatform.Api.Models;

public class Curso
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public string? Cover { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    // Navegação
    public List<Modulo> Modulos { get; set; } = new();
    public List<Assinatura> Assinaturas { get; set; } = new();
}
