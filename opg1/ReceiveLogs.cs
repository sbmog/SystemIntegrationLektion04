using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;

namespace opg1
{
	internal class ReceiveLogs
	{
		static async Task Main(string[] args)
		{
			var factory = new ConnectionFactory { HostName = "localhost" };

			using var connection = await factory.CreateConnectionAsync();
			using var channel = await connection.CreateChannelAsync();

			// Vi erklærer den samme exchange som afsenderen
			await channel.ExchangeDeclareAsync(exchange: "logs", type: ExchangeType.Fanout);

			// Vi opretter en midlertidig, unik kø der sletter sig selv når vi lukker
			var queueDeclareResult = await channel.QueueDeclareAsync(
				queue: string.Empty,
				durable: false,
				exclusive: true,
				autoDelete: true);

			var queueName = queueDeclareResult.QueueName;

			// Vi "binder" vores midlertidige kø til vores exchange
			await channel.QueueBindAsync(queue: queueName, exchange: "logs", routingKey: string.Empty);

			Console.WriteLine(" [*] Waiting for logs. To exit press CTRL+C");

			var consumer = new AsyncEventingBasicConsumer(channel);

			consumer.ReceivedAsync += (model, ea) =>
			{
				byte[] body = ea.Body.ToArray();
				var message = Encoding.UTF8.GetString(body);
				Console.WriteLine($" [x] {message}");
				return Task.CompletedTask;
			};

			await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);

			Console.WriteLine(" Press [enter] to exit.");
			Console.ReadLine();
		}
	}
}