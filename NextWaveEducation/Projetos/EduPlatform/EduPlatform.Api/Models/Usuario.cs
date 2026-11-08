using System.ComponentModel.DataAnnotations;

namespace EduPlatform.Api.Models;

public class Usuario
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Sobrenome { get; set; } = string.Empty;

    // Propriedade calculada, não mapeada
    public string NomeCompleto => $"{Nome} {Sobrenome}".Trim();

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string SenhaHash { get; set; } = string.Empty;

    public DateTime DataNascimento { get; set; }

    [MaxLength(20)]
    public string? Documento { get; set; }

    [MaxLength(20)]
    public string? TelefoneCelular { get; set; }

    public RoleUsuario Role { get; set; } = RoleUsuario.Aluno;

    public bool Ativo { get; set; } = true;

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    // Navegação
    public List<UsuarioAssinatura> Assinaturas { get; set; } = new();
    public List<UsuarioAulaConcluida> AulasConcluidas { get; set; } = new();
}
