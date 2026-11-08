using System.Text.Json.Serialization;
using Clinica.Api.BackgroundServices;
using Clinica.Api.Data;
using Clinica.Api.Excecoes;
using Clinica.Api.Seguranca;
using Clinica.Api.Servicos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------ Banco de dados ------------------------------
// Neste nível o provider é InMemory: zero configuração, e os dados somem
// quando a aplicação para. No Intermediário isso passa a ser SQLite.
builder.Services.AddDbContext<ClinicaDbContext>(opcoes =>
    opcoes.UseInMemoryDatabase("ClinicaVida"));

// -------------------------------- Serviços --------------------------------
builder.Services.AddScoped<PacienteService>();
builder.Services.AddScoped<MedicoService>();
builder.Services.AddScoped<ServicoService>();
builder.Services.AddScoped<AtendimentoService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<NotificacaoService>();

// A caixa de saída é singleton: faz o papel do servidor de e-mail e do
// gateway de SMS, que num ambiente real seriam externos.
builder.Services.AddSingleton<CaixaDeSaidaNotificacoes>();

// Background service que notifica os atendimentos do dia seguinte.
builder.Services.AddHostedService<LembreteBackgroundService>();

// -------------------- Autenticação e autorização (PLUS 2) --------------------
builder.Services
    .AddAuthentication(AutenticacaoBasicaHandler.Esquema)
    .AddScheme<AuthenticationSchemeOptions, AutenticacaoBasicaHandler>(
        AutenticacaoBasicaHandler.Esquema, null);

builder.Services.AddAuthorization();

// ---------------------------------- MVC ----------------------------------
builder.Services
    .AddControllers()
    .AddJsonOptions(opcoes =>
    {
        // As entidades têm navegação nos dois sentidos (Paciente tem
        // Atendimentos, e cada Atendimento tem Paciente). Sem isso o
        // serializador entra em laço. No Intermediário o problema desaparece,
        // porque lá a API devolve ViewModel em vez da entidade.
        opcoes.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        opcoes.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opcoes =>
{
    opcoes.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Clínica Vida — Gerenciador de Clínica",
        Version = "v1",
        Description = "Básico II: ASP.NET Core + Entity Framework Core (InMemory). "
                    + "Autenticação HTTP Basic com os perfis Administrador, Medico "
                    + "e Recepcionista."
    });

    // Botão "Authorize" do Swagger, para mandar as credenciais Basic.
    opcoes.AddSecurityDefinition("basic", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        In = ParameterLocation.Header,
        Description = "E-mail e senha do usuário. Teste: admin@clinicavida.com.br / Clinica123"
    });

    opcoes.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "basic"
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

// ---------------------------- Tratamento de erro ----------------------------
// Aqui ainda é o handler pronto do framework. No Intermediário isso vira um
// middleware próprio, que é o jeito de reaproveitar o tratamento em
// qualquer pipeline.
app.UseExceptionHandler(ramo => ramo.Run(async contexto =>
{
    var excecao = contexto.Features.Get<IExceptionHandlerFeature>()?.Error;

    var (status, mensagem) = excecao switch
    {
        RegraNegocioException regra => (StatusCodes.Status400BadRequest, regra.Message),
        _ => (StatusCodes.Status500InternalServerError,
              "Erro inesperado ao processar a requisição.")
    };

    contexto.Response.StatusCode = status;
    contexto.Response.ContentType = "application/json";

    await contexto.Response.WriteAsJsonAsync(new { mensagem });
}));

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
