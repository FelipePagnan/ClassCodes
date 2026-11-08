using System.ComponentModel.DataAnnotations;

namespace EduPlatform.Api.Models;

public class Assinatura
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    /// <summary>Duração em dias.</summary>
    public int Duracao { get; set; }

    public decimal Valor { get; set; }

    public List<Curso> Cursos { get; set; } = new();

    public List<UsuarioAssinatura> UsuarioAssinaturas { get; set; } = new();
}
