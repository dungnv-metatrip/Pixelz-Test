using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Shared.RabbitMq
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(string routingKey, T message);
        Task ConnectAsync();
        Task DeclareExchangeAsync(string exchangeName, string exchangeType = ExchangeType.Fanout);
    }
    public class RabbitEventPublisher : IEventPublisher, IAsyncDisposable
    {
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly IConnection _conn;
        private readonly string _exchange = "pixelz.exchange";
        public RabbitEventPublisher(IConfiguration cfg)
        {
            _factory = new ConnectionFactory { HostName = "localhost" };
        }

        /// <summary>
        /// Connects to RabbitMQ and creates a channel asynchronously.
        /// </summary>
        public async Task ConnectAsync()
        {
            _connection = await _factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
        }

        /// <summary>
        /// Declare an exchange.
        /// </summary>
        public async Task DeclareExchangeAsync(string exchangeName, string exchangeType = ExchangeType.Fanout)
        {
            if (_channel == null)
                throw new InvalidOperationException("Call ConnectAsync() first.");

            await _channel.ExchangeDeclareAsync(exchange: exchangeName, type: exchangeType);
        }

        ///// <summary>
        ///// Publish a message asynchronously.
        ///// </summary>
        //public async Task PublishAsync(string exchangeName, string message)
        //{
        //    if (_channel == null)
        //        throw new InvalidOperationException("Call ConnectAsync() first.");

        //    var body = Encoding.UTF8.GetBytes(message);
        //    await _channel.BasicPublishAsync(exchange: exchangeName, routingKey: string.Empty, body: body);
        //}
        public async Task PublishAsync<T>(string routingKey, T message)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            await _channel.BasicPublishAsync(_exchange, routingKey, body);
        }

        /// <summary>
        /// Close connection and channel.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_channel != null)
            {
                await _channel.CloseAsync();
                _channel.Dispose();
            }

            if (_connection != null)
            {
                await _connection.CloseAsync();
                _connection.Dispose();
            }
        }
    }
    public static class RabbitMqExtensions
    {
        public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration cfg)
        {
            services.AddSingleton<IEventPublisher, RabbitEventPublisher>();
            return services;
        }
    }

}