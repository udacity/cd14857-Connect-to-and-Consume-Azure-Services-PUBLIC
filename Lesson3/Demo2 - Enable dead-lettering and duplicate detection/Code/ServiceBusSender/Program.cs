using System.Text.Json;
using Azure.Messaging.ServiceBus;

// Demo 2: sender with optional MessageId (duplicate detection) and optional body (e.g. dlq-test).
string? connectionString = Environment.GetEnvironmentVariable("SERVICEBUS_CONNECTION_STRING");
string queueName = Environment.GetEnvironmentVariable("SERVICEBUS_QUEUE_NAME") ?? "order-queue";
int messageCount = int.TryParse(Environment.GetEnvironmentVariable("SERVICEBUS_MESSAGE_COUNT"), out int n) ? n : 1;
string? messageId = Environment.GetEnvironmentVariable("SERVICEBUS_MESSAGE_ID");
string? bodyOverride = Environment.GetEnvironmentVariable("SERVICEBUS_BODY");

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Demo 2 Sender. Set SERVICEBUS_CONNECTION_STRING. Optionally: SERVICEBUS_QUEUE_NAME (default: order-queue), SERVICEBUS_MESSAGE_COUNT (default: 1), SERVICEBUS_MESSAGE_ID (duplicate detection), SERVICEBUS_BODY (e.g. {\"orderId\":\"dlq-test\",\"item\":\"Test\"}).");
    return 1;
}

await using var client = new ServiceBusClient(connectionString);
ServiceBusSender sender = client.CreateSender(queueName);

for (int i = 0; i < messageCount; i++)
{
    string body = !string.IsNullOrEmpty(bodyOverride)
        ? bodyOverride
        : JsonSerializer.Serialize(new { orderId = i + 1, item = "Widget" });
    var msg = new ServiceBusMessage(body);
    if (!string.IsNullOrEmpty(messageId))
        msg.MessageId = messageId;
    await sender.SendMessageAsync(msg);
    Console.WriteLine($"Sent: {body}" + (msg.MessageId != null ? $" (MessageId: {msg.MessageId})" : ""));
}

Console.WriteLine($"Sent {messageCount} message(s) to queue '{queueName}'.");
return 0;
