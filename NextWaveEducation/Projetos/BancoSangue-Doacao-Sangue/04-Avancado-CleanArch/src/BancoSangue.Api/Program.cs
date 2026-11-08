using System.Text.Json.Serialization;
using BancoSangue.Api.Middlewares;
using BancoSangue.Application;
using BancoSangue.Infrastructure;
using BancoSangue.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Banco de Sangue API - Nível Avançado",
        Version = "v1",
        Description = "Arquitetura Limpa, CQRS (MediatR), Unit of Work, " +
                      "Domain Events, HostedService e testes unitários."
    });

    var xmlApi = Path.Combine(AppContext.BaseDirectory, "BancoSangue.Api.xml");
    if (File.Exists(xmlApi)) c.IncludeXmlComments(xmlApi);
});

// Camadas da Arquitetura Limpa: a API só conhece Application e Infrastructure.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

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
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Banco de Sangue API v1");
        c.RoutePrefix = "swagger";
    });
}

// Tratamento de exceções antes de qualquer outro middleware do pipeline.
app.UseMiddleware<TratamentoExcecoesMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger"))
   .ExcludeFromDescription();

app.Run();
