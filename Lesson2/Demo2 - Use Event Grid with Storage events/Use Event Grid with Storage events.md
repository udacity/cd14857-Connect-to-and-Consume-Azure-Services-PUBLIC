# Demo 2 – Use Event Grid with Storage events

## Scenario

Building on the Event Grid + Function flow from Demo 1, you now explore **Storage as an event source** in more depth. The same data center needs to react not only to blob creation but potentially to other **Storage events** (e.g. blob deletion, tier changes) and to **filter** which events reach which handler (e.g. by path prefix or event type). In this demo you **configure Event Grid with Storage events**—topic, subscriptions, optional filters—and show how multiple subscribers or a single function can receive only the events that matter, keeping the architecture clean and scalable.

## Objective

- Use **Storage** as an Event Grid **event source** and show **multiple event types** (e.g. Blob Created, Blob Deleted) and **filters** (e.g. subject prefix, event type).
- Create or adjust **Event Grid subscriptions** so only relevant events reach a handler (e.g. a Function or webhook).
- Demonstrate that different subscriptions can receive different subsets of Storage events.

## Prerequisites

- **Demo 1** completed (Storage account, container, and at least one Event Grid subscription to a Function).
- Access to the **Azure Portal** (and optionally the Function App from Demo 1).

## Steps

### 1. Review the existing Event Grid setup

a. In the Azure Portal, open the **Storage account** used in Demo 1 (same resource group as the Function App).  
b. In the left menu, under **Settings** or **Events**, click **Events** (or **Event subscriptions**).  
c. You should see at least one **Event subscription** (e.g. `ToFunction`) created in Demo 1, with **Endpoint type** = Azure Function and **Event types** = Blob Created. Open it and note the **Filters** tab (may be empty so far).  
d. **Goal**: Establish that Storage is the event source and one subscription already sends Blob Created to your Function.

### 2. Create a second subscription with filters (Blob Created, subject filter)

a. In the same Storage account, **Events** > **+ Event subscription**.  
b. **Name**: e.g. `filteredSubscription`. **Event schema**: **Cloud Events Schema v1.0** (same as Demo 1 if your Function binds to `CloudEvent`). **Topic resource**: your Storage account. **System topic name**: as required (e.g. same as storage account name).  
c. **Event types**: keep **Microsoft.Storage.BlobCreated** (Blob Created) only.  
d. Go to the **Filters** tab. Under **Subject filter**: enable **Subject Begins With** and enter `blobServices/default/containers/config-uploads` (adjust the container name if you used a different one in Demo 1). Enable **Subject Ends With** and enter `.json`.  
e. **Endpoint type**: **Azure Function**. Select the **same** Function App and Event Grid-triggered function as in Demo 1. **Create**.  
f. **Goal**: This subscription delivers only Blob Created events for blobs in **config-uploads** whose name ends with **.json**. The Demo 1 subscription is unchanged (all Blob Created in the account, or as originally configured).

### 3. Create a third subscription for Blob Deleted

a. **Events** > **+ Event subscription**.  
b. **Name**: e.g. `ToFunction-OnDelete`. **Event schema**: **Cloud Events Schema v1.0**. **Topic resource**: your Storage account.  
c. **Event types**: **Deselect** Blob Created; **select** **Microsoft.Storage.BlobDeleted** (Blob Deleted).  
d. **Endpoint type**: **Azure Function**. Select the same Function App and Event Grid-triggered function (the default code logs both created and deleted events). **Create**.  
e. **Goal**: You now have three subscriptions—Demo 1 (Blob Created), **filteredSubscription** (Blob Created, config-uploads + .json only), and **ToFunction-OnDelete** (Blob Deleted).

### 4. Test: file in another container (filtered subscription does not match)

a. In the Storage account, **Containers** > **+ Container**. Create a new container, e.g. `secondcontainer`.  
b. Open **secondcontainer** and **Upload** a file, e.g. `secondText.txt`.  
c. In the **Function App** > your Event Grid-triggered function > **Monitor**, check executions. **Expected**: The **Demo 1** subscription (all Blob Created) may fire once; **filteredSubscription** does **not** fire (wrong container path and file is `.txt`, not `.json`). So you see at most one execution for this upload.

### 5. Test: Blob Deleted (second subscription)

a. In **Containers** > **config-uploads**, **delete** an existing blob (e.g. `test.txt` from Demo 1).  
b. In the Function **Monitor** / **Logs**, confirm a new **execution**. In the event payload, verify **type** (CloudEvents) or **eventType** is `Microsoft.Storage.BlobDeleted` and the **subject** points to the deleted blob.  
c. **Expected**: Only **ToFunction-OnDelete** fires; the Blob Created subscriptions do not fire for deletes.

### 6. Test: Blob Created matching the filter (.json in config-uploads)

a. In **Containers** > **config-uploads**, **Upload** a file named `test.json` (any small content).  
b. In the Function **Monitor**, confirm **two** new executions for this single upload: one from the **Demo 1** subscription (Blob Created, no subject filter) and one from **filteredSubscription** (Blob Created, subject begins with config-uploads and ends with .json).  
c. **Expected**: One Storage event (Blob Created for test.json) is delivered to both subscribers that match—demonstrating filtering (only .json in config-uploads for filteredSubscription) and fan-out (same event to multiple subscriptions).

## Notes

- Storage emits events to Event Grid automatically when the topic is the Storage account. Use filters to avoid unnecessary invocations. For more event types (e.g. tier change), see Azure docs. **Exercise 1** uses Blob Created only; this demo extends to filters and multiple events.
