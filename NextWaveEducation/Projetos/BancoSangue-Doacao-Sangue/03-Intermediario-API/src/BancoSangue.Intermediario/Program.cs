using System.Text.Json.Serialization;
using BancoSangue.Intermediario.Aplicacao.Servicos;
using BancoSangue.Intermediario.Aplicacao.Validadores;
using BancoSangue.Intermediario.Dominio.Interfaces;
using BancoSangue.Intermediario.Infraestrutura.Dados;
using BancoSangue.Intermediario.Infraestrutura.Repositorios;
using BancoSangue.Intermediario.Infraestrutura.Servicos;
using BancoSangue.Intermediario.Middlewares;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Banco de Sangue API - Nível Intermediário",
        Version = "v1",
        Description = "API de gerenciamento de banco de doação de sangue. " +
                      "Repository, FluentValidation, Middleware de exceções, SQLite e integração ViaCEP."
    });
});

// Banco SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=bancosangue.db";

builder.Services.AddDbContext<BancoSangueDbContext>(options =>
    options.UseSqlite(connectionString));

// Padrão Repository
builder.Services.AddScoped<IDoadorRepository, DoadorRepository>();
builder.Services.AddScoped<IDoacaoRepository, DoacaoRepository>();
builder.Services.AddScoped<IEstoqueSangueRepository, EstoqueSangueRepository>();

// Serviços de aplicação
builder.Services.AddScoped<IDoadorAppService, DoadorAppService>();
builder.Services.AddScoped<IDoacaoAppService, DoacaoAppService>();
builder.Services.AddScoped<IEstoqueAppService, EstoqueAppService>();

// Integração com API externa de CEP
builder.Services.AddHttpClient<ICepService, ViaCepService>(client =>
{
    client.BaseAddress = new Uri("https://viacep.com.br/ws/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CriarDoadorInputModelValidator>();

var app = builder.Build();

// Cria o banco SQLite e aplica o seed do estoque na primeira execução.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BancoSangueDbContext>();
    context.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Banco de Sangue API v1"));
}

// Middleware de tratamento de exceções registrado antes do pipeline de rotas.
app.UseMiddleware<TratamentoExcecoesMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();
