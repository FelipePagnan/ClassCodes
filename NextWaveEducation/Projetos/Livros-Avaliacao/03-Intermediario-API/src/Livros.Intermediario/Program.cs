using System.Text.Json.Serialization;
using FluentValidation;
using Livros.Intermediario.Aplicacao.Servicos;
using Livros.Intermediario.Aplicacao.Validadores;
using Livros.Intermediario.Dominio.Entidades;
using Livros.Intermediario.Dominio.Enums;
using Livros.Intermediario.Dominio.Interfaces;
using Livros.Intermediario.Infraestrutura.Dados;
using Livros.Intermediario.Infraestrutura.Repositorios;
using Livros.Intermediario.Infraestrutura.Servicos;
using Livros.Intermediario.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Gerenciador de Avaliação de Livros - Nível Intermediário",
        Version = "v1",
        Description = "Repository, FluentValidation, middleware de exceções, " +
                      "InputModel/ViewModel, IEntityTypeConfiguration, SQLite, " +
                      "integração com o Google Books e upload de capa."
    });
});

// SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=livros.db";

builder.Services.AddDbContext<LivrosDbContext>(options =>
    options.UseSqlite(connectionString));

// Padrão Repository
builder.Services.AddScoped<ILivroRepository, LivroRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAvaliacaoRepository, AvaliacaoRepository>();

// Serviços de aplicação
builder.Services.AddScoped<ILivroAppService, LivroAppService>();
builder.Services.AddScoped<IUsuarioAppService, UsuarioAppService>();
builder.Services.AddScoped<IAvaliacaoAppService, AvaliacaoAppService>();

// Integração com a API externa de livros
builder.Services.AddHttpClient<IConsultaLivroExternoService, GoogleBooksService>(client =>
{
    client.BaseAddress = new Uri("https://www.googleapis.com/books/v1/");
    client.Timeout = TimeSpan.FromSeconds(15);
});

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CriarLivroInputModelValidator>();

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
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Livros API v1"));
}

// Middleware de tratamento de exceções antes do pipeline de rotas.
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

    context.SaveChanges();
}
