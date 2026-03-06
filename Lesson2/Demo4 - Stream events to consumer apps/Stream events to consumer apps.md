# Demo 4 – Stream events to consumer apps

## Scenario

The **Event Hubs** stream is in place and capturing to Storage (Demo 3). The next step is to have **consumer applications** read from the stream in real time—for example a dashboard, an alerting service, or a stream processor. In this demo you **stream events to consumer apps**: you configure **consumer groups**, use the **Event Hubs SDK** (e.g. Python or C#) to read events, and show how multiple consumers can process the same stream independently. By the end of this lesson, you have demonstrated both the "push" model (Event Grid → Function) and the "pull" model (Event Hubs → consumers), with a clear separation between event-driven and streaming use cases.

## Objective

- Use **consumer groups** so that one or more apps can read from the same Event Hub independently.
- **Read events** from the Event Hub using the **Event Hubs SDK** (e.g. Python or C#): create a consumer client, specify **consumer group** and **partition** (or use partition discovery), and process events in a loop.
- Show that **multiple consumers** (e.g. two console apps or two consumer group names) can process the same stream without blocking each other.

## Prerequisites

- **Demo 3** completed (Event Hubs namespace and Event Hub created; optional: Capture enabled). **Connection string** with **Listen** (or namespace connection string with Listen).
- For **checkpoint scenario** (step 5): a **Storage account** and a **Blob container** (e.g. `eventhub-checkpoints`) for persisting consumer group offsets.
- **SDK**: Python (`azure-eventhub`) or .NET (`Azure.Messaging.EventHubs`) installed locally or in a small app.

## Steps

### 1. Get connection details and choose a consumer group

a. Open the **Event Hubs namespace** from Demo 3. **Shared access policies**: use a policy with **Listen** (e.g. create `ListenPolicy` with **Listen** claim, or use the root policy if you have it). Copy the **Connection string–primary key**.  
b. Open the **telemetry** Event Hub. Note the **name** (e.g. `telemetry`). Left menu: **Consumer groups**. Use **$Default** or create a new one (e.g. `analytics`). Each consumer group has its own offset per partition.  
c. **Goal**: You need namespace connection string, event hub name, and consumer group name for the SDK.

### 2. Create a consumer app (C# sample in Lesson2/EventHubConsumerClient)

a. Use the **C# sample** in **Lesson2/EventHubConsumerClient/**: consumer group `$Default` (or pass as first argument: `dotnet run -- analytics`). The app reads from **all partitions**. If you set **EVENTHUB_CHECKPOINT_STORAGE** and **EVENTHUB_CHECKPOINT_CONTAINER**, it uses **EventProcessorClient** with checkpoint (**resume from last position** on restart). Stop with **Ctrl+C**.  
b. **Connection string**: Use a policy with **Listen** (e.g. `ListenPolicy`). Set the same env vars as Demo 3 (namespace connection string + Event Hub name, or connection string with EntityPath). Optionally `EVENTHUB_CONSUMER_GROUP` (default `$Default`).  
   - **PowerShell**: `$env:EVENTHUB_NAMESPACE_CONNECTION_STRING = "Endpoint=...;SharedAccessKeyName=ListenPolicy;SharedAccessKey=...;"` ; `$env:EVENTHUB_NAME = "telemetry"` ; `cd Lesson2/EventHubConsumerClient` ; `dotnet run`.  
c. **Goal**: A minimal consumer that reads events from the stream (all partitions).

### 3. Run the consumer and send events

a. **Run** the consumer (it will wait for events). In another terminal, **send** events using the Demo 3 sender: `cd Lesson2/EventHubSender`, set the same connection string (Send policy), then `dotnet run`.  
b. In the consumer console, **expected**: Events appear with body and timestamp; order is preserved per partition.  
c. **Goal**: Confirm the consumer receives events in real time (pull model); no polling—the SDK manages the connection and receives as events arrive.

### 4. Optional: multiple consumers (same or different consumer group)

a. **Same consumer group**: Start a **second** instance of the consumer (same `EVENTHUB_CONSUMER_GROUP`, default `$Default`, same partition). Only one consumer in the same group reads from a given partition at a time (competing consumers); the second may receive fewer events depending on partition assignment.  
b. **Different consumer group**: In the Event Hub, **Consumer groups** > **+ Consumer group**, create e.g. `analytics`. Run the consumer with **`EVENTHUB_CONSUMER_GROUP=analytics`** (PowerShell: `$env:EVENTHUB_CONSUMER_GROUP = "analytics"`). Send events again. **Expected**: **$Default** and **analytics** each read the same stream independently (each has its own offset).  
c. **Goal**: Show that Event Hubs supports multiple consumers and that Event Hubs is **pull** (consumers read at their pace) vs Event Grid **push** (subscribers are invoked by Event Grid).

### 5. Optional: checkpoint scenario (resume after restart)

To have each consumer group **resume from its last position** when you restart the reader (e.g. **analytics** receives the events it "missed" while stopped), use **checkpoint storage**. Create a **Blob Storage container** (e.g. `eventhub-checkpoints` in the same Storage account as Demo 3) and set **EVENTHUB_CHECKPOINT_STORAGE** (connection string of the Storage account) and **EVENTHUB_CHECKPOINT_CONTAINER** (e.g. `eventhub-checkpoints`). The app then uses **EventProcessorClient** and persists the offset per partition per consumer group in Blob Storage.

**Scenario**: (1) Start reader **$Default** (nothing yet). (2) Start reader **analytics** (nothing yet). (3) Run sender → each receives 20 events. (4) Run sender again → each receives 20. (5) Stop the **analytics** reader (Ctrl+C). (6) Run sender → **$Default** receives 20. (7) Restart **analytics** reader → it receives **immediately the 20 it missed**. (8) Run sender → both receive 20.

**PowerShell** (same env for both readers; change only consumer group per terminal):
- `$env:EVENTHUB_NAMESPACE_CONNECTION_STRING = "..."` ; `$env:EVENTHUB_NAME = "telemetry"` ; `$env:EVENTHUB_CHECKPOINT_STORAGE = "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net"` ; `$env:EVENTHUB_CHECKPOINT_CONTAINER = "eventhub-checkpoints"`
- Terminal 1: `dotnet run -- $Default`
- Terminal 2: `dotnet run -- analytics`
- Terminal 3 (sender): `cd Lesson2/EventHubSender` ; `dotnet run`

## Notes

- Use the **Listen** connection string (or namespace + SAS with Listen) for consumers; never expose in client-side code. For production, use **checkpointing** (e.g. blob storage) so processing can resume. This demo closes the event-driven lesson: Event Grid (push) + Event Hubs (stream + pull). **Exercise 2** focuses on ingestion and metrics; this demo focuses on the consumer side.
