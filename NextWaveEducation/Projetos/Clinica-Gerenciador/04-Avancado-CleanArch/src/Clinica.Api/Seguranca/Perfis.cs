using System.Security.Claims;
using Clinica.Aplicacao.Contratos;
using Clinica.Dominio.Enums;
using Clinica.Infraestrutura.Seguranca;

namespace Clinica.Api.Seguranca;

/// <summary>
/// Nomes dos perfis como string, para [Authorize(Roles = ...)]. Os valores
/// batem com o enum PerfilUsuario.
/// </summary>
public static class Perfis
{
    public const string Administrador = nameof(PerfilUsuario.Administrador);
    public const string Medico = nameof(PerfilUsuario.Medico);
    public const string Recepcionista = nameof(PerfilUsuario.Recepcionista);

    /// <summary>Quem opera a agenda: administração e recepção.</summary>
    public const string AdministradorOuRecepcionista = $"{Administrador},{Recepcionista}";

    /// <summary>Quem produz documento clínico: o médico (e a administração).</summary>
    public const string AdministradorOuMedico = $"{Administrador},{Medico}";

    /// <summary>Todos os perfis.</summary>
    public const string Todos = $"{Administrador},{Medico},{Recepcionista}";
}

/// <summary>
/// Implementação de IUsuarioAtual sobre o HttpContext.
///
/// O contrato está na aplicação e a implementação aqui, na API, porque é a
/// API que conhece HTTP. A camada de aplicação precisa saber quem está
/// logado para filtrar a agenda do médico, mas não pode depender de
/// HttpContext — se dependesse, o mesmo caso de uso não rodaria no
/// background service.
/// </summary>
public class UsuarioAtual : IUsuarioAtual
{
    private readonly ClaimsPrincipal? _usuario;

    public UsuarioAtual(IHttpContextAccessor acessor) => _usuario = acessor.HttpContext?.User;

    public int? UsuarioId => Inteiro(ClaimTypes.NameIdentifier);

    public string? Nome => _usuario?.FindFirst(ClaimTypes.Name)?.Value;

    public string? Perfil => _usuario?.FindFirst(ClaimTypes.Role)?.Value;

    public int? MedicoId => Inteiro(ClaimsDaClinica.MedicoId);

    public bool EhMedico => _usuario?.IsInRole(Perfis.Medico) ?? false;

    public int? FiltroDeMedico => EhMedico ? MedicoId : null;

    private int? Inteiro(string claim)
        => int.TryParse(_usuario?.FindFirst(claim)?.Value, out var valor) ? valor : null;
}
