using HexagonalArchitecture.AwesomeShop.Domain.Repositories;

namespace HexagonalArchitecture.AwesomeShop.Application.UseCases.GetAllOrders
{
    public class GetAllOrdersUseCase : IUseCase<NoInput, UseCaseResult<GetAllOrdersOutput>>
    {
        private readonly IOrderRepository _repository;

        public GetAllOrdersUseCase(IOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task<UseCaseResult<GetAllOrdersOutput>> Execute(NoInput input = null)
        {
            var orders = await _repository.GetAll();

            var output = new GetAllOrdersOutput(orders);

            return new UseCaseResult<GetAllOrdersOutput>(output, true);
        }
    }
}
