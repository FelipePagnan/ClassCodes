using ArchitecturalPatterns.AwesomeShop.Core.Repositories;
using MediatR;

namespace ArchitecturalPatterns.AwesomeShop.Application.Queries.GetOrderById
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, GetOrderByIdViewModel>
    {
        private readonly IOrderRepository _repository;

        public GetOrderByIdQueryHandler(IOrderRepository repository)
        {
            _repository = repository;
        }
        public async Task<GetOrderByIdViewModel> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order = await _repository.GetById(request.Id);

            var viewModel = new GetOrderByIdViewModel(order);

            return viewModel;
        }
    }
}
