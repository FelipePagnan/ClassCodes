using System.Text.Json.Serialization;
using Clinica.Api.BackgroundServices;
using Clinica.Api.Middlewares;
using Clinica.Api.Seguranca;
using Clinica.Aplicacao.Comum;
using Clinica.Aplicacao.Contratos;
using Clinica.Infraestrutura;
using Clinica.Infraestrutura.Seguranca;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ============================= Composição das camadas =============================
// Cada camada registra os próprios serviços. A API conhece as três; nenhuma
// das outras conhece a API.
builder.Services.AdicionarAplicacao();
builder.Services.AdicionarInfraestrutura(builder.Configuration);

builder.Services.Configure<LembretesConfiguracao>(
    builder.Configuration.GetSection(LembretesConfiguracao.Secao));

// Quem está logado: contrato na aplicação, implementação aqui, porque só a
// API conhece HTTP.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioAtual, UsuarioAtual>();

// Background service que notifica os atendimentos do dia seguinte.
builder.Services.AddHostedService<LembreteBackgroundService>();

// ========================== Autenticação e autorização ==========================
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opcoes =>
    {
        var jwt = builder.Configuration
            .GetSection(JwtConfiguracao.Secao)
            .Get<JwtConfiguracao>() ?? new JwtConfiguracao();

        opcoes.TokenValidationParameters = jwt.ObterParametrosDeValidacao();

        // Em produção isto deve ser true: só HTTPS.
        opcoes.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        opcoes.SaveToken = true;
    });

builder.Services.AddAuthorization();

// ==================================== MVC ====================================
builder.Services
    .AddControllers()
    .AddJsonOptions(opcoes =>
    {
        opcoes.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        opcoes.JsonSerializerOptions.DefaultIgnoreCondition =
            JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opcoes =>
{
    opcoes.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Clínica Vida — Gerenciador de Clínica",
        Version = "v1",
        Description = "Avançado: Clean Architecture, CQRS com MediatR, Unit of Work, "
                    + "eventos de domínio, HostedService e testes de unidade."
    });

    opcoes.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Faça login em /api/autenticacao/login e cole aqui o token. "
                    + "Teste: admin@clinicavida.com.br / Clinica123"
    });

    opcoes.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    foreach (var nome in new[] { "Clinica.Api", "Clinica.Aplicacao", "Clinica.Dominio" })
    {
        var xml = Path.Combine(AppContext.BaseDirectory, $"{nome}.xml");
        if (File.Exists(xml)) opcoes.IncludeXmlComments(xml);
    }
});

var app = builder.Build();

// Falha cedo se a chave do token estiver ausente ou curta: melhor na subida
// do que no primeiro login.
app.Services.GetRequiredService<IOptions<JwtConfiguracao>>().Value.ObterChave();

// O middleware de erro é o primeiro do pipeline, para capturar o que vier de
// qualquer etapa seguinte.
app.UseTratamentoDeExcecao();

await app.Services.PrepararBancoAsync();

app.UseSwagger();
app.UseSwaggerUI(opcoes =>
{
    opcoes.SwaggerEndpoint("/swagger/v1/swagger.json", "Clínica Vida v1");
    opcoes.RoutePrefix = string.Empty;
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
