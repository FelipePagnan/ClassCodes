namespace Clinica.Api.Seguranca;

/// <summary>
/// Nomes dos perfis como string, para usar em [Authorize(Roles = ...)].
/// Os valores batem com o enum PerfilUsuario.
/// </summary>
public static class Perfis
{
    public const string Administrador = "Administrador";
    public const string Medico = "Medico";
    public const string Recepcionista = "Recepcionista";

    /// <summary>Quem pode operar a agenda: administração e recepção.</summary>
    public const string AdministradorOuRecepcionista = $"{Administrador},{Recepcionista}";

    /// <summary>Quem pode ver dados clínicos: todos os perfis.</summary>
    public const string Todos = $"{Administrador},{Medico},{Recepcionista}";

    /// <summary>Quem produz documento clínico: o médico (e a administração).</summary>
    public const string AdministradorOuMedico = $"{Administrador},{Medico}";
}
