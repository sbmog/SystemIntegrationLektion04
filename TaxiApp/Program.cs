using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

// Vi deklarerer den samme exchange som i API'et
await channel.ExchangeDeclareAsync(exchange: "taxi-orders", type: ExchangeType.Fanout);

// Hver taxa-instans får sin egen midlertidige kø
var queueDeclareResult = await channel.QueueDeclareAsync(
    queue: string.Empty,
    durable: false,
    exclusive: true,
    autoDelete: true);

var queueName = queueDeclareResult.QueueName;

// Vi binder køen til exchangen
await channel.QueueBindAsync(queue: queueName, exchange: "taxi-orders", routingKey: string.Empty);

Console.WriteLine(" --- SØNDERHØJ TAXA MODTAGER ER ONLINE ---");

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    Console.WriteLine($"\n[NY BESTILLING]:\n{message}");

    return Task.CompletedTask;
};

await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);

Console.WriteLine(" Tryk [Enter] for at lukke.");
Console.ReadLine();