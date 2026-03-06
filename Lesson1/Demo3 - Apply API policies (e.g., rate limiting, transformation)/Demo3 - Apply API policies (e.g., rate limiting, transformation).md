# Demo 3 – Apply API policies (e.g., rate limiting, transformation)

## Scenario

The **Petstore API** is now in APIM (Demo 1 and 2), but the operations team wants to **control usage and protect the backend**. Some partners might send too many requests ("noisy neighbors"), and you may need to **transform** requests or responses (e.g. add headers, mask sensitive data). In this demo you **apply APIM policies**: for example **rate limiting** (e.g. by key or IP) and **transformation** (set-header, rewrite). This keeps the API predictable and the backend safe without changing the underlying service.

## Objective

- **Apply a rate-limit policy** (e.g. calls per minute by key or IP) to protect the backend from overuse.
- **Apply a transformation policy** (e.g. `set-header` to add or remove headers, or `find-and-replace` in the body).
- Show the effect in the **Test** tab (e.g. 429 after exceeding the limit, or modified response headers).

## Prerequisites

- **Demos 1 and 2** completed (APIM instance with an API imported).
- Access to the **Azure Portal**.

## Steps

### 1. Open the API and the policy editor

a. In the Azure Portal, open your **APIM** instance. Left menu: **APIs** > **Petstore API** (or the API you imported in Demo 2).  
b. In the left column under the API, select **All operations** (so the policy applies to every operation), or select a single operation (e.g. **GET findPetsByStatus**) to limit the policy scope.  
c. In the right pane, locate **Inbound processing**. Click the **`</>`** (code) icon to open the **Policy code editor**.  
d. You will see XML with sections `<inbound>`, `<backend>`, `<outbound>`, `<on-error>`. **Goal**: Add policies inside `<inbound>` so they run before the request is sent to the backend.

### 2. Add the rate-limit policy

a. In the Policy code editor, find the **`<inbound>`** tag.  
b. **Inside** `<inbound>`, add a new line with:  
   `<rate-limit calls="5" renewal-period="60" />`  
   This allows 5 calls per 60 seconds (per subscription/key by default).  
c. Click **Save**. If you get a syntax error, ensure the tag is properly closed and inside `<inbound>`.  
d. **Goal**: After saving, any client exceeding 5 requests per minute will receive **429 Too Many Requests**.

### 3. Add a transformation policy (set-header)

a. In the same policy, still inside **`<inbound>`**, add another policy. For example, to **add** a custom header to the response, add in the **`<outbound>`** section:  
   `<set-header name="X-Custom-Demo" exists-action="override"><value>Demo-Value</value></set-header>`  
   Or to **remove** a header (e.g. hide server info), in `<outbound>`:  
   `<set-header name="X-Powered-By" exists-action="delete" />`  
b. Click **Save**.  
c. **Goal**: Responses will include your custom header or no longer include X-Powered-By, so you can show that APIM transforms the response without changing the backend.

### 4. Test rate limiting (429)

a. Open the **Test** tab for the **Petstore API**. Select one operation (e.g. **GET findPetsByStatus**), set **status** to `available`.  
b. Click **Send** repeatedly **6 times** in quick succession. **Expected**: The first 5 requests return **200 OK**; the 6th returns **429 Too Many Requests** with a message about rate limit exceeded.  
c. **Goal**: Confirm the rate-limit policy is active and protecting the backend.

### 5. Test transformation (response headers)

a. Wait a moment or use a different browser/incognito if the rate limit is still in effect (or temporarily increase `calls` in the policy for this check).  
b. Send one request from the **Test** tab. In the **Response** section, open **Headers**.  
c. **Expected**: If you added `X-Custom-Demo`, it appears in the response headers. If you deleted `X-Powered-By`, it should be absent.  
d. **Goal**: Confirm the transformation policy is applied to the response.

### 6. Optional: rate-limit-by-key (different quotas per client)

a. In the policy, replace `<rate-limit calls="5" renewal-period="60" />` with a snippet that uses a key, e.g. from a query parameter:  
   `<rate-limit-by-key calls="5" renewal-period="60" counter-key="@(context.Request.Url.Query.GetValueOrDefault("client","default"))" />`  
b. **Save**. In **Test**, add query parameter **client** = `A`. Send 5 requests; the 6th returns 429. Then set **client** = `B` and send again. **Expected**: B gets 200 (separate quota).  
c. **Goal**: Show that rate limits can be applied per client (e.g. per partner) when using a key.

## Tests (validation)

Use these checks to confirm the policies behave as expected:

| # | Test | What to do | Expected result |
|---|------|------------|-----------------|
| 1 | **Rate limit – throttle** | In **Test**, select one operation (e.g. GET). Click **Send** repeatedly (e.g. 6 times) within a minute. | First N requests (e.g. 5) return **200 OK**; the next returns **429 Too Many Requests**. |
| 2 | **Rate limit – reset** | Wait until the rate-limit window has passed (e.g. 60 seconds), then click **Send** again. | **200 OK** again; the counter has reset. |
| 3 | **Transformation – header added** | If you added `<set-header name="X-Custom-Demo">`, inspect the **Response headers** in the Test result. | The header **X-Custom-Demo** is present with the value you set. |
| 4 | **Transformation – header removed** | If you used `<set-header name="X-Powered-By" exists-action="delete">`, inspect the response headers. | **X-Powered-By** is absent (or the backend value is overridden). |
| 5 | **rate-limit-by-key** (optional) | If you use `rate-limit-by-key` with e.g. a query parameter: send 5 requests with `?client=A`, then 5 with `?client=B`. | Each "client" has its own quota; B can still get **200** after A is throttled. |
| 6 | **Scope – single operation** | If the policy is applied to **one operation** only: hit that operation until 429, then call a **different** operation. | The other operation still returns **200** (not throttled). |

## Notes

- Policies can be scoped to **All operations**, a single operation, or a product. For production, combine rate limiting with subscription keys. See **Exercise 2** for a full rate-limit and monitoring flow.
