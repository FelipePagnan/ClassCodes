using HexagonalArchitecture.AwesomeShop.Application.UseCases;
using HexagonalArchitecture.AwesomeShop.Application.UseCases.AddOrder;
using HexagonalArchitecture.AwesomeShop.Application.UseCases.GetAllOrders;
using HexagonalArchitecture.AwesomeShop.Application.UseCases.GetOrderById;
using Microsoft.AspNetCore.Mvc;

namespace HexagonalArchitecture.AwesomeShop.API.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        // Ports HTTP definidos
        [HttpGet]
        public async Task<IActionResult> GetAll([FromServices] IUseCase<NoInput, UseCaseResult<GetAllOrdersOutput>> useCase)
        {
            var result = await useCase.Execute();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, [FromServices] IUseCase<int, UseCaseResult<GetOrderByIdOutput>> useCase)
        {
            var result = await useCase.Execute(id);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post(AddOrderInput input, [FromServices] IAddOrderUseCase useCase)
        {
            var result = await useCase.Execute(input);

            return CreatedAtAction(nameof(GetById), new { id = result }, input);
        }
    }
}
