using Microsoft.AspNetCore.Mvc;
using ProductionAdapterService.RequestModel;

namespace ProductionAdapterService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductionController : ControllerBase
    {
        [HttpPost]
        public IActionResult PushToProduction([FromBody] ProductionRequest request)
        {
            if (string.IsNullOrEmpty(request.OrderId))
                return BadRequest(new { success = false, message = "OrderId is required." });

            // Simulate internal system call
            var random = new Random();
            var success = random.Next(1, 10) > 2; // 80% success rate

            if (!success)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Failed to push order {request.OrderId} to production system."
                });
            }

            // Normally this is where we’d call an internal API via HttpClient.
            // Example:
            // await _httpClient.PostAsync("http://internal-system/api/orders", ...)

            return Ok(new
            {
                success = true,
                message = $"Order {request.OrderId} successfully pushed to production.",
                productionId = Guid.NewGuid().ToString(),
                status = request.Status
            });
        }

    }
}
