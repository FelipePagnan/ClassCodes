using System.Text.Json.Serialization;
using Caixinhas.Api.Middlewares;
using Caixinhas.Application;
using Caixinhas.Domain.Common;
using Caixinhas.Domain.Entities;
using Caixinhas.Domain.Enums;
using Caixinhas.Infrastructure;
using Caixinhas.Infrastructure.Persistence;
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
        Title = "Gerenciador de Objetivos Financeiros - Nível Avançado",
        Version = "v1",
        Description = "Arquitetura Limpa, CQRS (MediatR), Unit of Work com transação " +
                      "de banco, Domain Events, HostedService e testes unitários."
    });

    var xml = Path.Combine(AppContext.BaseDirectory, "Caixinhas.Api.xml");
    if (File.Exists(xml)) c.IncludeXmlComments(xml);
});

// Camadas da Arquitetura Limpa: a API só conhece Application e Infrastructure.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

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
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Caixinhas API v1");
        c.RoutePrefix = "swagger";
    });
}

// Tratamento de exceções antes de qualquer outro middleware do pipeline.
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

    // As transações passam pela entidade, então o ValorTotal e o status são
    // atualizados do mesmo jeito que numa requisição real.
    viagem.RegistrarTransacao(1_200m, TipoTransacao.Deposito, hoje);
    viagem.RegistrarTransacao(1_500.50m, TipoTransacao.Deposito, hoje);
    viagem.RegistrarTransacao(300m, TipoTransacao.Saque, hoje);

    reserva.RegistrarTransacao(2_000m, TipoTransacao.Deposito, hoje);
    reserva.RegistrarTransacao(2_000m, TipoTransacao.Deposito, hoje);

    notebook.RegistrarTransacao(8_000m, TipoTransacao.Deposito, hoje);

    context.SaveChanges();

    // O seed não passa pela Unit of Work, então os eventos acumulados aqui
    // não são despachados: limpa para não vazarem num commit posterior.
    foreach (BaseEntity entidade in new BaseEntity[] { viagem, reserva, notebook })
        entidade.LimparEventos();
}
