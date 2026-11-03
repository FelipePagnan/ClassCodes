using ArchitecturalPatterns.AwesomeShop.Core.Repositories;
using MediatR;

namespace ArchitecturalPatterns.AwesomeShop.Application.Commands.AddOrderUpdate
{
    public class AddOrderUpdateCommandHandler : IRequestHandler<AddOrderUpdateCommand>
    {
        private readonly IOrderRepository _repository;

        public AddOrderUpdateCommandHandler(IOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(AddOrderUpdateCommand request, CancellationToken cancellationToken)
        {
            var orderUpdate = request.ToEntity();

            await _repository.AddUpdate(orderUpdate);

            return Unit.Value;
        }
    }
}
