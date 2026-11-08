using BancoSangue.Application.Common.Interfaces;
using BancoSangue.Domain.Repositories;
using BancoSangue.Infrastructure.BackgroundServices;
using BancoSangue.Infrastructure.Persistence;
using BancoSangue.Infrastructure.Persistence.Repositories;
using BancoSangue.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BancoSangue.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=bancosangue-avancado.db";

        services.AddDbContext<BancoSangueDbContext>(options =>
            options.UseSqlite(connectionString));

        // Repositórios + Unit of Work
        services.AddScoped<IDoadorRepository, DoadorRepository>();
        services.AddScoped<IDoacaoRepository, DoacaoRepository>();
        services.AddScoped<IEstoqueSangueRepository, EstoqueSangueRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Despachante de eventos de domínio
        services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

        // Notificação de estoque baixo
        services.AddScoped<INotificadorEstoque, NotificadorEstoqueLog>();

        // Integração com a API externa de CEP
        services.AddHttpClient<ICepService, ViaCepService>(client =>
        {
            client.BaseAddress = new Uri("https://viacep.com.br/ws/");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        // HostedService que monitora o estoque periodicamente
        services.AddHostedService<MonitorEstoqueHostedService>();

        return services;
    }
}
