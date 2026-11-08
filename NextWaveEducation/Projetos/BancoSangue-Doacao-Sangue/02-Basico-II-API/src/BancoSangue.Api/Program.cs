using BancoSangue.Api.Dados;
using BancoSangue.Api.Excecoes;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Banco em memória, conforme especificação do nível Básico II.
builder.Services.AddDbContext<BancoSangueContext>(options =>
    options.UseInMemoryDatabase("BancoSangueDb"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<TratamentoExcecoesMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { } // exposto para testes de integração, se necessário
