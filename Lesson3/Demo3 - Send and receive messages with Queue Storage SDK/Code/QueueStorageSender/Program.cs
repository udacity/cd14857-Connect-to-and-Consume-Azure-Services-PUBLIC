using Azure.Storage.Queues;

string? connectionString = Environment.GetEnvironmentVariable("QUEUESTORAGE_CONNECTION_STRING");
string queueName = Environment.GetEnvironmentVariable("QUEUESTORAGE_QUEUE_NAME") ?? "simple-task-queue";
int messageCount = int.TryParse(Environment.GetEnvironmentVariable("QUEUESTORAGE_MESSAGE_COUNT"), out int n) ? n : 1;
string messageContent = Environment.GetEnvironmentVariable("QUEUESTORAGE_MESSAGE_BODY") ?? "Hello Queue Storage";

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Set QUEUESTORAGE_CONNECTION_STRING (Storage account connection string). Optionally: QUEUESTORAGE_QUEUE_NAME (default: simple-task-queue), QUEUESTORAGE_MESSAGE_COUNT (default: 1), QUEUESTORAGE_MESSAGE_BODY.");
    return 1;
}

var client = new QueueClient(connectionString, queueName);
await client.CreateIfNotExistsAsync();

for (int i = 0; i < messageCount; i++)
{
    string body = messageCount > 1 ? $"{messageContent} #{i + 1}" : messageContent;
    await client.SendMessageAsync(body);
    Console.WriteLine($"Sent: {body}");
}

Console.WriteLine($"Sent {messageCount} message(s) to queue '{queueName}'.");
return 0;
