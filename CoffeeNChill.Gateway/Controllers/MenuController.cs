using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CoffeeNChill.Gateway.Controllers
{
    [ApiController]
    [Route("api/menu")]
    [Authorize(Roles = "Admin,Staff")]
    public class MenuController : ControllerBase
    {
        // Public: anyone can view the menu
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetMenu()
        {
            return Ok("Menu is publicly accessible.");
        }

        // Admin and Staff only
        [HttpPost]
        public IActionResult CreateMenuItem()
        {
            return Ok("Menu item created.");
        }

        [HttpPut("{id}")]
        public IActionResult UpdateMenuItem(int id)
        {
            return Ok($"Menu item {id} updated.");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteMenuItem(int id)
        {
            return Ok($"Menu item {id} deleted.");
        }
    }
}


