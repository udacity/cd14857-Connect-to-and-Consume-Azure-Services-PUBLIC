# Demo 1 – Trigger Azure Function from Event Grid

## Scenario

You are building an **event-driven security workflow** for a data center: when a new configuration file is dropped into a blob container, a process must run automatically—for example to validate or log it. In this lesson you start from a **clean Azure environment** (no pre-existing Event Grid or Function resources). This demo shows how **Azure Event Grid** and an **Azure Function** (Event Grid trigger) work together: you create a Storage account and a container, deploy a Function that reacts to **Blob Created** events, and wire them with an Event Grid subscription. By the end, a single file upload triggers the function without any polling.

## Objective

- Create a **resource group**, a **Storage account** with a **container**, and an **Azure Function App** with an **Event Grid trigger**.
- Create an **Event Grid subscription** from the Storage account (event source) to the Function (handler), filtering on **Blob Created**.
- **Test** the flow by uploading a file to the container and verifying the Function runs (Monitor / Logs).

## Prerequisites

- An **Azure subscription** and access to the **Azure Portal** (for resource group, Storage account, Event Grid subscription).
- **Azure CLI** installed ([Installation guide](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli)); required for login and subscription selection before deploy.
- **Azure Functions Core Tools** (v4) installed locally ([Installation guide](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local)).
- A **runtime** for your chosen stack: **Node.js** (LTS), **Python** (3.9+), or **.NET** (SDK 6+) depending on the template you use.
- A terminal (e.g. **VS Code** integrated terminal or PowerShell). The Function is **created and deployed** from the command line; the default Event Grid trigger template generates code that logs the event (no custom code required for this demo).
- This lesson starts from a **clean environment** (no existing Event Grid or Function for this lesson).

## Steps

### 1. Create a resource group

a. In the Azure Portal **search bar**, type **Resource groups** and open it. Click **+ Create**.  
b. **Resource group**: e.g. `RG-EventGrid-Demo`. **Region**: **East US**. Click **Review + create**, then **Create**.  
c. **Goal**: Clean environment for this lesson; all resources (Storage, Function, Event Grid) will go in this group.

### 2. Create the Storage account and container (event source)

a. Search for **Storage accounts** > **+ Create**. **Resource group**: `RG-EventGrid-Demo`. **Storage account name**: e.g. `steventgriddemo<unique>` (globally unique). **Region**: **East US**. **Performance**: Standard. Create the account.  
b. When deployment completes, open the Storage account. Left menu: **Data storage** > **Containers** > **+ Container**.  
c. **Name**: `config-uploads`. **Public access level**: Private. Click **Create**.  
d. **Goal**: The event source; when a blob is created in this container, Storage will emit a **Blob Created** event to Event Grid.

### 3. Create the Function App (Azure) and the Event Grid trigger function (terminal / Core Tools)

a. **Create the Function App in Azure** (Portal or CLI). In the Portal: search **Function App** > **+ Create**. **Resource group**: `RG-EventGrid-Demo`. **Function App name**: e.g. `func-eventgrid-demo<unique>`. **Runtime stack**: match your local choice—e.g. **.NET 8 Isolated**, **Python** (3.9+), or **Node**. **Region**: **East US**. **Hosting**: **Consumption (Serverless)**. Create the app. Wait for deployment to complete.  
b. **Create the project locally from the terminal**. Open a terminal (e.g. VS Code integrated terminal). Create a folder and go into it. **Specify the runtime explicitly**: `func init` without options does **not** prompt and may default to .NET 6; to align with Azure (e.g. .NET 8 Isolated), run:
   - **.NET 8 Isolated**: `func init --worker-runtime dotnet-isolated --target-framework net8.0`
  
   Then add the function: `func new` — choose template **Event Grid trigger**, **Function name** e.g. `ProcessConfigUpload`. (With .NET isolated, do **not** pass `--language` to `func new`.) The generated code logs the event; you do **not** need to change it for this demo.  
c. **Run locally (optional)**. In the project folder: `func start`. Confirm the function starts; stop with Ctrl+C after a quick check.  
d. **Select the correct Azure account and subscription** (if you have several). Log in: `az login`. List subscriptions: `az account list --output table`. Set the subscription to use: `az account set --subscription "<subscription name or id>"`. Confirm: `az account show`.  
e. **Deploy to the Function App**. From the project folder: `func azure functionapp publish func-eventgrid-demo<unique>` (replace with your app name). Wait for the deployment to succeed.  
f. **Goal**: The event handler is deployed in Azure; Event Grid will POST events to this function's URL when you create the subscription in the next step.

### 4. Create the Event Grid subscription (Storage → Function)

a. Open your **Storage account** again. Left menu: **Events** (under "Settings" or "Data management"). If you don't see it, search for **Event subscriptions** in the Storage account or use **Events** from the resource menu.  
b. Click **+ Event subscription**. **Name**: e.g. `ToFunction`. **Event schema**: **Cloud Events Schema v1.0** (the Event Grid trigger template binds to `CloudEvent`; use Event Grid Schema only if your function expects that format). **Topic resource** should show your Storage account.  
c. **Event types**: Click **Filter to Event Types** and select only **Blob Created** (deselect others).  
d. **Endpoint type**: **Azure Function**. Click **Select an endpoint**. Choose your **Function App** and the function **ProcessConfigUpload**. Click **Confirm selection**, then **Create**.  
e. **Expected**: The subscription appears in the list; status becomes **Succeeded**. **Goal**: From now on, every blob created in this Storage account will trigger an event to your Function.

### 5. Test end-to-end (upload → Function runs)

a. Open **Storage account** > **Containers** > **config-uploads**. Click **Upload** > choose a small file (e.g. create a text file `test.txt` with "hello" and upload it) > **Upload**.  
b. Open your **Function App** > **Functions** > **ProcessConfigUpload**. Left menu: **Monitor** (or **Code + Test** > **Logs**).  
c. Within a few seconds you should see a new **invocation** (e.g. status **200**). Click the invocation to see the **execution details** and **logs**. The event payload (e.g. `data.url` with the blob URL, `subject` with the blob path) should appear in the logs.  
d. **Expected**: One file upload = one Function execution. No polling; Event Grid pushed the event to the Function. **Goal**: Confirm the full flow works.

## Notes

- Event Grid uses a **push** model: it invokes the Function when an event occurs. No polling. For production, secure the endpoint and consider retry/dead-letter behavior. See **Exercise 1 – Trigger Azure Function via Event Grid** for a full lab.
