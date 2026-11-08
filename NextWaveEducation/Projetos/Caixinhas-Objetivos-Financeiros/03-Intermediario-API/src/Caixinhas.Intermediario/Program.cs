using System.Text.Json.Serialization;
using Caixinhas.Intermediario.Aplicacao.Servicos;
using Caixinhas.Intermediario.Aplicacao.Validadores;
using Caixinhas.Intermediario.Dominio.Entidades;
using Caixinhas.Intermediario.Dominio.Enums;
using Caixinhas.Intermediario.Dominio.Interfaces;
using Caixinhas.Intermediario.Infraestrutura.Dados;
using Caixinhas.Intermediario.Infraestrutura.Repositorios;
using Caixinhas.Intermediario.Infraestrutura.Servicos;
using Caixinhas.Intermediario.Middlewares;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
        Title = "Gerenciador de Objetivos Financeiros - Nível Intermediário",
        Version = "v1",
        Description = "Repository, FluentValidation, middleware de exceções, " +
                      "InputModel/ViewModel, IEntityTypeConfiguration, SQLite, " +
                      "soft delete por query filter, simulação de evolução e upload de capa."
    });
});

// SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=caixinhas.db";

builder.Services.AddDbContext<CaixinhasDbContext>(options =>
{
    options.UseSqlite(connectionString);

    // Transacao exige um ObjetivoFinanceiro e as duas entidades têm filtro de
    // soft delete. O EF avisa sobre isso; aqui o comportamento é intencional,
    // já que remover o objetivo também marca as transações como deletadas.
    options.ConfigureWarnings(w =>
        w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning));
});

// Padrão Repository
builder.Services.AddScoped<IObjetivoRepository, ObjetivoRepository>();
builder.Services.AddScoped<ITransacaoRepository, TransacaoRepository>();

// Serviços de aplicação
builder.Services.AddScoped<IObjetivoAppService, ObjetivoAppService>();
builder.Services.AddScoped<ITransacaoAppService, TransacaoAppService>();
builder.Services.AddScoped<IRelatorioAppService, RelatorioAppService>();

// Validação de imagem de capa
builder.Services.AddSingleton<IValidadorDeImagem, ValidadorDeImagem>();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CriarObjetivoInputModelValidator>();

var app = builder.Build();

// Cria o banco SQLite e semeia dados de exemplo na primeira execução.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CaixinhasDbContext>();
    context.Database.EnsureCreated();
    SemearDados(context);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Caixinhas API v1"));
}

// Middleware de tratamento de exceções antes do pipeline de rotas.
app.UseMiddleware<TratamentoExcecoesMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();

static void SemearDados(CaixinhasDbContext context)
{
    if (context.Objetivos.Any()) return;

    var hoje = DateTime.Today;

    var viagem = new ObjetivoFinanceiro("Viagem para o Japão", 20_000m, hoje.AddMonths(18), 1_200m);
    var reserva = new ObjetivoFinanceiro("Reserva de emergência", 30_000m, null, 2_000m);
    var notebook = new ObjetivoFinanceiro("Notebook novo", 8_000m, hoje.AddMonths(4));

    context.Objetivos.AddRange(viagem, reserva, notebook);
    context.SaveChanges();

    // As transações passam pela entidade, então o ValorTotal e o status
    // são atualizados do mesmo jeito que numa requisição real.
    viagem.RegistrarTransacao(1_200m, TipoTransacao.Deposito, hoje);
    viagem.RegistrarTransacao(1_500.50m, TipoTransacao.Deposito, hoje);
    viagem.RegistrarTransacao(300m, TipoTransacao.Saque, hoje);

    reserva.RegistrarTransacao(2_000m, TipoTransacao.Deposito, hoje);
    reserva.RegistrarTransacao(2_000m, TipoTransacao.Deposito, hoje);

    notebook.RegistrarTransacao(8_000m, TipoTransacao.Deposito, hoje);

    context.SaveChanges();
}
