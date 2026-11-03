using CleanArchitecture.AwesomeShop.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.AwesomeShop.Application
{
    public static class ApplicationModule
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services
                .AddServices();

            return services;
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IOrderService, OrderService>();

            return services;
        }
    }
}
