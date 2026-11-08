using System.Text.Json.Serialization;
using Livros.Api.Dados;
using Livros.Api.Entidades;
using Livros.Api.Enums;
using Livros.Api.Excecoes;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Banco em memória, conforme especificação do nível Básico II.
builder.Services.AddDbContext<LivrosContext>(options =>
    options.UseInMemoryDatabase("LivrosDb"));

var app = builder.Build();

// Dados de exemplo para facilitar os testes.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LivrosContext>();
    SemearDados(context);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<TratamentoExcecoesMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();

static void SemearDados(LivrosContext context)
{
    if (context.Livros.Any()) return;

    var ddd = new Livro
    {
        Titulo = "Domain-Driven Design",
        Autor = "Eric Evans",
        ISBN = "978-0321125217",
        Editora = "Addison-Wesley",
        Descricao = "Tackling complexity in the heart of software.",
        Genero = GeneroLivroEnum.Tecnico,
        AnoDePublicacao = 2003,
        QuantidadePaginas = 560
    };

    var duna = new Livro
    {
        Titulo = "Duna",
        Autor = "Frank Herbert",
        ISBN = "978-8576572008",
        Editora = "Aleph",
        Descricao = "O planeta deserto Arrakis e a especiaria mais valiosa do universo.",
        Genero = GeneroLivroEnum.FiccaoCientifica,
        AnoDePublicacao = 1965,
        QuantidadePaginas = 680
    };

    var ana = new Usuario { Nome = "Ana Souza", Email = "ana.souza@example.com" };
    var bruno = new Usuario { Nome = "Bruno Lima", Email = "bruno.lima@example.com" };

    context.Livros.AddRange(ddd, duna);
    context.Usuarios.AddRange(ana, bruno);
    context.SaveChanges();

    var hoje = DateTime.Today;

    context.Avaliacoes.AddRange(
        new Avaliacao
        {
            IdLivro = ddd.Id, IdUsuario = ana.Id, Nota = 5,
            Descricao = "Mudou a forma como penso modelagem de software.",
            DataInicioLeitura = hoje.AddDays(-60), DataFimLeitura = hoje.AddDays(-20)
        },
        new Avaliacao
        {
            IdLivro = ddd.Id, IdUsuario = bruno.Id, Nota = 4,
            Descricao = "Excelente, mas denso em algumas partes.",
            DataInicioLeitura = hoje.AddDays(-45), DataFimLeitura = hoje.AddDays(-10)
        },
        new Avaliacao
        {
            IdLivro = duna.Id, IdUsuario = ana.Id, Nota = 5,
            Descricao = "Uma obra-prima da ficção científica.",
            DataInicioLeitura = hoje.AddDays(-30), DataFimLeitura = hoje.AddDays(-5)
        });

    context.SaveChanges();

    // Calcula a nota média inicial dos livros semeados.
    foreach (var livro in context.Livros.Include(l => l.Avaliacoes))
        livro.RecalcularNotaMedia();

    context.SaveChanges();
}
