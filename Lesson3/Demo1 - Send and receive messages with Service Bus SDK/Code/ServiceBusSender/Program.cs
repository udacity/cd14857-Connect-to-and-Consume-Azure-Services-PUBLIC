using System.Text.Json;
using Azure.Messaging.ServiceBus;

string? connectionString = Environment.GetEnvironmentVariable("SERVICEBUS_CONNECTION_STRING");
string queueName = Environment.GetEnvironmentVariable("SERVICEBUS_QUEUE_NAME") ?? "order-queue";
int messageCount = int.TryParse(Environment.GetEnvironmentVariable("SERVICEBUS_MESSAGE_COUNT"), out int n) ? n : 1;

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Set SERVICEBUS_CONNECTION_STRING. Optionally SERVICEBUS_QUEUE_NAME (default: order-queue), SERVICEBUS_MESSAGE_COUNT (default: 1).");
    return 1;
}

await using var client = new ServiceBusClient(connectionString);
ServiceBusSender sender = client.CreateSender(queueName);

for (int i = 0; i < messageCount; i++)
{
    var order = new { orderId = i + 1, item = "Widget" };
    var body = JsonSerializer.Serialize(order);
    await sender.SendMessageAsync(new ServiceBusMessage(body));
    Console.WriteLine($"Sent: {body}");
}

Console.WriteLine($"Sent {messageCount} message(s) to queue '{queueName}'.");
return 0;
