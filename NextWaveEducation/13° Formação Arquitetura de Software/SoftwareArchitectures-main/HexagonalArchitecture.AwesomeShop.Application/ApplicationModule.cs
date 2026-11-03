using HexagonalArchitecture.AwesomeShop.Application.UseCases;
using HexagonalArchitecture.AwesomeShop.Application.UseCases.AddOrder;
using HexagonalArchitecture.AwesomeShop.Application.UseCases.GetAllOrders;
using HexagonalArchitecture.AwesomeShop.Application.UseCases.GetOrderById;
using Microsoft.Extensions.DependencyInjection;

namespace HexagonalArchitecture.AwesomeShop.Application
{
    public static class ApplicationModule
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services
                .AddUseCases();

            return services;
        }

        private static IServiceCollection AddUseCases(this IServiceCollection services)
        {
            services.AddScoped<IUseCase<NoInput, UseCaseResult<GetAllOrdersOutput>>, GetAllOrdersUseCase>();
            services.AddScoped<IUseCase<int, UseCaseResult<GetOrderByIdOutput>>, GetOrderByIdUseCase>();

            services.AddScoped<IAddOrderUseCase, AddOrderUseCase>();

            return services;
        }
    }
}
