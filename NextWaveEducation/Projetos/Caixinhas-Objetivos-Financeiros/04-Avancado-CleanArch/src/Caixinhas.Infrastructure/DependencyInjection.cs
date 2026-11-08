using Caixinhas.Application.Common.Interfaces;
using Caixinhas.Domain.Repositories;
using Caixinhas.Infrastructure.BackgroundServices;
using Caixinhas.Infrastructure.Persistence;
using Caixinhas.Infrastructure.Persistence.Repositories;
using Caixinhas.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Caixinhas.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=caixinhas-avancado.db";

        services.AddDbContext<CaixinhasDbContext>(options =>
        {
            options.UseSqlite(connectionString);

            // Transacao exige um ObjetivoFinanceiro e as duas entidades têm
            // filtro de soft delete. O EF avisa sobre isso; aqui o comportamento
            // é intencional, já que remover o objetivo também marca as
            // transações dele como deletadas.
            options.ConfigureWarnings(w =>
                w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning));
        });

        // Repositórios + Unit of Work
        services.AddScoped<IObjetivoRepository, ObjetivoRepository>();
        services.AddScoped<ITransacaoRepository, TransacaoRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Despachante de eventos de domínio
        services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

        // Notificação de metas
        services.AddScoped<INotificadorDeMetas, NotificadorDeMetasLog>();

        // Validação de imagem de capa
        services.AddSingleton<IValidadorDeImagem, ValidadorDeImagem>();

        // HostedService que confere os saldos e avisa sobre prazos vencidos
        services.AddHostedService<MonitorDeObjetivosHostedService>();

        return services;
    }
}
