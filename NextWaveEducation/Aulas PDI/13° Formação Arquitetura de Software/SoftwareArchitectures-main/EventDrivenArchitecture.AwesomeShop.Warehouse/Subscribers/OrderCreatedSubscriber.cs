using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using EventDrivenArchitecture.AwesomeShop.Warehouse.Events;
using System.Text;
using Newtonsoft.Json;
using EventDrivenArchitecture.AwesomeShop.Warehouse.Integrations;

namespace EventDrivenArchitecture.AwesomeShop.Warehouse.Subscribers
{
    public class OrderCreatedSubscriber : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private const string Queue = "warehouse-service/order-created";
        private const string RoutingKeySubscribe = "order-created";
        private const string RoutingKeyPublish = "order-shipped";
        private readonly IServiceProvider _serviceProvider;
        private const string Exchange = "warehouse-service";
        private const string OrdersExchange = "orders-service";
        public OrderCreatedSubscriber(IServiceProvider serviceProvider)
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            _connection = connectionFactory.CreateConnection("warehouse-service-order-created-consumer");

            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: Queue,
                durable: false,
                exclusive: false,
                autoDelete: true,
                null);

            _channel.QueueBind(Queue, OrdersExchange, RoutingKeySubscribe);

            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, eventArgs) =>
            {
                var contentArray = eventArgs.Body.ToArray();
                var contentString = Encoding.UTF8.GetString(contentArray);
                var orderCreatedEvent = JsonConvert.DeserializeObject<OrderCreatedEvent>(contentString);

                Console.WriteLine($"Message OrderCreatedEvent received with Id {orderCreatedEvent.Id}");

                ShipOrder(orderCreatedEvent);

                _channel.BasicAck(eventArgs.DeliveryTag, false);

                var orderShipped = new OrderShippedEvent(orderCreatedEvent.Id);
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(orderShipped));

                _channel.BasicPublish(Exchange, RoutingKeyPublish, null, jsonBytes);
            };

            _channel.BasicConsume(Queue, false, consumer);

            return Task.CompletedTask;
        }

        public void ShipOrder(OrderCreatedEvent @event)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var shippingService = scope.ServiceProvider.GetService<IShippingService>();

                shippingService.ShipOrder(@event);
            }
        }
    }
}
