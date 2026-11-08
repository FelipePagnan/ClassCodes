using ArchitecturalPatterns.AwesomeShop.Core.Repositories;
using MediatR;

namespace ArchitecturalPatterns.AwesomeShop.Application.Commands.AddOrder
{
    public class AddOrderCommandHandler : IRequestHandler<AddOrderCommand, int>
    {
        private readonly IOrderRepository _repository;

        public AddOrderCommandHandler(IOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(AddOrderCommand request, CancellationToken cancellationToken)
        {
            var order = request.ToEntity();

            var id = await _repository.Add(order);

            return id;
        }
    }
}
