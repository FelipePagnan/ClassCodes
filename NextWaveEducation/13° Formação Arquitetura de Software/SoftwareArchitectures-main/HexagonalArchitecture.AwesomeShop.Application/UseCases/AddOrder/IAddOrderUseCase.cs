namespace HexagonalArchitecture.AwesomeShop.Application.UseCases.AddOrder
{
    public interface IAddOrderUseCase
    {
        Task<int> Execute(AddOrderInput input);
    }
}
