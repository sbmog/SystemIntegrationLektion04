using System;
using System.Text;
using RabbitMQ.Client;

namespace opg1
{
    internal class EmitLog
    {
        static async Task Main(string[] args)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            // Vi deklarerer en exchange af typen 'fanout'
            await channel.ExchangeDeclareAsync(exchange: "logs", type: ExchangeType.Fanout);

            var message = GetMessage(args);
            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(exchange: "logs", routingKey: string.Empty, body: body);

            Console.WriteLine($" [x] Sent '{message}'");
        }

        static string GetMessage(string[] args)
        {
            return ((args.Length > 0) ? string.Join(" ", args) : "info: Hello World!");
        }
    }
}