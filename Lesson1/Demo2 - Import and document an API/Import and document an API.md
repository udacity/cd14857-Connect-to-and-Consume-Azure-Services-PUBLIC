# Demo 2 – Import and document an API

## Scenario

Using the **API Management instance** you created in Demo 1, you now need to **onboard a real API** so that partners and developers can call it through the gateway. The company has an existing **Petstore**-style API (or an internal API) described by an **OpenAPI specification**. In this demo you **import** that API into APIM and **add documentation** (descriptions, examples) so that consumers understand how to use it. All traffic will flow through the same APIM instance, with a clear, documented surface.

## Objective

- **Import** an API into APIM from an **OpenAPI** specification (e.g. Petstore URL or a local file).
- Set **display name** and **API URL suffix** so the API is reachable via the gateway.
- Add or review **documentation** (descriptions, examples) so developers know how to use the API.

## Prerequisites

- **Demo 1** completed (APIM instance created).
- Access to the **Azure Portal**.

## Steps

### 1. Open APIM and start adding an API from OpenAPI

a. In the Azure Portal, open your **API Management** instance (created in Demo 1).  
b. In the left menu, click **APIs** (under "API Management").  
c. Click **+ Add API**. In the list of options, select **OpenAPI** (under "Create from definition" or similar).  
d. **Goal**: Create a new API in APIM whose operations and backend URL come from an OpenAPI spec.

### 2. Enter the OpenAPI specification and API details

a. In **Create from OpenAPI specification**, **OpenAPI specification**: paste the URL  
   `https://petstore.swagger.io/v2/swagger.json`  
   (or upload a local OpenAPI JSON/YAML file if you use an internal API).  
b. **Display name**: e.g. `Petstore API`. This name appears in the developer portal and in the APIs list.  
c. **API URL suffix**: e.g. `petstore`. The full path to this API will be `https://<your-apim>.azure-api.net/petstore`.  
d. Leave **API version** and **Versioning scheme** as default unless you need versioning.  
e. Click **Create**. APIM will create the API and its operations from the spec.

### 3. Review operations and add documentation

a. Open the newly created **Petstore API** (click it in the APIs list).  
b. In the left column, under the API name, you see **Design**, **Test**, **Settings**, etc. Click **Design** (or **Overview**).  
c. Review the **Operations** list (e.g. GET /pet/findByStatus, GET /pet/{petId}, etc.). Click one operation (e.g. **GET findPetsByStatus**).  
d. In the **Design** tab for that operation, add or edit **Description** (e.g. "Returns pets matching the given status") and **Query parameters** description if needed. Optionally add **Example** in Request/Response.  
e. Repeat for one or two other operations so the API is clearly documented. **Save** if prompted.  
f. **Goal**: Developers see clear descriptions and examples when they browse the API in the portal or developer portal.

### 4. Test the API through the gateway

a. Open the **Test** tab for the **Petstore API**.  
b. Select an operation (e.g. **GET findPetsByStatus**). Set the **status** query parameter to `available`.  
c. Click **Send**. **Expected**: **200 OK** and a JSON body with pet data. The request went through the APIM gateway to the Petstore backend.  
d. Optionally try another operation (e.g. GET /pet/{petId} with a valid pet Id) to confirm the gateway forwards correctly.  
e. **Goal**: Confirm the API is reachable at `https://<your-apim>.azure-api.net/petstore/...` and responds as expected.

## Notes

- The backend URL in the spec is used by APIM to forward requests; for Petstore, the public URL is used. For internal APIs, use your backend base URL. Align naming with **Exercise 1** if you run the exercise in the same lesson.
