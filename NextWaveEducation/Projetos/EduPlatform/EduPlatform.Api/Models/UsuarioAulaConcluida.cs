namespace EduPlatform.Api.Models;

public class UsuarioAulaConcluida
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid IdUsuario { get; set; }
    public Usuario? Usuario { get; set; }

    public Guid IdAula { get; set; }
    public Aula? Aula { get; set; }

    public DateTime DataConclusao { get; set; } = DateTime.UtcNow;

    /// <summary>Nota de 0 a 10.</summary>
    public int Nota { get; set; }
}
