# Demo 3 – Send and receive messages with Queue Storage SDK

## Scenario

The team now needs to choose the **right queue** for different workloads: **Service Bus** (Demos 1–2) is ideal for ordered, reliable, transactional messages (e.g. orders), but some scenarios—such as **high-volume, simple tasks** (e.g. image resizing, batch notifications)—do not need FIFO or dead-lettering and can use a simpler, cheaper option. In this demo you **send and receive messages with the Queue Storage SDK** (Azure Storage Queues): you create a storage account and a queue, then use the SDK to enqueue and dequeue messages. By comparing this with Service Bus, learners see when to use **Queue Storage** (simple, massive scale) versus **Service Bus** (orchestration, reliability, duplicate detection)—closing the message-based lesson with a clear decision framework.

## Objective

- Create a **Storage account** and a **queue** (Azure Queue Storage) in the same lesson resource group (or a new one).
- **Send** messages using the **Queue Storage SDK** (e.g. `azure-storage-queue` for Python or `Azure.Storage.Queues` for .NET): create queue client, encode message (e.g. base64 or plain text), and **send**.
- **Receive** messages: **get** (peek or dequeue), process, and **delete** to remove from the queue. Compare with Service Bus: no PeekLock, no built-in dead-letter or duplicate detection; simpler and suited for high-volume, simple tasks.

## Prerequisites

- **Demos 1–2** completed (Service Bus queue and reliability features). Same or separate resource group for Storage.
- **C# samples**: **Lesson3/QueueStorageSender**, **Lesson3/QueueStorageReceiver** (Azure.Storage.Queues). **Connection string** (Storage account, Access keys) for **QUEUESTORAGE_CONNECTION_STRING**.

## Steps

### 1. Create Storage account and queue (or use existing)

a. In the Azure Portal, open the **resource group** from Demo 1 (e.g. `RG-ServiceBus-Demo`) or create a new one. **+ Create** > search **Storage account** > **Create**.  
b. **Resource group**: same or new. **Storage account name**: e.g. `stqueuedemo<unique>`. **Region**: **East US**. **Performance**: Standard. **Create**.  
c. When deployed, open the Storage account. Left menu: **Data storage** > **Queues** > **+ Queue**. **Name**: e.g. `simple-task-queue`. **Create**.  
d. **Access keys**: In the Storage account, go to **Security + networking** > **Access keys**. Copy **Connection string** (key1) or note **Account name** and **Key** for the SDK. **Goal**: Queue and credentials for Queue Storage SDK.

### 2. Send messages with the Queue Storage SDK (sender – Lesson3/QueueStorageSender)

a. Use the **C# sample** in **Lesson3/QueueStorageSender/**: it creates a `QueueClient` and sends one or more text messages (e.g. `Hello Queue Storage`) to `simple-task-queue`. The SDK gère l’encodage.  
b. **Environment variables (PowerShell)**:  
   - `QUEUESTORAGE_CONNECTION_STRING` : connection string du compte de stockage (Access keys).  
   - `QUEUESTORAGE_QUEUE_NAME` *(optionnel)* : nom de la file (défaut `simple-task-queue`).  
   - `QUEUESTORAGE_MESSAGE_COUNT` *(optionnel)* : nombre de messages à envoyer (défaut `1`).  
   - `QUEUESTORAGE_MESSAGE_BODY` *(optionnel)* : contenu du message (défaut `Hello Queue Storage`).  
   Exemple :  
   ```powershell
   $env:QUEUESTORAGE_CONNECTION_STRING = "DefaultEndpointsProtocol=https;AccountName=<name>;AccountKey=<key>;EndpointSuffix=core.windows.net"
   $env:QUEUESTORAGE_QUEUE_NAME = "simple-task-queue"
   cd Lesson3\QueueStorageSender
   dotnet run
   ```  
c. Dans le portail : **Storage account** > **Queues** > **simple-task-queue**. Clique sur la queue puis sur **Peek messages** ou rafraîchis ; le message (ou le compteur) doit apparaître.  
d. **Expected**: Message is in the queue. **Goal**: Producer can enqueue via SDK; message size is small (Queue Storage max 64 KB per message).

### 3. Receive and delete messages with the SDK (receiver – Lesson3/QueueStorageReceiver)

a. Use the **C# sample** in **Lesson3/QueueStorageReceiver/**: il crée un `QueueClient`, appelle `ReceiveMessagesAsync(maxMessages: 1, visibilityTimeout: 30 s)` en boucle, affiche le corps, puis appelle `DeleteMessageAsync(MessageId, PopReceipt)` pour supprimer le message.  
b. **Environment variables (PowerShell)**:  
   - `QUEUESTORAGE_CONNECTION_STRING` : même valeur que pour le sender.  
   - `QUEUESTORAGE_QUEUE_NAME` *(optionnel)* : même queue (défaut `simple-task-queue`).  
   Exemple :  
   ```powershell
   $env:QUEUESTORAGE_CONNECTION_STRING = "DefaultEndpointsProtocol=https;AccountName=<name>;AccountKey=<key>;EndpointSuffix=core.windows.net"
   $env:QUEUESTORAGE_QUEUE_NAME = "simple-task-queue"
   cd Lesson3\QueueStorageReceiver
   dotnet run
   ```  
c. **Run** the receiver. **Expected**: Message(s) are received, printed, then deleted. In the portal, the message count decreases. If you do **not** delete, the message becomes visible again after the **visibility timeout** (30 s).  
d. **Goal**: Consumer dequeues, processes, and deletes; at-least-once delivery (if delete fails, message reappears).

### 4. Compare Queue Storage vs Service Bus (recap)

a. **Queue Storage**: Simple; 64 KB max message; no FIFO guarantee; no built-in dead-letter or duplicate detection; visibility timeout then re-queue; cheap, scales with storage. **Use for**: high-volume, simple tasks (e.g. image resizing, batch notifications).  
b. **Service Bus**: PeekLock, Complete/Abandon; dead-letter queue; duplicate detection; sessions for FIFO; at-least-once with retry. **Use for**: ordered, critical workflows (e.g. orders, transactions).  
c. **Goal**: Clear decision framework for the message-based lesson; refer to **Exercise 2** for hands-on send/receive and visibility timeout vs lock behavior.

## Notes

- Queue Storage has a **visibility timeout** after receive; if the message is not deleted in time, it becomes visible again (at-least-once delivery). No built-in dead-letter; implement your own retry or move to a “poison” queue. See **Exercise 2 – Send and receive with Queue Storage and Service Bus** for hands-on observation of visibility timeout and Service Bus abandon/redelivery.
