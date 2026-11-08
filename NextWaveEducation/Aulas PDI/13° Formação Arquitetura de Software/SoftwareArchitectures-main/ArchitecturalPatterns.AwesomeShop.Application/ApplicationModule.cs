using ArchitecturalPatterns.AwesomeShop.Application.Commands.AddOrder;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ArchitecturalPatterns.AwesomeShop.Application
{
    public static class ApplicationModule
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services
                .AddMediator();

            return services;
        }

        private static IServiceCollection AddMediator(this IServiceCollection services)
        {
            services.AddMediatR(typeof(AddOrderCommand));

            return services;
        }
    }
}
