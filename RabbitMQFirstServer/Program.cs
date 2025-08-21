using System;
using RabbitMQ.Client;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
internal class Program
{
    private static async Task Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost", UserName = "myuser", Password = "mypassword" };
        using (var connection = await factory.CreateConnectionAsync())
        using (var channel = await connection.CreateChannelAsync())
        {
            await channel.QueueDeclareAsync(queue: "hello",
                                            durable: false,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);

            string message = "Привет - Hello World!";
            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(exchange: "",
                                            routingKey: "hello",
                                            mandatory: true, // fail if the message can’t be routed
                                            basicProperties: new BasicProperties { Persistent = true }, // message will be saved to disk,
                                            body: body);
            Console.WriteLine($" [x] Sent {message}");

        }
        // Example two
        await JsonExample();
    }

    private static async Task JsonExample()
    {
        var factory = new ConnectionFactory() { HostName = "localhost", UserName = "myuser", Password = "mypassword" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        // Ensure the queue exists (create it if not already there)
        await channel.QueueDeclareAsync(
            queue: "orders",
            durable: true, // save to disk so the queue isn’t lost on broker restart
            exclusive: false, // can be used by other connections
            autoDelete: false, // don’t delete when the last consumer disconnects
            arguments: null);

        // Create a message
        var orderPlaced = new
        {
            OrderId = Guid.NewGuid(),
            Total = 99.99,
            CreatedAt = DateTime.UtcNow
        };
        var message = JsonSerializer.Serialize(orderPlaced);
        var body = Encoding.UTF8.GetBytes(message);

        // Publish the message
        await channel.BasicPublishAsync(
            exchange: string.Empty, // default exchange
            routingKey: "orders",
            mandatory: true, // fail if the message can’t be routed
            basicProperties: new BasicProperties { Persistent = true }, // message will be saved to disk
            body: body);

        Console.WriteLine($"Sent: {message}");
    }
}