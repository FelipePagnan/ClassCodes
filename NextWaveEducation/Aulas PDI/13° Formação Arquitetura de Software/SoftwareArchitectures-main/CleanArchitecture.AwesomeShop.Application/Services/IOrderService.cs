using CleanArchitecture.AwesomeShop.Application.InputModels;
using CleanArchitecture.AwesomeShop.Application.ViewModels;

namespace CleanArchitecture.AwesomeShop.Application.Services
{
    public interface IOrderService
    {
        Task<ResultBase<int>> Add(AddOrderInputModel input);
        Task<ResultBase<GetAllOrdersViewModel>> GetAll();
        Task<ResultBase<GetOrderByIdViewModel>> GetById(int id);
    }
}
