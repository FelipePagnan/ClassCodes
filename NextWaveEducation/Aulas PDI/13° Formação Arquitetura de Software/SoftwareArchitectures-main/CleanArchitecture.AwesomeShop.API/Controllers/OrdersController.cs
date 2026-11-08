using CleanArchitecture.AwesomeShop.Application.InputModels;
using CleanArchitecture.AwesomeShop.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.AwesomeShop.API.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrdersController(IOrderService service)
        {
            _service = service;
        }
       
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAll();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetById(id);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post(AddOrderInputModel input)
        {
            var result = await _service.Add(input);

            return CreatedAtAction(nameof(GetById), new { id = result }, input);
        }
    }
}
