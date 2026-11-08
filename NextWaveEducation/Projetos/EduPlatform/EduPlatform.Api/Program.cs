using EduPlatform.Api.Data;
using EduPlatform.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers + Swagger
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "EduPlatform API",
        Version = "v1",
        Description = "API para gestão de cursos, assinaturas e progresso de alunos."
    });
});

// Banco de dados (SQLite por padrão — fácil de rodar localmente sem instalar nada)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=eduplatform.db"));

// Integração Asaas
builder.Services.AddHttpClient<IAsaasService, AsaasService>();

// CORS liberado para desenvolvimento
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// Cria o banco de dados (SQLite) automaticamente ao iniciar, caso não exista.
// Para um fluxo com migrations versionadas, rode:
//   dotnet ef migrations add Inicial
//   dotnet ef database update
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
