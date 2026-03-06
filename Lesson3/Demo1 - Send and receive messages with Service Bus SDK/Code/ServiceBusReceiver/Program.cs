using Azure.Messaging.ServiceBus;

string? connectionString = Environment.GetEnvironmentVariable("SERVICEBUS_CONNECTION_STRING");
string queueName = Environment.GetEnvironmentVariable("SERVICEBUS_QUEUE_NAME") ?? "order-queue";

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Set SERVICEBUS_CONNECTION_STRING. Optionally SERVICEBUS_QUEUE_NAME (default: order-queue).");
    return 1;
}

await using var client = new ServiceBusClient(connectionString);
await using var receiver = client.CreateReceiver(queueName, new ServiceBusReceiverOptions { ReceiveMode = ServiceBusReceiveMode.PeekLock });

Console.WriteLine($"Receiving from queue '{queueName}' (PeekLock).\n");

int received = 0;
while (true)
{
    ServiceBusReceivedMessage? message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5));
    if (message == null)
    {
        if (received == 0)
            Console.WriteLine("No message received (timeout). Run the sender first.");
        break;
    }

    string body = message.Body.ToString();
    Console.WriteLine($"[{message.SequenceNumber}] {body}");

    await receiver.CompleteMessageAsync(message);
    received++;
}

Console.WriteLine($"Received and completed {received} message(s).");
return 0;
