using CleanArchitecture.AwesomeShop.Application.InputModels;
using CleanArchitecture.AwesomeShop.Application.ViewModels;
using CleanArchitecture.AwesomeShop.Core.DomainServices;
using CleanArchitecture.AwesomeShop.Core.Repositories;

namespace CleanArchitecture.AwesomeShop.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repository;

        public OrderService(IOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task<ResultBase<int>> Add(AddOrderInputModel input)
        {
            var order = input.ToEntity();
            
            var id = await _repository.Add(order);

            return new ResultBase<int>(id);
        }

        public async Task<ResultBase<GetAllOrdersViewModel>> GetAll()
        {
            var orders = await _repository.GetAll();

            var viewModel = new GetAllOrdersViewModel(orders);

            return new ResultBase<GetAllOrdersViewModel>(viewModel);
        }

        public async Task<ResultBase<GetOrderByIdViewModel>> GetById(int id)
        {
            var order = await _repository.GetById(id);

            var viewModel = new GetOrderByIdViewModel(order);

            return new ResultBase<GetOrderByIdViewModel>(viewModel);
        }
    }
}
