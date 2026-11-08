using System.Text.Json.Serialization;
using Livros.Api.Middlewares;
using Livros.Application;
using Livros.Domain.Entities;
using Livros.Domain.Enums;
using Livros.Infrastructure;
using Livros.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Gerenciador de Avaliação de Livros - Nível Avançado",
        Version = "v1",
        Description = "Arquitetura Limpa, CQRS (MediatR), Unit of Work, " +
                      "Domain Events, HostedService e testes unitários."
    });

    var xml = Path.Combine(AppContext.BaseDirectory, "Livros.Api.xml");
    if (File.Exists(xml)) c.IncludeXmlComments(xml);
});

// Camadas da Arquitetura Limpa: a API só conhece Application e Infrastructure.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Cria o banco SQLite e semeia dados de exemplo na primeira execução.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LivrosDbContext>();
    context.Database.EnsureCreated();
    SemearDados(context);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Livros API v1");
        c.RoutePrefix = "swagger";
    });
}

// Tratamento de exceções antes de qualquer outro middleware do pipeline.
app.UseMiddleware<TratamentoExcecoesMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();

static void SemearDados(LivrosDbContext context)
{
    if (context.Livros.Any()) return;

    var ddd = new Livro(
        "Domain-Driven Design",
        "Tackling complexity in the heart of software.",
        "978-0321125217",
        "Eric Evans",
        "Addison-Wesley",
        GeneroLivroEnum.Tecnico,
        2003,
        560);

    var duna = new Livro(
        "Duna",
        "O planeta deserto Arrakis e a especiaria mais valiosa do universo.",
        "978-8576572008",
        "Frank Herbert",
        "Aleph",
        GeneroLivroEnum.FiccaoCientifica,
        1965,
        680);

    var hobbit = new Livro(
        "O Hobbit",
        "A jornada de Bilbo Bolseiro até a Montanha Solitária.",
        "978-8595084742",
        "J. R. R. Tolkien",
        "HarperCollins",
        GeneroLivroEnum.Fantasia,
        1937,
        336);

    var ana = new Usuario("Ana Souza", "ana.souza@example.com");
    var bruno = new Usuario("Bruno Lima", "bruno.lima@example.com");

    context.Livros.AddRange(ddd, duna, hobbit);
    context.Usuarios.AddRange(ana, bruno);
    context.SaveChanges();

    var hoje = DateTime.Today;

    ddd.AdicionarAvaliacao(ana.Id, 5,
        "Mudou a forma como penso modelagem de software.",
        hoje.AddDays(-60), hoje.AddDays(-20));

    ddd.AdicionarAvaliacao(bruno.Id, 4,
        "Excelente, mas denso em algumas partes.",
        hoje.AddDays(-45), hoje.AddDays(-10));

    duna.AdicionarAvaliacao(ana.Id, 5,
        "Uma obra-prima da ficção científica.",
        hoje.AddDays(-30), hoje.AddDays(-5));

    // No seed a média é calculada direto; em runtime isso acontece
    // pelo handler do evento AvaliacaoCadastradaEvent.
    ddd.RecalcularNotaMedia();
    duna.RecalcularNotaMedia();

    context.SaveChanges();

    // Os eventos do seed não são despachados; limpa para não vazar ao ChangeTracker.
    foreach (var entidade in new Livros.Domain.Common.BaseEntity[] { ddd, duna, hobbit, ana, bruno })
        entidade.LimparEventos();
}
