# Connected Microservices with Container Apps

[Azure Container Apps(Preview)](https://docs.microsoft.com/en-us/azure/container-apps/overview) enables users to run containerized applications in a completely Serverless manner providing complete isolation of *Orchestration* and *Infrastructure*. Applications built on Azure Container Apps can dynamically scale based on the various triggers as well as [KEDA-supported scalers](https://keda.sh/docs/scalers/)

Features of Azure Container Apps include:

- Run multiple **Revisions** of containerized applications
- **Autoscale** apps based on any KEDA-supported scale trigger
- Enable HTTPS **Ingress** without having to manage other Azure infrastructure like *L7 Load Balancers* 
- Easily implement **Blue/Green** deployment and perform **A/B Testing** by splitting traffic across multiple versions of an application
- **Azure CLI** extension or **ARM** templates to automate management of containerized applications
- Manage Application **Secrets** securely
- View **Application Logs** using *Azure Log Analytics*
- **Manage** multiple Container Apps using [Self-hosted Gateway](https://docs.microsoft.com/en-us/azure/api-management/self-hosted-gateway-overview) feature of Azure APIM providing rich APIM Policies and Authentication mechainsms to the Container Apps

This article would demonstrate:

- [How to Setup Azure Container Apps using Azure CLI](#How to Setup)
- [How to Deploy a containerized *Logic App* as Azure Container App](#Deploy Azure Logic App as Container App)
- [How to Deploy a containerized *Azure Function* as Azure Container App](#Deploy Azure Function as Container App)
- [Deploy the *Self-hosted Gateway* component of an APIM instance as a Container App itself](#Deploy Self-hosted Gateway as Container App)
- [Integrate the two Container Apps with APIM Container App](#Integrate All using APIM)
- [Test the entire flow end to end](#Test End-to-End)



## How to Setup

#### Set CLI Varibales

```bash
tenantId="<tenantId>"
subscriptionId="<subscriptionId>"
resourceGroup="<resourceGroup>"
monitoringResourceGroup="<monitoringResourceGroup>?"
location="<location>"
logWorkspace="<logWorkspace>"
basicEnvironment="basic-env"
securedEnvironment="secure-env"
acrName="<acrName>"
registryServer="<container_registry_server>"
registryUserName="<container_registry_username>"
registryPassword="<container_registry_password>"

# Optional - NOT a requirement for Contyainer Apps but mostly for microservice applications
storageName="<storage_account_name>"

# Optional - Primary for Securing Container Apps
containerAppVnetName="containerapp-workshop-vnet"
containerAppVnetId=

# Optional - Subnet for Control plane of the Container Apps Infrastructure
controlPlaneSubnetName="containerapp-cp-subnet"
controlPlaneSubnetId=

# Optional - Subnet for hosting Container Apps
appsSubnetName="containerapp-app-subnet"
appsSubnetId=

# Both Control plane Subnet and Application Services Subnet should be in same VNET viz. $containerAppVnetName
```

#### Configure Azure CLI

```bash
# Add CLI extension for Container Apps
az extension add \
  --source https://workerappscliextension.blob.core.windows.net/azure-cli-extension/containerapp-0.2.0-py2.py3-none-any.whl
  
# Register the Microsoft.Web namespace
az provider register --namespace Microsoft.Web
az provider show --namespace Microsoft.Web
```

#### Create Resourcer Groups

```bash
# Hosting Container Apps
az group create --name $resourceGroup --location $location

# Hosting Log Analytics Workspace for Container Apps
az group create --name $monitoringResourceGroup --location $location
```

#### Create Log Analytics Workspace

```bash
az monitor log-analytics workspace create --resource-group $monitoringResourceGroup --workspace-name $logWorkspace

# Retrieve Log Analytics ResourceId
logWorkspaceId=$(az monitor log-analytics workspace show --query customerId -g $monitoringResourceGroup -n $logWorkspace -o tsv)

# Retrieve Log Analytics Secrets
logWorkspaceSecret=$(az monitor log-analytics workspace get-shared-keys --query primarySharedKey -g $monitoringResourceGroup -n $logWorkspace -o tsv)
```

#### Create Container App Environment

```bash
# Simple environment with no additional security for the underlying sInfrastructure
az containerapp env create --name $basicEnvironment --resource-group $resourceGroup \
  --logs-workspace-id $logWorkspaceId --logs-workspace-key $logWorkspaceSecret --location $location
```



## Deploy Azure Logic App as Container App

## Deploy Azure Function as Container App

## Deploy Self-hosted Gateway as Container App

## Integrate All using APIM

## Test End-to-End 