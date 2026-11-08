using System.ComponentModel.DataAnnotations;

namespace EduPlatform.Api.Models;

public class Modulo
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public Guid IdCurso { get; set; }
    public Curso? Curso { get; set; }

    public List<Aula> Aulas { get; set; } = new();
}
