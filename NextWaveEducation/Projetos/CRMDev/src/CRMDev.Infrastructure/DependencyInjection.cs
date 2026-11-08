using System.Text;
using CRMDev.Application.Interfaces;
using CRMDev.Application.Services;
using CRMDev.Infrastructure.Data;
using CRMDev.Infrastructure.Identity;
using CRMDev.Infrastructure.Repositories;
using CRMDev.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CRMDev.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Banco de dados: SQLite por padrão (zero-config); troque a ConnectionString
        // "DefaultConnection" e o provedor abaixo para SQL Server se preferir.
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=crmdev.db";

        services.AddDbContext<CRMDevDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<CRMDevDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization();

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IContatoRepository, ContatoRepository>();
        services.AddScoped<IOportunidadeRepository, OportunidadeRepository>();
        services.AddScoped<IAreaAtuacaoRepository, AreaAtuacaoRepository>();
        services.AddScoped<ContatoService>();
        services.AddScoped<OportunidadeService>();

        return services;
    }
}
