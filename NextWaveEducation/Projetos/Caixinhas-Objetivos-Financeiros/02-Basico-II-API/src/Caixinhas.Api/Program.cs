using System.Text.Json.Serialization;
using Caixinhas.Api.Dados;
using Caixinhas.Api.Entidades;
using Caixinhas.Api.Enums;
using Caixinhas.Api.Excecoes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Banco em memória, conforme especificação do nível Básico II.
builder.Services.AddDbContext<CaixinhasContext>(options =>
{
    options.UseInMemoryDatabase("CaixinhasDb");

    // Transacao exige um ObjetivoFinanceiro e as duas entidades têm filtro de
    // soft delete. O EF avisa sobre isso; aqui o comportamento é intencional,
    // já que remover o objetivo também marca as transações como deletadas.
    options.ConfigureWarnings(w =>
        w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning));
});

var app = builder.Build();

// Dados de exemplo para facilitar os testes.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CaixinhasContext>();
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

static void SemearDados(CaixinhasContext context)
{
    if (context.Objetivos.Any()) return;

    var hoje = DateTime.Today;

    var viagem = new ObjetivoFinanceiro
    {
        Titulo = "Viagem para o Japão",
        QuantidadeAlvo = 20_000m,
        Prazo = hoje.AddMonths(18),
        QuantidadeIdealAporteMensal = 1_200m,
        Status = StatusObjetivo.EmProgresso
    };

    var reserva = new ObjetivoFinanceiro
    {
        Titulo = "Reserva de emergência",
        QuantidadeAlvo = 30_000m,
        QuantidadeIdealAporteMensal = 2_000m,
        Status = StatusObjetivo.EmProgresso
    };

    var notebook = new ObjetivoFinanceiro
    {
        Titulo = "Notebook novo",
        QuantidadeAlvo = 8_000m,
        Prazo = hoje.AddMonths(4),
        Status = StatusObjetivo.EmProgresso
    };

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
