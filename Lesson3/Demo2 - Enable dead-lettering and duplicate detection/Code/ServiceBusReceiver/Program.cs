using Azure.Messaging.ServiceBus;

// Demo 2: receiver with optional Abandon mode (DLQ demo) and DLQ inspection (DeadLetterReason / DeadLetterErrorDescription).
// Use SERVICEBUS_QUEUE_NAME=order-queue/$DeadLetterQueue to read from the DLQ.
string? connectionString = Environment.GetEnvironmentVariable("SERVICEBUS_CONNECTION_STRING");
string queueName = Environment.GetEnvironmentVariable("SERVICEBUS_QUEUE_NAME") ?? "order-queue";
bool abandon = string.Equals(Environment.GetEnvironmentVariable("SERVICEBUS_ABANDON"), "true", StringComparison.OrdinalIgnoreCase);

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Demo 2 Receiver. Set SERVICEBUS_CONNECTION_STRING. Optionally: SERVICEBUS_QUEUE_NAME (default: order-queue; use order-queue/$DeadLetterQueue for DLQ), SERVICEBUS_ABANDON=true (abandon for DLQ demo).");
    return 1;
}

await using var client = new ServiceBusClient(connectionString);
await using var receiver = client.CreateReceiver(queueName, new ServiceBusReceiverOptions { ReceiveMode = ServiceBusReceiveMode.PeekLock });

Console.WriteLine($"Receiving from '{queueName}' (PeekLock). " + (abandon ? "Mode: ABANDON (message will be redelivered)." : "Mode: COMPLETE.") + "\n");

int received = 0;
while (true)
{
    ServiceBusReceivedMessage? message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5));
    if (message == null)
    {
        if (received == 0)
            Console.WriteLine("No message received (timeout).");
        break;
    }

    string body = message.Body.ToString();
    Console.WriteLine($"[{message.SequenceNumber}] {body}");
    if (!string.IsNullOrEmpty(message.DeadLetterReason))
        Console.WriteLine($"  DeadLetterReason: {message.DeadLetterReason}");
    if (!string.IsNullOrEmpty(message.DeadLetterErrorDescription))
        Console.WriteLine($"  DeadLetterErrorDescription: {message.DeadLetterErrorDescription}");

    if (abandon)
        await receiver.AbandonMessageAsync(message);
    else
        await receiver.CompleteMessageAsync(message);
    received++;
}

Console.WriteLine($"Received and {(abandon ? "abandoned" : "completed")} {received} message(s).");
return 0;
