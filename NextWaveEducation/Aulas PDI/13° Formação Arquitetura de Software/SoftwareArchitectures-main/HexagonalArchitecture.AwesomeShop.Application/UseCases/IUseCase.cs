namespace HexagonalArchitecture.AwesomeShop.Application.UseCases
{
    public interface IUseCase<T, U> where U: UseCaseResult
    {
        Task<U> Execute(T input = default(T));
    }
}
