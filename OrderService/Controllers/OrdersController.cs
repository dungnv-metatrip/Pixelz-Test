using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using OrderService.Repositories;
using OrderService.RequestModel;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderRepository _repository;
        private readonly HttpClient _httpClient;


        public OrdersController(OrderRepository repository, IHttpClientFactory httpClientFactory)
        {
            _repository = repository;
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpGet]
        public IActionResult GetOrders([FromQuery] string? name)
        {
            var result = string.IsNullOrEmpty(name)
                ? _repository.GetAll()
                : _repository.SearchByName(name);

            return Ok(result);
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            var order = _repository.GetById(request.OrderId);
            if (order == null)
                return NotFound($"Order {request.OrderId} not found.");

            // Step 1: Mock Payment Service
            var paymentPayload = JsonSerializer.Serialize(new { amount = order.Amount });
            var paymentResponse = await _httpClient.PostAsync(
                "http://localhost:5002/api/payments",
                new StringContent(paymentPayload, Encoding.UTF8, "application/json")
            );

            var paymentSuccess = paymentResponse.IsSuccessStatusCode;

            if (!paymentSuccess)
            {
                order.Status = "PaymentFailed";
                _repository.Update(order);
                return BadRequest("Payment failed.");
            }

            // Step 2: Update order
            order.Status = "Paid";
            _repository.Update(order);

            // Step 3: Notify Email Service
            var emailPayload = JsonSerializer.Serialize(new
            {
                to = "client@gmail.com",
                subject = $"Your order {order.Name} was successful",
                body = $"Order {order.Id} has been paid successfully."
            });
            await _httpClient.PostAsync("http://localhost:5003/api/emails",
                new StringContent(emailPayload, Encoding.UTF8, "application/json"));

            // Step 4: Push to Production System
            var productionPayload = JsonSerializer.Serialize(new
            {
                orderId = order.Id,
                status = "ReadyForProduction"
            });
            var productionResponse = await _httpClient.PostAsync(
                "http://localhost:5004/api/production",
                new StringContent(productionPayload, Encoding.UTF8, "application/json")
            );

            if (productionResponse.IsSuccessStatusCode)
            {
                order.Status = "SentToProduction";
                _repository.Update(order);
            }

            return Ok(order);
        }

    }
}
