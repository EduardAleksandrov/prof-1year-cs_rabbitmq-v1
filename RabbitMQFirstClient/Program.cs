using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
internal class Program
{
    private static async Task Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost", UserName = "guest", Password = "guest" };
        using (var connection = await factory.CreateConnectionAsync())
        using (var channel = await connection.CreateChannelAsync())
        {
            await channel.QueueDeclareAsync(queue: "hello",
                                            durable: false,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($" [x] Received {message}");
                // Acknowledge the message
                await ((AsyncEventingBasicConsumer)sender).Channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
            };
            await channel.BasicConsumeAsync(queue: "hello",
                                            autoAck: false,
                                            consumer: consumer);
            Console.WriteLine("Waiting for messages...");
            Console.ReadLine();
        }
        // Example two
        await JsonExample2();
    }
    private static async Task JsonExample()
    {
        var factory = new ConnectionFactory() { HostName = "localhost", UserName = "guest", Password = "guest" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        // Declare (or check) the queue to consume from
        await channel.QueueDeclareAsync(
            queue: "orders",
            durable: true, // must match the producer's queue settings
            exclusive: false, // can be used by other connections
            autoDelete: false, // don’t delete when the last consumer disconnects
            arguments: null);

        // Define a consumer and start listening
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            byte[] body = eventArgs.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            var orderPlaced = JsonSerializer.Deserialize<OrderPlaced>(message);

            Console.WriteLine($"Received: OrderPlaced - {orderPlaced?.OrderId} - {orderPlaced?.Total} - {orderPlaced?.CreatedAt}");

            // Acknowledge the message
            await ((AsyncEventingBasicConsumer)sender)
                .Channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);

            await Console.Out.WriteLineAsync("Отправлено Ack");
        };
        await channel.BasicConsumeAsync("orders", autoAck: false, consumer);

        Console.WriteLine("Waiting for messages...");
        Console.ReadLine();
    }

    private static async Task JsonExample2()
    {
        var factory = new ConnectionFactory() { HostName = "localhost", UserName = "guest", Password = "guest" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        // Declare (or check) the queue to consume from
        await channel.QueueDeclareAsync(
            queue: "orders",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        Console.WriteLine("Waiting for messages...");

        while (true)
        {
            // Получение одного сообщения
            var result = await channel.BasicGetAsync("orders", autoAck: false);
            if (result != null)
            {
                byte[] body = result.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                var orderPlaced = JsonSerializer.Deserialize<OrderPlaced>(message);

                Console.WriteLine($"Received: OrderPlaced - {orderPlaced?.OrderId} - {orderPlaced?.Total} - {orderPlaced?.CreatedAt}");

                // Acknowledge the message
                await channel.BasicAckAsync(result.DeliveryTag, multiple: false);
                await Console.Out.WriteLineAsync("Отправлено Ack");
            }
            else
            {
                Console.WriteLine("No messages available. Waiting...");
            }

            // Задержка перед следующим запросом
            await Task.Delay(1000); // Задержка в 1 секунду
        }
    }

    public class OrderPlaced
    {
        public Guid OrderId { get; set; } // Свойство для идентификатора заказа
        public double Total { get; set; } // Свойство для общей суммы заказа
        public DateTime CreatedAt { get; set; } // Свойство для даты и времени создания заказа

    }
}