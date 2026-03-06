using System.Text;
using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;

// Use the primary connection string from the Event Hubs namespace (Shared access policies).
// If the connection string includes EntityPath, you can leave EVENTHUB_NAME unset.
string? connectionString = Environment.GetEnvironmentVariable("EVENTHUB_NAMESPACE_CONNECTION_STRING")
    ?? Environment.GetEnvironmentVariable("EVENTHUB_CONNECTION_STRING");
string eventHubName = Environment.GetEnvironmentVariable("EVENTHUB_NAME") ?? "telemetry";

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Set EVENTHUB_NAMESPACE_CONNECTION_STRING (or EVENTHUB_CONNECTION_STRING) and optionally EVENTHUB_NAME (default: telemetry).");
    return 1;
}

// If connection string contains EntityPath, use it alone; otherwise pass eventHubName.
await using var producer = connectionString.Contains("EntityPath=", StringComparison.OrdinalIgnoreCase)
    ? new EventHubProducerClient(connectionString)
    : new EventHubProducerClient(connectionString, eventHubName);

const int eventCount = 20;
var random = new Random();

using var batch = await producer.CreateBatchAsync();

for (int i = 0; i < eventCount; i++)
{
    var payload = new { deviceId = "demo", temp = 18 + random.Next(0, 15), sequence = i + 1 };
    var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
    batch.TryAdd(new EventData(bytes));
}

await producer.SendAsync(batch);
Console.WriteLine($"Sent {eventCount} events to Event Hub '{eventHubName}'.");
await producer.CloseAsync();
return 0;
