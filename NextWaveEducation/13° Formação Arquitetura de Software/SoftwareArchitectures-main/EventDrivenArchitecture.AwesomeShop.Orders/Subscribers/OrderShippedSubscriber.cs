using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;
using EventDrivenArchitecture.AwesomeShop.Orders.Events;
using EventDrivenArchitecture.AwesomeShop.Orders.Repositories;
using EventDrivenArchitecture.AwesomeShop.Orders.Enums;

namespace EventDrivenArchitecture.AwesomeShop.Orders.Subscribers
{
    public class OrderShippedSubscriber : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private const string Queue = "orders-service/order-created";
        private const string RoutingKeySubscribe = "order-shipped";
        private readonly IServiceProvider _serviceProvider;
        private const string Exchange = "orders-service";
        private const string WarehouseExchange = "warehouse-service";
        public OrderShippedSubscriber(IServiceProvider serviceProvider)
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            _connection = connectionFactory.CreateConnection("orders-service-order-shipped-consumer");

            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(Exchange, "direct", false, true, null);
            _channel.ExchangeDeclare(WarehouseExchange, "direct", false, true, null);

            _channel.QueueDeclare(
                queue: Queue,
                durable: false,
                exclusive: false,
                autoDelete: true,
                null);

            _channel.QueueBind(Queue, WarehouseExchange, RoutingKeySubscribe);

            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, eventArgs) =>
            {
                var contentArray = eventArgs.Body.ToArray();
                var contentString = Encoding.UTF8.GetString(contentArray);
                var orderShippedEvent = JsonConvert.DeserializeObject<OrderShippedEvent>(contentString);

                Console.WriteLine($"Message OrderShippedEvent received with Id {orderShippedEvent.Id}");

                UpdateOrderStatus(orderShippedEvent);

                _channel.BasicAck(eventArgs.DeliveryTag, false);
            };

            _channel.BasicConsume(Queue, false, consumer);

            return Task.CompletedTask;
        }

        public void UpdateOrderStatus(OrderShippedEvent @event)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var repository = scope.ServiceProvider.GetService<IOrderRepository>();

                repository.UpdateOrderStatus(@event.Id, OrderStatus.Shipped);
            }
        }
    }
}
