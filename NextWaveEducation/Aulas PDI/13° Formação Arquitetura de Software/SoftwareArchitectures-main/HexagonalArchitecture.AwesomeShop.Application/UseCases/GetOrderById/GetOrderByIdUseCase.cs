using HexagonalArchitecture.AwesomeShop.Domain.Repositories;

namespace HexagonalArchitecture.AwesomeShop.Application.UseCases.GetOrderById
{
    public class GetOrderByIdUseCase : IUseCase<int, UseCaseResult<GetOrderByIdOutput>>
    {
        private readonly IOrderRepository _repository;

        public GetOrderByIdUseCase(IOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task<UseCaseResult<GetOrderByIdOutput>> Execute(int input)
        {
            var order = await _repository.GetById(input);
            var output = new GetOrderByIdOutput(order);

            return new UseCaseResult<GetOrderByIdOutput>(output, true);
        }
    }
}
