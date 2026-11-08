using System.ComponentModel.DataAnnotations;

namespace EduPlatform.Api.Models;

public class Aula
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    [MaxLength(500)]
    public string? LinkVideo { get; set; }

    /// <summary>Duração em segundos.</summary>
    public int Duracao { get; set; }

    public Guid IdModulo { get; set; }
    public Modulo? Modulo { get; set; }

    public List<UsuarioAulaConcluida> Conclusoes { get; set; } = new();
}
