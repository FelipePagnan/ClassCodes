using Livros.Application.Common.Interfaces;
using Livros.Domain.Repositories;
using Livros.Infrastructure.BackgroundServices;
using Livros.Infrastructure.Persistence;
using Livros.Infrastructure.Persistence.Repositories;
using Livros.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Livros.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=livros-avancado.db";

        services.AddDbContext<LivrosDbContext>(options =>
            options.UseSqlite(connectionString));

        // Repositórios + Unit of Work
        services.AddScoped<ILivroRepository, LivroRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IAvaliacaoRepository, AvaliacaoRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Despachante de eventos de domínio
        services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

        // Validação de imagens de capa
        services.AddSingleton<IValidadorDeImagem, ValidadorDeImagem>();

        // Integração com a API externa de livros
        services.AddHttpClient<IConsultaLivroExternoService, GoogleBooksService>(client =>
        {
            client.BaseAddress = new Uri("https://www.googleapis.com/books/v1/");
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        // HostedService que confere as notas médias periodicamente
        services.AddHostedService<RecalculoNotasHostedService>();

        return services;
    }
}
