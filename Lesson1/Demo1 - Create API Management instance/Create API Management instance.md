# Demo 1 – Create API Management instance

## Scenario

You are a **Cloud Developer** for a pet supply retailer. The company wants to expose its catalog and services via APIs to partners and mobile apps, but first needs a **secure, centralized gateway** in Azure. In this lesson, you start from a **clean Azure environment** (no pre-existing APIM or connectivity resources). This demo focuses on **provisioning the API Management instance** that will become the single entry point for all API traffic—setting the foundation for the next demos in this lesson.

## Objective

- Create a new **resource group** and an **Azure API Management (APIM)** instance in the portal.
- Choose an appropriate **pricing tier** (e.g. Consumption for labs) and **region**.
- Confirm the APIM instance is deployed and accessible; it will be used as the gateway in the next demos.

## Prerequisites

- An **Azure subscription**.
- Access to the **Azure Portal**.

## Steps

### 1. Create a resource group

a. In the Azure Portal **search bar**, type **Resource groups** and open the blade.  
b. Click **+ Create**. **Subscription**: yours. **Resource group**: e.g. `RG-APIM-Demo`. **Region**: **East US** (or your preferred region).  
c. Click **Review + create**, then **Create**.  
d. **Goal**: Isolated environment for this lesson's APIM demos; no pre-existing resources.

### 2. Start API Management creation

a. In the search bar, type **API Management** and select **API Management services**.  
b. Click **+ Create**. You will configure **Basics**, then **Review + create**.

### 3. Configure APIM Basics

a. **Subscription** and **Resource group**: select `RG-APIM-Demo` (created in step 1).  
b. **Region**: **East US** (same as the resource group).  
c. **Resource name**: e.g. `apim-demo-<unique>` (must be globally unique; try adding your initials or a number).  
d. **Pricing tier**: **Consumption** (no SLA; suitable for demos). Click **Review + create**.  
e. Wait for validation; then click **Create**.

### 4. Wait for deployment and open the instance

a. Deployment may take a few minutes. Go to **Notifications** (bell icon) to track progress.  
b. When deployment completes, click **Go to resource** (or open **API Management services** and select your instance).  
c. On the **Overview** page, note the **Gateway URL** (e.g. `https://apim-demo-xxx.azure-api.net`). This is the base URL for all APIs you will add in the next demos.  
d. **Expected**: APIM is in state **Active**; you can click **APIs** in the left menu and see an empty list (or the default "Echo API" depending on tier).

### 5. Optional: note Developer portal URL

a. In the left menu, under **APIs** or **Developer portal**, you may see a link to the **Developer portal** (where subscribers can see APIs and get keys).  
b. For this demo, the **Gateway URL** is enough; you will call APIs via that hostname in Demo 2 and later.

## Notes

- **Consumption** tier has no SLA and is suited for demos; use **Developer** or **Standard** for production. Lesson is independent: use a dedicated resource group for this lesson's demos.
