// using System;
// using System.Text;
// using RabbitMQ.Client;
// using RabbitMQ.Client.Events;

// class Program
// {
//     static void Main(string[] args)
//     {
//         // Настройки подключения
//         var factory = new ConnectionFactory() { HostName = "localhost" };
//         using var connection = factory.CreateConnection();
//         using var channel = connection.CreateModel();

//         // Объявление очередей
//         string sourceQueue = "source_queue";
//         string targetQueue = "target_queue";

//         channel.QueueDeclare(queue: sourceQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
//         channel.QueueDeclare(queue: targetQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);

//         // Получение сообщения из очереди
//         var consumer = new EventingBasicConsumer(channel);
//         consumer.Received += (model, ea) =>
//         {
//             var body = ea.Body.ToArray();
//             var message = Encoding.UTF8.GetString(body);
//             Console.WriteLine($"Получено сообщение: {message}");

//             // Обработка сообщения
//             var processedMessage = ProcessMessage(message);

//             // Отправка обработанного сообщения в другую очередь
//             var targetBody = Encoding.UTF8.GetBytes(processedMessage);
//             channel.BasicPublish(exchange: "", routingKey: targetQueue, basicProperties: null, body: targetBody);
//             Console.WriteLine($"Отправлено сообщение в {targetQueue}: {processedMessage}");
//         };

//         channel.BasicConsume(queue: sourceQueue, autoAck: true, consumer: consumer);

//         // Бесконечный цикл для поддержания работы программы
//         Console.WriteLine("Программа работает. Нажмите [Ctrl+C] для выхода.");
//         while (true)
//         {
//             // Можно добавить задержку, чтобы избежать излишней загрузки процессора
//             System.Threading.Thread.Sleep(1000);
//         }
//     }

//     static string ProcessMessage(string message)
//     {
//         // Здесь можно добавить логику обработки сообщения
//         return message.ToUpper(); // Пример: преобразование текста в верхний регистр
//     }
// }