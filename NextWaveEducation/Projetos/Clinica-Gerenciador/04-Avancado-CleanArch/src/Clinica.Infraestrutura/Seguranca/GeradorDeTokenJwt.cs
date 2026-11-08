using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Clinica.Aplicacao.Contratos;
using Clinica.Dominio.Entidades;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Clinica.Infraestrutura.Seguranca;

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

/// <summary>Claims próprias da aplicação.</summary>
public static class ClaimsDaClinica
{
    public const string MedicoId = "clinica:medico_id";
}

/// <summary>Emite o token JWT assinado com HMAC-SHA256.</summary>
public class GeradorDeTokenJwt : IGeradorDeToken
{
    private readonly JwtConfiguracao _configuracao;

    public GeradorDeTokenJwt(IOptions<JwtConfiguracao> configuracao)
        => _configuracao = configuracao.Value;

    public (string Token, DateTime ExpiraEm) Gerar(Usuario usuario)
    {
        var expiraEm = DateTime.UtcNow.AddMinutes(_configuracao.MinutosDeValidade);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email.Valor),
            new(ClaimTypes.Role, usuario.Perfil.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // O vínculo com o médico vai no token: é assim que a API filtra a
        // agenda sem consultar o banco a cada requisição.
        if (usuario.MedicoId is not null)
            claims.Add(new Claim(ClaimsDaClinica.MedicoId, usuario.MedicoId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer: _configuracao.Emissor,
            audience: _configuracao.Audiencia,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiraEm,
            signingCredentials: new SigningCredentials(
                _configuracao.ObterChave(), SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
    }
}
