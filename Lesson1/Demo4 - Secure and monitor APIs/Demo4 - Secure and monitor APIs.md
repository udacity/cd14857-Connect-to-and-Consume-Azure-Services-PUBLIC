# Demo 4 – Secure and monitor APIs

## Scenario

The API is imported, documented, and protected by policies (Demos 1–3). The final step is to **harden security** using **Products** and **subscription keys** so that only authorized clients (partners, apps) can call the API, and to **enable monitoring** so the team can see who is calling and how. In this demo you **create a Product** (a bundle of APIs and policies), **create Subscriptions** that issue **primary and secondary keys**, enforce **subscription required** on the API, and then **validate** that calls without a key get **401** while calls with a valid key succeed. You also set up **Metrics** and **Logs** for visibility. By the end, the Petstore API is secured by subscription keys and observable in the portal.

## Objective

- **Understand Products and Subscriptions**: a **Product** groups one or more APIs (and can have its own policies); a **Subscription** belongs to a product and provides **primary** and **secondary** keys that clients send in the `Ocp-Apim-Subscription-Key` header.
- **Create a Product**, add the API to it, and **create a Subscription** to obtain subscription keys.
- **Enforce subscription-based security**: set **Subscription required** on the API; demonstrate **401** when the key is missing and **200 OK** when a valid key is sent.
- **Enable monitoring**: use APIM **Metrics** and **Logs** (or **Diagnostic settings** to Log Analytics / Application Insights) to observe requests, status codes, and latency.

## Prerequisites

- **Demos 1–3** completed (APIM with API imported and policies applied).
- Access to the **Azure Portal**.

## Steps

### 1. Enable subscription requirement on the API

a. Open **APIM** > **APIs** > your API (e.g. Petstore) > **Settings**.  
b. Under **Subscription**, check **Subscription required**. **Save**.

### 2. Create a Product and add the API

a. Go to **APIM** > **Products** (left menu). Click **+ Add**.  
b. **Display name**: e.g. `Petstore – Partners`. **Description**: e.g. "API access for partners". Leave **Requires subscription** checked (default). **State**: **Published**. **Create**.  
c. Open the new product. Go to **APIs** (under the product). Click **+ Add** and attach your **Petstore API** (or the API you imported in Demo 2). Save.

### 3. Create a Subscription and get the keys

a. In the same **Product** (or from **APIM** > **Subscriptions**), go to **Subscriptions**. Click **+ Add subscription**.  
b. **Name**: e.g. `Partner-A`. **Scope**: **Product** and select `Petstore – Partners`. **Create**.  
c. Open the new subscription. Note the **Primary key** and **Secondary key**. These are the **subscription keys** clients must send in the header `Ocp-Apim-Subscription-Key`. (Optionally use **Regenerate** to create new keys and invalidate old ones.)

### 4. Test with and without the subscription key

a. **Without key**: Call the API URL in a browser or Postman **without** the `Ocp-Apim-Subscription-Key` header. **Expected:** **401 Unauthorized** (or "Access denied due to missing subscription key").  
b. **With key**: In **APIM** > **APIs** > your API > **Test**, the subscription key is usually auto-filled. Click **Send**. **Expected:** **200 OK**.  
c. **With key in tool**: In Postman or curl, add header `Ocp-Apim-Subscription-Key: <primary-key>`. Call the same operation. **Expected:** **200 OK**.

### 5. Monitoring

a. In APIM, go to **Monitoring** > **Metrics**. Show **Requests**, **Response code** distribution (e.g. 200 vs 401), and **Latency**.  
b. Optionally: **Diagnostic settings** (APIM resource) and send logs to a **Log Analytics** workspace; then use **Logs** to query by subscription name or response code.

## Products and subscription keys (recap)

| Concept | Role |
|--------|------|
| **Product** | Groups APIs (and optional policies). Defines who can subscribe (e.g. "Partners", "Internal"). |
| **Subscription** | Belongs to a product. Provides **Primary** and **Secondary** keys. Each key identifies the subscriber; APIM can apply rate limits or quotas per subscription. |
| **Subscription required** | On each API (or operation): when checked, every request must include a valid `Ocp-Apim-Subscription-Key`; otherwise APIM returns 401. |
| **Key in request** | Client sends header: `Ocp-Apim-Subscription-Key: <primary-or-secondary-key>`. |

## Notes

- **Secondary key** is for key rotation: switch clients to the secondary, regenerate the primary, then switch back. For production, consider **OAuth 2.0** or **managed identity** in addition to subscription keys. Align with **Exercise 1** (Create and Secure an API) and **Exercise 2** (Apply Rate Limiting and Monitor Traffic) for hands-on practice.
