# Demo 3 – Set up Event Hubs namespace and capture

## Scenario

The organization is adding **high-volume telemetry** (e.g. from IoT devices or security cameras) that does not fit the "one event, one subscriber" model of Event Grid. You need a **streaming ingestion** layer that can accept millions of events per second and optionally **archive** them for batch processing. In this demo you **create an Event Hubs namespace and an Event Hub**, then enable **Event Hubs Capture** to persist the stream to Blob Storage (e.g. in Avro format). This sets up the pipeline that downstream analytics or apps will consume in Demo 4—all within the same lesson’s storyline and resource group.

## Objective

- Create an **Event Hubs namespace** and an **Event Hub** (with partition count and retention) in the same lesson resource group.
- Enable **Event Hubs Capture** to archive the stream to **Blob Storage** (e.g. Avro format, time/count window).
- Send sample events with a **C# console app** (Azure.Messaging.EventHubs) using the primary connection string and verify **Capture** writes files to the container.

## Prerequisites

- **Demos 1–2** completed (same resource group can be used). A **Storage account** in that resource group is needed for Capture.
- Access to the **Azure Portal**.

## Steps

### 1. Create the Event Hubs namespace

a. In the Azure Portal, open the **resource group** used in Demos 1–2 (e.g. `RG-EventGrid-Demo`). Click **+ Create**.  
b. Search for **Event Hubs** and select the **Event Hubs** offer (namespace). **Resource group**: same (e.g. `RG-EventGrid-Demo`). **Namespace name**: e.g. `eh-demo-<unique>` (globally unique). **Location**: **East US**. **Pricing tier**: **Standard** (required for Capture). Create the namespace.  
c. Wait for deployment. **Goal**: The namespace will contain one or more Event Hubs (the actual "topics" / streams).

### 2. Create the Event Hub (stream)

a. Open the new **Event Hubs namespace**. Click **+ Event Hub** (or "Event Hub" under Entities).  
b. **Name**: e.g. `telemetry`. **Partition count**: e.g. `2` (sufficient for demo). **Message retention**: e.g. `1` day. Click **Create**.  
c. **Goal**: This is the stream that will receive events; Capture will archive from this hub to Storage.

### 3. Enable Event Hubs Capture to Blob Storage

a. Open the **telemetry** Event Hub. Left menu: **Settings** > **Capture** (or **Capture** in the overview).  
b. Toggle **Capture** to **On**. **Capture Provider**: **Azure Storage**.  
c. **Storage account**: Select the Storage account from Demo 1 (same resource group). **Container**: Create new or use existing, e.g. `eventhubs-capture`.  
d. **Capture format**: **Avro**. **Time window (minutes)** and/or **Size window (MB)**: e.g. 5 minutes and 100 MB (so capture flushes at least every 5 min or 100 MB). **Save**.  
e. **Expected**: Capture is enabled. Events sent to the hub will be batched and written to the container as Avro files. **Goal**: Durable archive of the stream for batch processing.

### 4. Create a Shared access policy and get the connection string (Send)

a. In the **Event Hubs namespace** (not the single hub), go to **Settings** > **Shared access policies**. Click **+ Add**.  
b. **Policy name**: e.g. `SendPolicy`. **Claims**: check **Send** only. **Create**.  
c. Open the policy and copy the **Connection string–primary key**. You will use this to send events (portal **Process data** or SDK).  
d. **Goal**: Producers (devices, apps) use this key to send events; they do not need Listen.

### 5. Send sample events and verify Capture (C# console app)

a. **Create and run the C# console app**. Either use the sample in **Lesson2/EventHubSender/** or create from scratch. In the VS Code terminal:
   - **Using the sample**: `cd Lesson2/EventHubSender` then skip to step 5c.
   - **From scratch**: `dotnet new console -n EventHubSender -o EventHubSender` → `cd EventHubSender` → `dotnet add package Azure.Messaging.EventHubs` → replace `Program.cs` with the sample in **Lesson2/EventHubSender/Program.cs**.  
b. **Connection string**: Use the **Connection string–primary key** from the namespace Shared access policy (step 4). The namespace connection string does not include the Event Hub name—set the **Event Hub name** (e.g. `telemetry`) in code or via environment variable `EVENTHUB_NAME`. Alternatively, use the connection string from the **Event Hub** entity (Settings > Shared access policies), which includes `EntityPath` and is sufficient alone.  
c. **Run the sender**: Set the connection string and (if needed) the Event Hub name, then run the app. **PowerShell** (session actuelle uniquement) :
   - Avec la connection string du **namespace** (sans EntityPath) :  
     `$env:EVENTHUB_NAMESPACE_CONNECTION_STRING = "Endpoint=sb://<namespace>.servicebus.windows.net/;SharedAccessKeyName=SendPolicy;SharedAccessKey=<clé>;"`  
     `$env:EVENTHUB_NAME = "telemetry"`
   - Avec la connection string de l’**Event Hub** (inclut EntityPath) :  
     `$env:EVENTHUB_CONNECTION_STRING = "Endpoint=sb://...;SharedAccessKeyName=...;SharedAccessKey=...;EntityPath=telemetry"`
   - Puis : `dotnet run`. L’app envoie une vingtaine d’événements (ex. `{"deviceId":"demo","temp":22}`) au hub.  
d. Wait for the **Capture window** to elapse (e.g. 5 minutes, or send enough data to hit the size window). Then open **Storage account** > **Containers** > **eventhubs-capture**. Navigate the folder structure: `<namespace>` / `<event-hub>` / `<year>` / `<month>` / `<day>` / `<hour>` (and partition). You should see **.avro** files.  
e. **Expected**: Avro files appear in the container; each file contains a batch of events. **Goal**: Confirm Capture is writing the stream to Storage.

### 6. Consumer groups (brief)

a. In the **telemetry** Event Hub, go to **Consumer groups**. Note the **$Default** consumer group (created automatically).  
b. Explain: each consumer group has its own offset per partition; multiple apps can read the same stream independently. You will use this in Demo 4 to read events with the SDK. **Goal**: Set up for the next demo.

## Notes

- **Capture** is available in **Standard** tier. Use the same Storage account as Demo 1 for simplicity, with a dedicated container (e.g. `eventhubs-capture`). For production, consider Capture to Data Lake and retention policies. See **Exercise 2 – Stream Events with Event Hubs** for ingestion and metrics.
