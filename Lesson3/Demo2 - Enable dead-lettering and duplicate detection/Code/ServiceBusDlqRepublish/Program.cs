using Azure.Messaging.ServiceBus;

// Demo 2 step 6: receive one message from DLQ, republish to main queue, complete DLQ message.
string? connectionString = Environment.GetEnvironmentVariable("SERVICEBUS_CONNECTION_STRING");
string mainQueueName = Environment.GetEnvironmentVariable("SERVICEBUS_QUEUE_NAME") ?? "order-queue";
string dlqName = $"{mainQueueName}/$DeadLetterQueue";

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Set SERVICEBUS_CONNECTION_STRING. Optionally SERVICEBUS_QUEUE_NAME (default: order-queue).");
    return 1;
}

await using var client = new ServiceBusClient(connectionString);
await using var dlqReceiver = client.CreateReceiver(dlqName, new ServiceBusReceiverOptions { ReceiveMode = ServiceBusReceiveMode.PeekLock });
ServiceBusSender mainSender = client.CreateSender(mainQueueName);

Console.WriteLine($"Reading from DLQ '{dlqName}', republishing to '{mainQueueName}'.\n");

ServiceBusReceivedMessage? message = await dlqReceiver.ReceiveMessageAsync(TimeSpan.FromSeconds(10));
if (message == null)
{
    Console.WriteLine("No message in DLQ (timeout).");
    return 0;
}

string body = message.Body.ToString();
Console.WriteLine($"DLQ message: {body}");
Console.WriteLine($"  DeadLetterReason: {message.DeadLetterReason}");
Console.WriteLine($"  DeadLetterErrorDescription: {message.DeadLetterErrorDescription}");

await mainSender.SendMessageAsync(new ServiceBusMessage(body));
Console.WriteLine($"Republished to '{mainQueueName}'.");

await dlqReceiver.CompleteMessageAsync(message);
Console.WriteLine("Completed DLQ message (removed from DLQ).");
return 0;
