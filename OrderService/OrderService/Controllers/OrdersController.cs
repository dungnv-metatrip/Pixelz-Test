using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using OrderService.RequestModel;
using OrderService.Repositories.Interfaces;
using Shared.RabbitMq;
using Shared.Contracts.Events;
using OrderService.Models;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _repository;
        private readonly HttpClient _httpClient;
        private readonly Shared.RabbitMq.IEventPublisher _publisher;
        private readonly IConfiguration _configuration;

        public OrdersController(IOrderRepository repository, IConfiguration configuration, IHttpClientFactory httpClientFactory, IEventPublisher publisher)
        {
            _repository = repository;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
            _publisher = publisher;
        }

        [HttpGet]
        public IActionResult GetOrders([FromQuery] string? name)
        {
            var result = string.IsNullOrEmpty(name)
                ? _repository.GetAll()
                : _repository.SearchByName(name);

            return Ok(result);
        }
        [HttpPost("create-order")]
        public IActionResult CreateOrder([FromBody] OrderDto orderDto)
        {
            if (orderDto == null)
                return BadRequest("Order data is required.");

            var order = new Order
            {
                Id = Guid.NewGuid().ToString(),
                Name = orderDto.Name,
                Amount = orderDto.Amount,
                Email = orderDto.Email
            };

            _repository.Add(order);

            return CreatedAtAction(nameof(GetOrders), new { id = order.Id }, order);
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
               _configuration.GetSection("PaymentSvcEndpoint").Value,
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
            // Step 3: Send mesage to EmailService And InvoiceService
            if (order.Status == "Paid")
            {
                await _publisher.ConnectAsync();
                await _publisher.DeclareExchangeAsync("pixelz.exchange");
                await _publisher.PublishAsync("", new OrderCheckedOutEvent(order.Id, order.Name, order.Amount, order.Email));
            }

            //Step 4: Push to Production System
            var productionPayload = JsonSerializer.Serialize(new
            {
                orderId = order.Id,
                status = "ReadyForProduction"
            });

            var productionResponse = await _httpClient.PostAsync(
                 _configuration.GetSection("productionSvcEndpoint").Value,
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
