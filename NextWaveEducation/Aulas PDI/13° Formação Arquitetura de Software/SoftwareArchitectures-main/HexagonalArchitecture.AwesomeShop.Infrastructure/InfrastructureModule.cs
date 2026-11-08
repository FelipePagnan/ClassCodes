using HexagonalArchitecture.AwesomeShop.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HexagonalArchitecture.AwesomeShop.Infrastructure
{
    public static class InfrastructureModule
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services
                .AddRepositories();

            return services;
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IOrderRepository, IOrderRepository>();

            return services;
        }
    }
}
