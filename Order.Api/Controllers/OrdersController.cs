using System;
using Order.Api.Persistence;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Order.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;

        public OrdersController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var data = await _orderRepository
                .GetOrdersAsync();

            return Ok(data);
        }

        [HttpGet]
        [Route("{orderId}", Name = "GetByOrderId")]
        public async Task<IActionResult> GetOrderById(string orderId)
        {
            var order = await _orderRepository
                .GetOrderAsync(Guid.Parse(orderId));

            if (order == null)
                return NotFound();

            return Ok(order);
        }
    }
}