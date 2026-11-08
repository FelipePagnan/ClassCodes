using System.Text.Json.Serialization;
using Clinica.Api.Api.Middlewares;
using Clinica.Api.Api.Seguranca;
using Clinica.Api.Aplicacao.Servicos;
using Clinica.Api.Aplicacao.Validadores;
using Clinica.Api.BackgroundServices;
using Clinica.Api.Infraestrutura.Contexto;
using Clinica.Api.Infraestrutura.Repositorios;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------ Configuração ------------------------------
builder.Services.Configure<JwtConfiguracao>(
    builder.Configuration.GetSection(JwtConfiguracao.Secao));

builder.Services.Configure<LembretesConfiguracao>(
    builder.Configuration.GetSection(LembretesConfiguracao.Secao));

// ------------------------------ Banco de dados ------------------------------
// SQLite: arquivo no disco, então os dados sobrevivem ao reinício — ao
// contrário do InMemory do nível anterior.
builder.Services.AddDbContext<ClinicaDbContext>(opcoes =>
    opcoes.UseSqlite(
        builder.Configuration.GetConnectionString("Clinica")
        ?? "Data Source=clinica.db"));

// ------------------------------- Repositórios -------------------------------
builder.Services.AddScoped<IPacienteRepositorio, PacienteRepositorio>();
builder.Services.AddScoped<IMedicoRepositorio, MedicoRepositorio>();
builder.Services.AddScoped<IServicoRepositorio, ServicoRepositorio>();
builder.Services.AddScoped<IAtendimentoRepositorio, AtendimentoRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();

// ---------------------------- Serviços de aplicação ----------------------------
builder.Services.AddScoped<PacienteAppService>();
builder.Services.AddScoped<MedicoAppService>();
builder.Services.AddScoped<ServicoAppService>();
builder.Services.AddScoped<AtendimentoAppService>();
builder.Services.AddScoped<UsuarioAppService>();
builder.Services.AddScoped<NotificacaoAppService>();

// A caixa de saída é singleton e substitui o SMTP e o gateway de SMS. Trocar
// por um envio real é registrar outra implementação de IEnviadorDeNotificacao.
builder.Services.AddSingleton<IEnviadorDeNotificacao, CaixaDeSaidaEmMemoria>();

// Background service que notifica os atendimentos do dia seguinte.
builder.Services.AddHostedService<LembreteBackgroundService>();

// ------------------------------- Validação -------------------------------
// Registra de uma vez todos os AbstractValidator do assembly.
builder.Services.AddValidatorsFromAssemblyContaining<PacienteInputModelValidator>();

// --------------------- Autenticação e autorização (PLUS 2) ---------------------
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

// ---------------------------------- MVC ----------------------------------
builder.Services
    .AddControllers()
    .AddJsonOptions(opcoes =>
    {
        // Não há mais ReferenceHandler.IgnoreCycles aqui: a API devolve
        // ViewModel, e ViewModel não tem laço de navegação.
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
        Description = "Intermediário: FluentValidation, Repository, Middleware de erro, "
                    + "InputModel/ViewModel, IEntityTypeConfiguration, SQLite e JWT."
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

    var xml = Path.Combine(AppContext.BaseDirectory,
        $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml");

    if (File.Exists(xml)) opcoes.IncludeXmlComments(xml);
});

var app = builder.Build();

// Falha cedo se a chave do token estiver ausente ou curta: melhor na subida
// do que no primeiro login.
app.Services.GetRequiredService<IOptions<JwtConfiguracao>>().Value.ObterChave();

// O middleware de erro é o primeiro do pipeline, para capturar o que vier
// de qualquer etapa seguinte.
app.UseTratamentoDeExcecao();

// ------------------------------ Dados iniciais ------------------------------
using (var escopo = app.Services.CreateScope())
{
    var context = escopo.ServiceProvider.GetRequiredService<ClinicaDbContext>();
    await DadosIniciais.SemearAsync(context);
}

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
