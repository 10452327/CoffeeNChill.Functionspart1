using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeNChill.Gateway.Controllers
{
    [ApiController]
    [Route("api/documents")]
    [Authorize(Roles = "Admin,Staff")]
    public class DocumentsController : ControllerBase
    {
        [HttpPost]
        public IActionResult UploadDocument()
        {
            return Ok("Document uploaded.");
        }

        [HttpGet]
        public IActionResult GetDocuments()
        {
            return Ok("Documents retrieved.");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteDocument(int id)
        {
            return Ok($"Document {id} deleted.");
        }
    }
}
