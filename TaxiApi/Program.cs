using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/api/taxi/order", async (TaxiOrder order) =>
{
    var factory = new ConnectionFactory { HostName = "localhost" };
    using var connection = await factory.CreateConnectionAsync();
    using var channel = await connection.CreateChannelAsync();

    await channel.ExchangeDeclareAsync(exchange: "taxi-orders", type: ExchangeType.Fanout);

    var message = JsonSerializer.Serialize(order);
    var body = Encoding.UTF8.GetBytes(message);

    await channel.BasicPublishAsync(exchange: "taxi-orders", routingKey: string.Empty, body: body);

    return Results.Ok(new { statud = "Bestilling modtaget", Kunde = order.CustomerName });
});



app.Run();

public record TaxiOrder(string CustomerName, string PickupLocation, string Destination);