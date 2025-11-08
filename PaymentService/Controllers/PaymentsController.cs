using Microsoft.AspNetCore.Mvc;
using PaymentService.RequestModel;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        [HttpPost]
        public IActionResult ProcessPayment([FromBody] PaymentRequest request)
        {
            // Simulate logic:
            // - Fail if amount is too low
            // - Fail if card number ends with an odd digit
            // - Otherwise succeed

            if (request.Amount <= 0)
                return BadRequest(new { success = false, message = "Invalid amount." });

            // Simple simulation rule
            var random = new Random();
            var success = random.Next(1, 10) > 3; // 70% success rate

            if (!success)
            {
                return StatusCode(402, new
                {
                    success = false,
                    message = "Payment failed by mock provider."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Payment processed successfully.",
                transactionId = Guid.NewGuid().ToString(),
                amount = request.Amount
            });
        }
    }
}
