using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Clinica.Api.Servicos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Clinica.Api.Seguranca;

/// <summary>
/// Autenticação HTTP Basic (RFC 7617). Neste nível ela resolve o PLUS de
/// autenticação e autorização sem depender de pacote externo: o
/// AuthenticationHandler e o [Authorize(Roles = ...)] já vêm no framework
/// compartilhado do ASP.NET Core.
///
/// A partir do nível Intermediário isso vira JWT, que é o que se usa de
/// verdade — Basic manda e-mail e senha em toda requisição.
/// </summary>
public class AutenticacaoBasicaHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string Esquema = "Basic";

    private readonly UsuarioService _usuarioService;

    public AutenticacaoBasicaHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        UsuarioService usuarioService)
        : base(options, logger, encoder)
        => _usuarioService = usuarioService;

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var cabecalho))
            return AuthenticateResult.NoResult();

        var valor = cabecalho.ToString();

        if (!valor.StartsWith($"{Esquema} ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        string email;
        string senha;

        try
        {
            var credenciais = Encoding.UTF8.GetString(
                Convert.FromBase64String(valor[(Esquema.Length + 1)..].Trim()));

            // O formato é "usuario:senha"; a senha pode conter ':'.
            var separador = credenciais.IndexOf(':');
            if (separador < 0) return AuthenticateResult.Fail("Credenciais mal formatadas.");

            email = credenciais[..separador];
            senha = credenciais[(separador + 1)..];
        }
        catch (FormatException)
        {
            return AuthenticateResult.Fail("Credenciais mal formatadas.");
        }

        var usuario = await _usuarioService.AutenticarAsync(email, senha);

        if (usuario is null)
            return AuthenticateResult.Fail("E-mail ou senha inválidos.");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Perfil.ToString())
        };

        // O vínculo com o médico é o que permite filtrar a agenda para que
        // cada médico veja apenas os próprios atendimentos.
        if (usuario.MedicoId is not null)
            claims.Add(new Claim(ClaimsDoUsuario.MedicoId, usuario.MedicoId.Value.ToString()));

        var identidade = new ClaimsIdentity(claims, Esquema);
        var principal = new ClaimsPrincipal(identidade);

        return AuthenticateResult.Success(new AuthenticationTicket(principal, Esquema));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        // Sem o WWW-Authenticate o navegador não abre a caixa de login.
        Response.Headers.WWWAuthenticate = $"{Esquema} realm=\"Clinica Vida\", charset=\"UTF-8\"";
        return base.HandleChallengeAsync(properties);
    }
}

/// <summary>Nomes das claims próprias da aplicação.</summary>
public static class ClaimsDoUsuario
{
    public const string MedicoId = "clinica:medico_id";
}

/// <summary>Atalhos de leitura do usuário autenticado.</summary>
public static class UsuarioLogadoExtensions
{
    public static int? ObterMedicoId(this ClaimsPrincipal usuario)
    {
        var claim = usuario.FindFirst(ClaimsDoUsuario.MedicoId)?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }

    public static bool EhMedico(this ClaimsPrincipal usuario)
        => usuario.IsInRole(Perfis.Medico);

    /// <summary>
    /// Médico só enxerga a própria agenda; administrador e recepcionista
    /// enxergam todas. Devolve o filtro de médico a aplicar, ou null.
    /// </summary>
    public static int? FiltroDeMedico(this ClaimsPrincipal usuario)
        => usuario.EhMedico() ? usuario.ObterMedicoId() : null;
}
