using ArchitecturalPatterns.AwesomeShop.Application.Commands.AddOrder;
using ArchitecturalPatterns.AwesomeShop.Application.Commands.AddOrderUpdate;
using ArchitecturalPatterns.AwesomeShop.Application.Queries.GetOrderById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ArchitecturalPatterns.AwesomeShop.API.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetOrderByIdQuery(id);

            var viewModel = await _mediator.Send(query);

            return Ok(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Post(AddOrderCommand command)
        {
            var id = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetById), new { id }, command);
        }

        [HttpPost("{id}/updates")]
        public async Task<IActionResult> PostUpdate(AddOrderUpdateCommand command)
        {
            await _mediator.Send(command);

            return NoContent();
        }
    }
}
