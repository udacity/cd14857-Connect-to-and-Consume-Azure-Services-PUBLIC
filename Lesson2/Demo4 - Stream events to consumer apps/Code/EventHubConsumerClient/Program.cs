using System.Text;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;

// Connection string must have Listen claim (namespace or Event Hub).
// Consumer group: first command-line argument, or EVENTHUB_CONSUMER_GROUP, or $Default.
// With checkpoint: set EVENTHUB_CHECKPOINT_STORAGE (Storage connection string) and EVENTHUB_CHECKPOINT_CONTAINER
//   so that each consumer group resumes from last position when restarted (e.g. "analytics" receives missed events).
string? connectionString = Environment.GetEnvironmentVariable("EVENTHUB_NAMESPACE_CONNECTION_STRING")
    ?? Environment.GetEnvironmentVariable("EVENTHUB_CONNECTION_STRING");
string eventHubName = Environment.GetEnvironmentVariable("EVENTHUB_NAME") ?? "telemetry";
string consumerGroup = (args.Length >= 1 && !string.IsNullOrWhiteSpace(args[0]))
    ? args[0].Trim()
    : (Environment.GetEnvironmentVariable("EVENTHUB_CONSUMER_GROUP") ?? "$Default");
string? checkpointStorage = Environment.GetEnvironmentVariable("EVENTHUB_CHECKPOINT_STORAGE")
    ?? Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
string checkpointContainer = Environment.GetEnvironmentVariable("EVENTHUB_CHECKPOINT_CONTAINER") ?? "eventhub-checkpoints";

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Usage: dotnet run [consumer-group]");
    Console.WriteLine("  consumer-group  Optional. Overrides EVENTHUB_CONSUMER_GROUP (default: $Default).");
    Console.WriteLine("Env: EVENTHUB_NAMESPACE_CONNECTION_STRING (or EVENTHUB_CONNECTION_STRING), optionally EVENTHUB_NAME.");
    Console.WriteLine("Checkpoint (resume after restart): EVENTHUB_CHECKPOINT_STORAGE + EVENTHUB_CHECKPOINT_CONTAINER (default: eventhub-checkpoints).");
    return 1;
}

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

// Mode processor with checkpoint: each consumer group resumes from last position when restarted.
if (!string.IsNullOrEmpty(checkpointStorage))
{
    var blobClient = new BlobContainerClient(checkpointStorage, checkpointContainer);
    await blobClient.CreateIfNotExistsAsync(cancellationToken: cts.Token);

    EventProcessorClient processor = connectionString.Contains("EntityPath=", StringComparison.OrdinalIgnoreCase)
        ? new EventProcessorClient(blobClient, consumerGroup, connectionString)
        : new EventProcessorClient(blobClient, consumerGroup, connectionString, eventHubName);

    processor.ProcessEventAsync += async args =>
    {
        string body = Encoding.UTF8.GetString(args.Data.Body.ToArray());
        Console.WriteLine($"[p{args.Partition.PartitionId} {args.Data.EnqueuedTime:HH:mm:ss.fff}] {body}");
        // Checkpoints are not automatic: call UpdateCheckpointAsync so restart resumes from here.
        await args.UpdateCheckpointAsync(args.CancellationToken);
    };
    processor.ProcessErrorAsync += args =>
    {
        Console.WriteLine($"Error: {args.Exception.Message}");
        return Task.CompletedTask;
    };

    Console.WriteLine($"Event Hub '{eventHubName}', consumer group '{consumerGroup}', checkpoint: {checkpointContainer}");
    Console.WriteLine("Reading from all partitions (processor with checkpoint). Restart = resume from last position. Press Ctrl+C to stop.\n");

    await processor.StartProcessingAsync(cts.Token);
    try
    {
        await Task.Delay(Timeout.Infinite, cts.Token);
    }
    catch (OperationCanceledException) { }

    // Use CancellationToken.None so the processor can flush the last checkpoint before exiting.
    // If we pass the cancelled token, the stop may abort before the checkpoint is written.
    Console.WriteLine("Stopping and saving checkpoint...");
    await processor.StopProcessingAsync(CancellationToken.None);
    Console.WriteLine("Consumer stopped.");
    return 0;
}

// Simple mode (no checkpoint): EventHubConsumerClient.
await using var consumer = connectionString.Contains("EntityPath=", StringComparison.OrdinalIgnoreCase)
    ? new EventHubConsumerClient(consumerGroup, connectionString)
    : new EventHubConsumerClient(consumerGroup, connectionString, eventHubName);

string[] partitionIds = await consumer.GetPartitionIdsAsync();
Console.WriteLine($"Event Hub '{eventHubName}', consumer group '{consumerGroup}', partitions: [{string.Join(", ", partitionIds)}]");
Console.WriteLine("Reading from all partitions. Press Ctrl+C to stop.\n");

IAsyncEnumerable<PartitionEvent> eventEnumerable = consumer.ReadEventsAsync(cts.Token);

try
{
    await foreach (PartitionEvent partitionEvent in eventEnumerable)
    {
        string body = Encoding.UTF8.GetString(partitionEvent.Data.Body.ToArray());
        string partitionId = partitionEvent.Partition.PartitionId;
        Console.WriteLine($"[p{partitionId} {partitionEvent.Data.EnqueuedTime:HH:mm:ss.fff}] {body}");
    }
}
catch (OperationCanceledException)
{
    // Ctrl+C or cancellation token: exit cleanly.
}

Console.WriteLine("Consumer stopped.");
return 0;
