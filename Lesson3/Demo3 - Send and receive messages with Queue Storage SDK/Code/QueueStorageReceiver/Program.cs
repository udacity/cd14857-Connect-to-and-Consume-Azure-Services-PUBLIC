using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

string? connectionString = Environment.GetEnvironmentVariable("QUEUESTORAGE_CONNECTION_STRING");
string queueName = Environment.GetEnvironmentVariable("QUEUESTORAGE_QUEUE_NAME") ?? "simple-task-queue";

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Set QUEUESTORAGE_CONNECTION_STRING. Optionally QUEUESTORAGE_QUEUE_NAME (default: simple-task-queue).");
    return 1;
}

var client = new QueueClient(connectionString, queueName);

Console.WriteLine($"Receiving from queue '{queueName}'. Messages are deleted after processing.\n");

int received = 0;
while (true)
{
    Response<QueueMessage[]?> response = await client.ReceiveMessagesAsync(maxMessages: 1, visibilityTimeout: TimeSpan.FromSeconds(30));
    QueueMessage[]? messages = response.Value;
    if (messages == null || messages.Length == 0)
    {
        if (received == 0)
            Console.WriteLine("No message in queue (or visibility timeout). Run the sender first.");
        break;
    }

    foreach (QueueMessage message in messages)
    {
        string body = message.Body?.ToString() ?? string.Empty;
        Console.WriteLine($"[{message.MessageId}] {body}");
        await client.DeleteMessageAsync(message.MessageId, message.PopReceipt);
        received++;
    }
}

Console.WriteLine($"Received and deleted {received} message(s).");
return 0;
