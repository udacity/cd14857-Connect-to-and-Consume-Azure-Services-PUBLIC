# Demo 2 – Enable dead-lettering and duplicate detection

## Scenario

The **order-processing queue** from Demo 1 is in place, but in production some messages **fail** after several retries (e.g. invalid payload, downstream timeout) and others may be **sent twice** by a buggy client. The team needs **dead-lettering** (failed messages moved to a side-queue for inspection and reprocessing) and **duplicate detection** (optional, to avoid processing the same order twice). In this demo you **enable dead-lettering** (e.g. on message expiration or max delivery count) and **duplicate detection** on the Service Bus queue, then show how to send, fail, and inspect messages in the dead-letter queue, and how duplicates are discarded when the feature is enabled. This makes the messaging layer suitable for critical, once-only workflows.

## Objective

- **Enable dead-lettering** on the queue (e.g. **Dead-letter on message expiration** and/or **Max delivery count**). Send a message that will never be completed (or let it expire) and show it moving to the **dead-letter queue (DLQ)**.
- **Enable duplicate detection** (e.g. **Duplicate detection history** window). Send two messages with the same **MessageId** within the window and show that the duplicate is discarded.
- **Inspect and republish** (optional): read a message from the DLQ in the portal or via SDK and explain reprocessing strategies.

## Prerequisites

- **Demo 1** completed (Service Bus namespace and queue; **Lesson3/ServiceBusSender** and **Lesson3/ServiceBusReceiver**).
- **Demo 2 apps**: **Lesson3/ServiceBusSenderDemo2**, **Lesson3/ServiceBusReceiverDemo2** (and optionally **Lesson3/ServiceBusDlqRepublish** for step 6). Same **SERVICEBUS_CONNECTION_STRING**.

## Steps

### 1. Enable dead-lettering on the queue

a. In the Azure Portal, open the **Service Bus** namespace from Demo 1 > **Queues** > **order-queue**.  
b. Click **Queue settings** (or **Settings** in the left menu). Enable **Dead-lettering on message expiration** (toggle On).  
c. **Default message time to live**: e.g. 14 days (or leave default). **Max delivery count**: set to **3** (after 3 delivery attempts without complete, the message moves to the DLQ). **Save**.  
d. **Goal**: Messages that are abandoned or never completed will be moved to the dead-letter queue after 3 attempts.

### 2. Trigger a message to go to the dead-letter queue (abandon 3 times)

a. **Send** one message with **Lesson3/ServiceBusSenderDemo2**. Optional body: `$env:SERVICEBUS_BODY = '{"orderId":"dlq-test","item":"Test"}'` ; `cd Lesson3/ServiceBusSenderDemo2` ; `dotnet run`.  
b. **Receiver** in abandon mode: **Lesson3/ServiceBusReceiverDemo2** with **SERVICEBUS_ABANDON=true** (calls **AbandonMessageAsync**). `$env:SERVICEBUS_ABANDON = "true"` ; `cd Lesson3/ServiceBusReceiverDemo2` ; `dotnet run`. Receive once → message is abandoned.  
c. **Run the receiver again** (same command) twice more; **abandon** each time.  
d. After the **third** abandon, the message is moved to the DLQ. In the portal: **order-queue** > **Dead-letter message count** = **1**. **Goal**: Demonstrate automatic dead-lettering after max deliveries.

### 3. Inspect the dead-letter queue and the dead-letter reason

a. **Portal**: **order-queue** > **Service Bus Explorer** > dead-letter sub-queue **order-queue/$DeadLetterQueue**. **Peek** or **Receive** and show **DeadLetterReason** / **DeadLetterErrorDescription**.  
b. **SDK**: Run **Lesson3/ServiceBusReceiverDemo2** with **SERVICEBUS_QUEUE_NAME=order-queue/$DeadLetterQueue**. The app prints body and **DeadLetterReason** / **DeadLetterErrorDescription**. Complete to remove from DLQ after inspection, or leave for step 6.  
c. **Expected**: Message body unchanged; reason (e.g. `MaxDeliveryCountExceeded`) explains why it was dead-lettered. **Goal**: Diagnose failed messages.

### 4. Enable duplicate detection on the queue

a. Open **order-queue** > **Queue settings**. Find **Duplicate detection**. **Duplicate detection history**: set to e.g. **2 minutes** (or 10 minutes). **Save**.  
b. **Goal**: Any message with a **MessageId** that was already sent within the last 2 minutes will be discarded as a duplicate.

### 5. Demonstrate duplicate detection (same MessageId)

a. **Sender** with **MessageId**: **Lesson3/ServiceBusSenderDemo2** with **SERVICEBUS_MESSAGE_ID=order-123**. `$env:SERVICEBUS_MESSAGE_ID = "order-123"` ; `cd Lesson3/ServiceBusSenderDemo2` ; `dotnet run`.  
b. **Within the duplicate detection window** (e.g. 2 minutes), run the **sender again** with the **same** **SERVICEBUS_MESSAGE_ID=order-123**.  
c. Portal: **Active message count** = **1** (second discarded). Run **Lesson3/ServiceBusReceiverDemo2** once (without ABANDON) to receive the first message. **Goal**: Duplicate detection prevents duplicate processing when clients retry with the same MessageId.

### 6. Optional: republish from DLQ to main queue

a. Use **Lesson3/ServiceBusDlqRepublish**: receives one message from **order-queue/$DeadLetterQueue**, sends the same body to **order-queue**, then **completes** the DLQ message.  
b. **PowerShell**: `$env:SERVICEBUS_CONNECTION_STRING = "..."` ; optionally `$env:SERVICEBUS_QUEUE_NAME = "order-queue"` ; `cd Lesson3/ServiceBusDlqRepublish` ; `dotnet run`.  
c. **Expected**: One message moves from DLQ back to the main queue; DLQ count decreases. **Goal**: Simple reprocessing pattern (receive from DLQ → republish → complete DLQ).

## Notes

- **Duplicate detection** requires a **MessageId** set by the sender; the window is in time (e.g. 10 minutes). Use for idempotent processing (e.g. orders). **Dead-letter** reasons are in message properties (e.g. `DeadLetterReason`, `DeadLetterErrorDescription`). See **Exercise 1** for portal-based send/receive; this demo adds reliability features. **Exercise 2** has you send/receive with both Queue Storage and Service Bus and observe visibility timeout vs lock/abandon behavior.
