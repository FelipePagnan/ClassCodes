using Clinica.Console.Enums;

namespace Clinica.Console.Entidades;

/// <summary>
/// Usuário do sistema. A senha nunca é guardada em texto: apenas o hash
/// PBKDF2 e o salt que o gerou.
/// </summary>
public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public string SenhaHash { get; set; } = string.Empty;
    public string SenhaSalt { get; set; } = string.Empty;

    public PerfilUsuario Perfil { get; set; }

    /// <summary>
    /// Vínculo com o cadastro de médico, quando o perfil é Medico. É o que
    /// permite a um médico ver apenas a própria agenda.
    /// </summary>
    public int? MedicoId { get; set; }
    public Medico? Medico { get; set; }

    public bool Ativo { get; set; } = true;
    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public DateTime? UltimoAcesso { get; set; }

    public override string ToString()
        => $"[{Id}] {Nome,-26} {Email,-30} {Perfil}{(Ativo ? string.Empty : "  [INATIVO]")}";
}
