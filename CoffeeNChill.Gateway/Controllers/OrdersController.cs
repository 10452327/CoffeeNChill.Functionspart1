using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeNChill.Gateway.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize(Roles = "Admin,Staff")]
    public class OrdersController : ControllerBase
    {
        [HttpPost("queue")]
        [AllowAnonymous]
        public IActionResult AddToQueue()
        {
            return Ok("Order added to queue.");
        }

        [HttpPut("status")]
        public IActionResult UpdateOrderStatus()
        {
            return Ok("Order status updated.");
        }
    }
}
