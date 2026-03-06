# Demo 1 – Send and receive messages with Service Bus SDK

## Scenario

You are the **Lead Developer** for an e-commerce platform (**Global-Retail Connect**) that must process orders reliably. During peaks, the backend cannot handle every request synchronously, so you introduce a **message queue** as a buffer: the front end sends orders to the queue, and workers process them at their own pace. In this lesson you start from a **clean Azure environment** (no pre-existing Service Bus or queue resources). This demo shows how to **create a Service Bus namespace and a queue**, then **send and receive messages** using the **Service Bus SDK** (e.g. .NET or Python)—establishing the core producer-consumer pattern that the next demos will extend with reliability features and comparisons to Queue Storage.

## Objective

- Create a **Service Bus namespace** and a **queue** in a new resource group (clean environment for this lesson).
- **Send** messages to the queue using the **Service Bus SDK** (e.g. .NET or Python): create a sender client, serialize a payload (e.g. JSON order), and send.
- **Receive** messages (e.g. **PeekLock** mode): create a receiver client, receive one or more messages, process, and **complete** (or abandon) to remove from the queue.

## Prerequisites

- An **Azure subscription** and access to the **Azure Portal**. This lesson starts from a **clean environment** (no pre-existing Service Bus for this lesson).
- **SDK**: e.g. `azure-servicebus` (Python) or `Azure.Messaging.ServiceBus` (.NET) installed locally or in a small app.

## Steps

### 1. Create resource group and Service Bus namespace

a. In the Azure Portal **search bar**, type **Resource groups** > **+ Create**. **Resource group**: e.g. `RG-ServiceBus-Demo`. **Region**: **East US**. **Create**.  
b. Search for **Service Bus** > **+ Create**. **Resource group**: `RG-ServiceBus-Demo`. **Namespace name**: e.g. `sb-demo-<unique>` (globally unique). **Location**: **East US**. **Pricing tier**: **Standard**. **Create**.  
c. Wait for deployment. **Goal**: Clean messaging environment for this lesson.

### 2. Create the queue and get the connection string

a. Open the **Service Bus** namespace. Left menu: **Entities** > **Queues** > **+ Queue**.  
b. **Name**: e.g. `order-queue`. Leave defaults (or set **Max delivery count** to 10 for later dead-letter demo). **Create**.  
c. In the namespace (not the queue), go to **Settings** > **Shared access policies**. Click **+ Add**. **Policy name**: e.g. `SendListenPolicy`. **Claims**: **Send** and **Listen**. **Create**.  
d. Open the policy and copy the **Connection string–primary key**. You will use it in your sender and receiver code. **Goal**: Queue exists and you have a connection string for the SDK.

### 3. Send messages with the SDK (sender – Lesson3/ServiceBusSender)

a. Use the **C# sample** in **Lesson3/ServiceBusSender/**: `ServiceBusClient`, `CreateSender("order-queue")`, sends a JSON message `{"orderId": 1, "item": "Widget"}` (or several if `SERVICEBUS_MESSAGE_COUNT` is set).  
b. **Connection string**: Set **SERVICEBUS_CONNECTION_STRING** (connection string–primary key from Shared access policies). Optionally **SERVICEBUS_QUEUE_NAME** (default `order-queue`), **SERVICEBUS_MESSAGE_COUNT** (default 1).  
   - **PowerShell**: `$env:SERVICEBUS_CONNECTION_STRING = "Endpoint=sb://...;SharedAccessKeyName=SendListenPolicy;SharedAccessKey=...;"` ; `cd Lesson3/ServiceBusSender` ; `dotnet run`.  
c. In the portal: **Service Bus** namespace > **Queues** > **order-queue**. Check **Active message count** (e.g. 1) or **Service Bus Explorer** > **Peek from start** to see the message.  
d. **Expected**: Message appears in the queue. **Goal**: Producer can send to the queue via SDK.

### 4. Receive and complete messages with the SDK (receiver – Lesson3/ServiceBusReceiver)

a. Use the **C# sample** in **Lesson3/ServiceBusReceiver/**: `ServiceBusClient`, `CreateReceiver` with **PeekLock**, `ReceiveMessageAsync` in a loop, print body, `CompleteMessageAsync`; exits when no message is received within the timeout.  
b. **Connection string**: Same **SERVICEBUS_CONNECTION_STRING**. Optionally **SERVICEBUS_QUEUE_NAME** (default `order-queue`).  
   - **PowerShell**: `$env:SERVICEBUS_CONNECTION_STRING = "..."` ; `cd Lesson3/ServiceBusReceiver` ; `dotnet run`.  
c. **Run** the receiver. **Expected**: The message(s) are received, body printed, then completed. In the portal, **Active message count** returns to **0**.  
d. **Goal**: Consumer receives with PeekLock and completes so the message is removed; no redelivery.

### 5. Optional: send multiple messages and receive in a loop

a. **Sender**: Set **SERVICEBUS_MESSAGE_COUNT** (e.g. 5): `$env:SERVICEBUS_MESSAGE_COUNT = "5"` ; run **Lesson3/ServiceBusSender**. The app sends 5 messages with `orderId` 1–5.  
b. **Receiver**: Run **Lesson3/ServiceBusReceiver**. It receives in a loop until no message is available (5 s timeout); each message is printed and completed.  
c. **Expected**: All messages are received and completed; queue is empty. **Goal**: Show batch processing pattern.

## Notes

- **PeekLock** ensures the message is hidden while processing; if the app crashes without completing, the message reappears after the lock expires. Use **Abandon** to retry or let it go to dead-letter after max deliveries. See **Exercise 1 – Send and Receive with Service Bus Queues** for a portal-only version; this demo emphasizes the SDK.
