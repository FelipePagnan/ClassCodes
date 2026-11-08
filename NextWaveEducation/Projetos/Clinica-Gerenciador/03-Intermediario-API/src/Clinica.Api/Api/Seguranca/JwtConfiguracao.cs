using System.Security.Claims;
using System.Text;
using Clinica.Api.Dominio.Enums;
using Microsoft.IdentityModel.Tokens;

namespace Clinica.Api.Api.Seguranca;

/// <summary>Configuração do token, lida de appsettings na seção "Jwt".</summary>
public class JwtConfiguracao
{
    public const string Secao = "Jwt";

    public string Emissor { get; set; } = "ClinicaVida";
    public string Audiencia { get; set; } = "ClinicaVida.Api";
    public string ChaveSecreta { get; set; } = string.Empty;
    public int MinutosDeValidade { get; set; } = 480;

    /// <summary>
    /// A chave precisa ter pelo menos 32 bytes para HMAC-SHA256. Falhar aqui,
    /// na subida da aplicação, é melhor do que falhar no primeiro login.
    /// </summary>
    public SymmetricSecurityKey ObterChave()
    {
        if (string.IsNullOrWhiteSpace(ChaveSecreta))
            throw new InvalidOperationException(
                "Jwt:ChaveSecreta não está configurada em appsettings.");

        var bytes = Encoding.UTF8.GetBytes(ChaveSecreta);

        if (bytes.Length < 32)
            throw new InvalidOperationException(
                "Jwt:ChaveSecreta precisa ter ao menos 32 bytes (32 caracteres ASCII).");

        return new SymmetricSecurityKey(bytes);
    }

    public TokenValidationParameters ObterParametrosDeValidacao() => new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        RequireExpirationTime = true,
        ValidIssuer = Emissor,
        ValidAudience = Audiencia,
        IssuerSigningKey = ObterChave(),
        // Sem isso o .NET tolera 5 minutos de token expirado por padrão.
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.Name
    };
}

/// <summary>
/// Nomes dos perfis como string, para usar em [Authorize(Roles = ...)].
/// Os valores batem com o enum PerfilUsuario.
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

/// <summary>Claims próprias da aplicação.</summary>
public static class ClaimsDaClinica
{
    public const string MedicoId = "clinica:medico_id";
}

/// <summary>Atalhos de leitura do usuário autenticado.</summary>
public static class UsuarioLogadoExtensions
{
    public static int? ObterUsuarioId(this ClaimsPrincipal usuario)
        => int.TryParse(usuario.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id)
            ? id
            : null;

    public static int? ObterMedicoId(this ClaimsPrincipal usuario)
        => int.TryParse(usuario.FindFirst(ClaimsDaClinica.MedicoId)?.Value, out var id)
            ? id
            : null;

    public static bool EhMedico(this ClaimsPrincipal usuario) => usuario.IsInRole(Perfis.Medico);

    /// <summary>
    /// Médico só enxerga a própria agenda; administrador e recepcionista
    /// enxergam todas. Devolve o filtro de médico a aplicar, ou null.
    /// </summary>
    public static int? FiltroDeMedico(this ClaimsPrincipal usuario)
        => usuario.EhMedico() ? usuario.ObterMedicoId() : null;
}
