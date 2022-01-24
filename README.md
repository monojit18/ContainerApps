# Get the Best of Containers, Spend No Time on K8s!

## Introduction

Cloud Native application deployment using a K8s based service or tool is now. coomon practice providing -

- Immense Flexibility
- Scalability
- Resiliency
- Reliability

This allows organizations to rollout Stable releases much faster. But there are downsides of this approach as well:

- Need to have a deep insights into K8s eco-system
- Managing the K8s Cluster - its security, performance, upgrades
- Additional effort needed for ensuring Application Isolation, Security, Multi-tenancy
- Solid implementation of Container Insight solutions for continuous monitoring of Pods, Services and Nodes
- Imperative to have some 3rd party solutions like Service Mesh to have better insights into application flow and integrations for complex systems with large no of granular Microservice

While managed services like AKS provides a lot of relief to the Organizations but they want to move towards an even more Managed solution that can take away the complexities of K8s eco-system and its subsequent management; yet they do not want to compromise on most of the K8s benefits.

[Azure Container Apps](https://docs.microsoft.com/en-us/azure/container-apps/overview) is a service aimed at solving this problem and make Microservices deployment easier and quicker!

### What the Document does

- A deep insights into [Azure Container Apps](https://docs.microsoft.com/en-us/azure/container-apps/overview)
  - Benefits
  - Features
  - How to Setup - *Azure CLI* and *ARM*
  - Connected Examples


### What the Document does NOT

- Deep-dive on [K8s](https://kubernetes.io/docs/home/)

- Deep-dive on [AKS](https://docs.microsoft.com/en-us/azure/aks/intro-kubernetes)

- Programmatic aspects viz. integrations with [Dapr](https://dapr.io/) etc.

  

## Overview

![azure-container-apps-revisions](./Assets/azure-container-apps-revisions.png)

Azure Container Apps enables users to run containerized applications in a completely Serverless manner providing complete isolation of *Orchestration* and *Infrastructure*. Few Common uses of *Azure Container Apps* include:

- Deploying API endpoints
- Hosting background processing applications
- Handling event-driven processing
- Running microservice

Applications built on Azure Container Apps can dynamically scale based on the various triggers as well as [KEDA-supported scaler](https://keda.sh/docs/scalers/)

Features of Azure Container Apps include:

- Run multiple **Revisions** of containerized applications
- **Autoscale** apps based on any KEDA-supported scale trigger
- Enable HTTPS **Ingress** without having to manage other Azure infrastructure like *L7 Load Balancers* 
- Easily implement **Blue/Green** deployment and perform **A/B Testing** by splitting traffic across multiple versions of an application
- **Azure CLI** extension or **ARM** templates to automate management of containerized applications
- Manage Application **Secrets** securely
- View **Application Logs** using *Azure Log Analytics*.

## Plan

### How to Setup

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



### Deploy Apps with Container Apps

#### httpcontainerapp

- A Containerized Application which responds to http Post requests
- The app is built with Azure Function for Http trigger
- Only returns some pre-formatted response message

```bash
httpImageName="$registryServer/httpcontainerapp:v1.0.0"
azureWebJobsStorage="<Storage Connection string as needed by Azure Function>"

# Deploy Container App
az containerapp create --name httpcontainerapp --resource-group $resourceGroup \
--image $httpImageName --environment $basicEnvironment \
--registry-login-server $registryServer --registry-username $registryUserName \
--registry-password $registryPassword \
#External Ingress - generates a Public FQDN
--ingress external --target-port 80 --transport http \
# Min/Max Replicas
--min-replicas 1 --max-replicas 5 \
# CPU/Memory specs; similar to resource quota requests oin K8s Deployment manifest
--cpu 0.25 --memory 0.5Gi \
# Secrets needed by Azure Function App; similar to K8s secrets
--secrets azurewebjobsstorage=$azureWebJobsStorage \
# Environment variables assigned from secrets created; similar to secretRef in K8s Deployment manifest
--environment-variables "AzureWebJobsStorage=secretref:azurewebjobsstorage"
```

- Creates a simple Container App with *External* Ingress

  ![containerapp-simple-1](./Assets/containerapp-simple-1.png)

  - Generates a *Public FQDN*

    ![http-containerapp-overview](./Assets/http-containerapp-overview.png)

    - The App can be accessed from anywhere
    - No separate Load Balancer in dded to maintain; Azure does it automatically

  - *--target-port* indicates the Container Port; basically as exposed in Dockerfile and similar to ***containerPort*** in K8s Deployment manifest

  - This Deployment also ensures a *minimum of 1 replica* and *maximum of 5 replicas* for this App

  - Azure Container Registry credentials are passed as CLI arguments

    - *--registry-login-server*
    - *--registry-username*
    - *--registry-password*

  - *CPU* and *Memory* is also specified - similar to resource quota in K8s Deployment manifest

    ![containerapp-external-ingress](./Assets/containerapp-external-ingress.png)

  - Secrets are added as part of the Container App Deployment process

    ![containerapp-secrets](./Assets/containerapp-secrets.png)

- Manage Revisions

  - Get a list of Revisions

    ```bash
    az containerapp revision list --name httpcontainerapp --resource-group $resourceGroup --query="[].name"
    ```

  - Deactivate/Activate Revisions

    ![azure-container-apps-lifecycle-deactivate](./Assets/azure-container-apps-lifecycle-deactivate.png)

    ```bash
    az containerapp revision deactivate --name "<revision_name>" --app httpcontainerapp \
    --resource-group $resourceGroup
    
    az containerapp revision activate --name "<revision_name>" --app httpcontainerapp \
    --resource-group $resourceGroup
    ```

- Split Traffic

  ![azure-container-apps-revisions-traffic-split](./Assets/azure-container-apps-revisions-traffic-split.png)

  - Split Traffic between two revisions by 50%

    ```bash
    az containerapp update --traffic-weight "httpcontainerapp--rv1=50,httpcontainerapp--rv2=50" \
    --name httpcontainerapp --resource-group $resourceGroup
    ```

  - Route all Traffic to latest revision

    ```bash
    # Assuming httpcontainerapp--rv2 as the latest Revision
    az containerapp update --traffic-weight "httpcontainerapp--rv1=0,httpcontainerapp--rv2=100" \
    --name httpcontainerapp --resource-group $resourceGroup
    ```

    ![containerapp-provisioning](./Assets/containerapp-provisioning.png)

#### httpcontainerapp-secured

- A Containerized Application which responds to http Post requests

- The app is built with Azure Function for *Http trigger*

- Only returns some pre-formatted response message

- Application runs within a Secured Container App Environment

- Create a **Secured** Environment for the Container App

  ```bash
  az containerapp env create --name $securedEnvironment --resource-group $resourceGroup \
    --logs-workspace-id $logWorkspaceId --logs-workspace-key $logWorkspaceSecret --location $location \
    # Subnet for Control Plane Infrastructure
    --controlplane-subnet-resource-id $controlPlaneSubnetId \
      # Subnet for Container App(s)
    --app-subnet-resource-id $appsSubnetId
    
  # Both Control plane Subnet and Application Services Subnet should be in same VNET viz. $containerAppVnetName
  ```

- Create secured Container app injected into the *Virtual Network*

  ```bash
  az containerapp create --name httpcontainerapp-secured --resource-group $resourceGroup \
  # Secured Environment for the Container App
    --image $httpImageName --environment $securedEnvironment \
    --registry-login-server $registryServer --registry-username $registryUserName \
    --registry-password $registryPassword \
    # Ingress: Internal; generates Private FQDN, no access from outside of the Virtual Network
    --ingress internal --target-port 80 --transport http \
    --min-replicas 1 --max-replicas 5 \
    --cpu 0.25 --memory 0.5Gi \
    --secrets azurewebjobsstorage=$azureWebJobsStorage \
    --environment-variables "AzureWebJobsStorage=secretref:azurewebjobsstorage"
  ```

  - Application would run within a specified Virtual Network
  - Internal/Private FQDN for the form - *<APP_NAME>.internal.<UNIQUE_IDENTIFIER>.<REGION_NAME>.azurecontainerapps.io*
  - All Applicationds within the same *Secured Environment* would share same internal/Private IP address

#### httpcontainerapp-mult

- A Containerized Application which responds to http Post requests

- The app is built with Azure Function for *Http trigger*

- Only returns some pre-formatted response message

- Application running within a *Virtual Network*

- External Ingress to accept calls from Outside of the Virtual Network

- Would call **[httpcontainerapp-secured](#httpcontainerapp-secured)** internally - since both exist within the same *Virtual Network*

  ![containerapp-secured-1](./Assets/containerapp-secured-1.png)

  ```bash
  az containerapp create --name httpcontainerapp-mult --resource-group $resourceGroup \
    --image $httpImageName --environment $securedEnvironment \
    --registry-login-server $registryServer --registry-username $registryUserName \
    --registry-password $registryPassword \
    --ingress external --target-port 80 --transport http \
    --min-replicas 1 --max-replicas 5 \
    --cpu 0.25 --memory 0.5Gi \
    --secrets azurewebjobsstorage=$azureWebJobsStorage \
    --environment-variables "AzureWebJobsStorage=secretref:azurewebjobsstorage"
  ```

#### blobcontainerapp

![containerapp-blob](./Assets/containerapp-blob.png)

- A Containerized [Application](https://raw.githubusercontent.com/monojit18/ContainerApps/master/Microservices/BlobContainerApp/BlobContainerApp/BlobContainerApp.cs?token=GHSAT0AAAAAABM52P35TSLNLMW3NCVOVZXCYPXAB6A) which responds toBlob events

- The app is built with Azure Function for *Blob trigger*

  ```bash
  az containerapp create --name blobcontainerapp --resource-group $resourceGroup \
    --image $blobImageName --environment $basicEnvironment \
    --registry-login-server $registryServer --registry-username $registryUserName \
    --registry-password $registryPassword \
    --min-replicas 1 --max-replicas 10 \
    --secrets azurewebjobsstorage=$azureWebJobsStorage \
    --environment-variables "AzureWebJobsStorage=secretref:azurewebjobsstorage"
  ```

- Unlike previous apps, *NO* **Ingress** is specified here; since the application is listening to the Blob events which is an *Outbound* call

  ![containerapp-ingress-disabled](./Assets/containerapp-ingress-disabled.png)

  - No FQDN is generated as Ingress is disabled
  - No *InBound* call is needed (*or possible*)
  - Application responds to the Blob storage events Only



### Deploy with ARM templates

- Deploy **[blobcontainerapp](#blobcontainerapp)** using ARM

  ```json
  {
      "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
      "contentVersion": "1.0.0.0",
      "parameters": {
          "containerappName": {
              "defaultValue": "blobcontainerapp",
              "type": "String"
          },
          "location": {
              "defaultValue": "eastus",
              "type": "String"
          },
          "environmentName": {
              "defaultValue": "basic-env",
              "type": "String"
          },
          "imageName": {
              "defaultValue": "",
              "type": "String"
          },
          "acrServer": {
              "defaultValue": "",
              "type": "String"
          },
          "acrUsername": {
              "defaultValue": "",
              "type": "String"
          },
          "acrPassword": {
              "defaultValue": "",
              "type": "String"
          },
          "azureWebjobsStorage": {
              "defaultValue": "",
              "type": "String"
          }
      },
      "variables": {
  
          "passwordSecretName": "passwordsecret",
          "storageSecretName": "azurewebjobsstorage"
  
      },
      "resources": [
          {
              "apiVersion": "2021-03-01",
              "type": "Microsoft.Web/containerApps",
              "name": "[parameters('containerappName')]",
              "location": "[parameters('location')]",
              "properties": {
                  "kubeEnvironmentId": "[resourceId('Microsoft.Web/kubeEnvironments', parameters('environmentName'))]",
                  "configuration": {   
                      "secrets": [{
                          "name": "azurewebjobsstorage",
                          "value": "[parameters('azureWebjobsStorage')]"
                      },
                      {
                          "name": "passwordsecret",
                          "value": "[parameters('acrPassword')]"
  
                      }],
                      "registries": [{
                          "server": "[parameters('acrServer')]",
                          "username": "[parameters('acrUsername')]",
                          "passwordSecretRef": "[variables('passwordSecretName')]"
                      }]
                  },
                  "template": {                    
                      "containers": [
                          {
                              "name": "blob-container",
                              "image": "[parameters('imageName')]",                            
                              "env": [
                                  {
                                      "name": "AzureWebJobsStorage",
                                      "secretRef": "[variables('storageSecretName')]"
                                  }                                
                              ],
                              "resources": {
                                  "cpu": 0.5,
                                  "memory": "1Gi"
                              }
                          }
                      ],                    
                      "scale": {
                          "minReplicas": 1,
                          "maxReplicas": 10,
                          "rules": [
                          {
                              "name": "blob-scaling",
                              "custom": {
                                  "type": "azure-blob",
                                  "metadata": {
                                      "blobContainerName": "blobcontainerapp",
                                      "blobCount": "3"
                                  },
                                  "auth": [{
                                      "secretRef": "azurewebjobsstorage",
                                      "triggerParameter": "connection"
                                  }]
                              }
                          }]
                      }
                  }
              }
          }
      ]
  }
  ```

  ```bash
  blobImageName="$registryServer/blobcontainerapp:v1.0.0"
  azureWebJobsStorage="<Storage connection string as needed by Azure Function>"
  
  az deployment group create -f ./blob-deploy.json -g $resourceGroup \
  --parameters imageName=$blobImageName acrServer=$registryServer \
  acrUsername=$registryUserName acrPassword=$registryPassword azureWebjobsStorage=$azureWebJobsStorage
  ```

  - Secret values are passed to the Container Apps through *secrets* section in the template

    ```json
    "secrets": [{
                  "name": "azurewebjobsstorage",
                  "value": "[parameters('azureWebjobsStorage')]"
                },
                {
                  "name": "passwordsecret",
                  "value": "[parameters('acrPassword')]"
    
                }]
    ```

  - Scaling configuration is provided by the *scale* section of the template

    - Refer [Scale Triggers](https://docs.microsoft.com/en-us/azure/container-apps/scale-app) as supported by Container Apps
    - Scale *type* and *metadata* are similar to what [KEDA Scalers](https://keda.sh/docs/2.5/scalers/) provie us with

    ```json
    "scale": {
        "minReplicas": 1,
        "maxReplicas": 10,
        "rules": [
        {
            "name": "blob-scaling",
            "custom": {
            "type": "azure-blob",
            "metadata": {
            "blobContainerName": "blobcontainerapp",
            "blobCount": "3"
            },
            "auth": [{
            "secretRef": "azurewebjobsstorage",
            "triggerParameter": "connection"
            }]
        	}
        }]
    }
    ```

    

- Deploy **[httpcontainerapp](#httpcontainerapp)** using ARM

  ```json
  {
      "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
      "contentVersion": "1.0.0.0",
      "parameters": {
          "containerappName": {
              "defaultValue": "httpcontainerapp",
              "type": "String"
          },
          "location": {
              "defaultValue": "eastus",
              "type": "String"
          },
          "environmentName": {
              "defaultValue": "basic-env",
              "type": "String"
          },
          "imageName": {
              "defaultValue": "",
              "type": "String"
          },
          "acrServer": {
              "defaultValue": "",
              "type": "String"
          },
          "acrUsername": {
              "defaultValue": "",
              "type": "String"
          },
          "acrPassword": {
              "defaultValue": "",
              "type": "String"
          },
          "azureWebjobsStorage": {
              "defaultValue": "",
              "type": "String"
          },
          "revisionSuffix": {
              "defaultValue": "",
              "type": "String"
          }
      },
       "variables": {
  
          "passwordSecretName": "passwordsecret",
          "storageSecretName": "azurewebjobsstorage"
  
      },
      "resources": [
          {
              "apiVersion": "2021-03-01",
              "type": "Microsoft.Web/containerApps",
              "name": "[parameters('containerappName')]",
              "location": "[parameters('location')]",
              "properties": {
                  "kubeEnvironmentId": "[resourceId('Microsoft.Web/kubeEnvironments', parameters('environmentName'))]",
                  "configuration": {   
                      "secrets": [{
                          "name": "azurewebjobsstorage",
                          "value": "[parameters('azureWebjobsStorage')]"
                      },
                      {
                          "name": "passwordsecret",
                          "value": "[parameters('acrPassword')]"
  
                      }],
                      "registries": [{
                          "server": "[parameters('acrServer')]",
                          "username": "[parameters('acrUsername')]",
                          "passwordSecretRef": "[variables('passwordSecretName')]"
                      }],
                      "ingress": {
                          "external": true,
                          "targetPort": 80,
                          "allowInsecure": false,
                          "traffic": [
                              {
                                  "latestRevision": true,
                                  "weight": 100
                              }
                              // {
                              //     "revisionName": "httpcontainerapp--rv1",
                              //     "weight": 90
                              // },
                              // {
                              //     "revisionName": "httpcontainerapp--rv2",
                              //     "weight": 10  
                              // }                            
                          ]
                      }
                  },
                  "template": {
                      "revisionSuffix": "[parameters('revisionSuffix')]",
                      "containers": [
                          {
                              "name": "blob-container",
                              "image": "[parameters('imageName')]",                            
                              "env": [
                                  {
                                      "name": "AzureWebJobsStorage",
                                      "secretRef": "[variables('storageSecretName')]"
                                  }                                
                              ],
                              "resources": {
                                  "cpu": 0.5,
                                  "memory": "1Gi"
                              }
                          }
                      ],                    
                      "scale": {
                          "minReplicas": 1,
                          "maxReplicas": 10,
                          "rules": [
                          {
                              "name": "http-scaling",
                              "http": {
                                  "metadata": {
                                      "concurrentRequests": "100"                                
                                  }
                              }
                          }]
                      }
                  }
              }
          }
      ]
  }
  ```

  - Traffic Splitting is handled by *traffic* section of the template

    ```json
    "traffic": [
          {
            "latestRevision": true,
            "weight": 100
          }
          // {
          //     "revisionName": "httpcontainerapp--rv1",
          //     "weight": 90
          // },
          // {
          //     "revisionName": "httpcontainerapp--rv2",
          //     "weight": 10  
          // }                            
    ]
    ```

  

  ## Connecting the Dots...

  - Build a connected Microservice example with *Azure Function*, *Logic App*
    - Each Application to be deployed as a Container App to provide an end to end Serverless experience
    - Complete abstraction of *Infrastructure* and *Orchestration* of the underlying resources
    - Expose these apps with *Internal Ingress* for blocking public access
    - Inject all apps into a Virtual Network (*Secured Environment*) providing complete isolation
  - Integrate with Azure APIM to provide a *Gateway* to service to the backend Containerized APIs
    - Create an APIM instance on Azure with a [Self-hosted Gateway](https://docs.microsoft.com/en-us/azure/api-management/self-hosted-gateway-overview)
    - Deploy APIM as a docker container with *Container App* and in the same *Secured Environment* as above
    - Place all Internal Container Apps (*as deployed above*) as backend for the APIM
    - Expose the APIM Container App with *External Ingress* thus making it the only public facing endpoint for the entire system
      - APIM Container App (*Self-hosted Gateway*) would be able to call the internal Container Apps since being part of the same Secured Environment

  ![apim-container-app](./Assets/apim-container-app.png)

  

  ### Step-by-Step

  #### Logic App in a Container

  - Let us first Create and Deploy a Logic app as Docker Container

  - Logic App runs an Azure Function locally and hence few tools/extensions need to be installed

    ##### Pre-Requisites

    - Azure Function Core Tools - [v3.x](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=v3%2Cwindows%2Ccsharp%2Cportal%2Cbash)
      - The abobve link is for macOS; please install the appropriate links in the same page for other Operating Systems
      - At the time of writing, Core tools 3.x only supports the *Logic App Designer* within Visual Studio Code
      - The current example has been tested with - Function Core Tools version **3.0.3904** on a *Windows box*
    - [Docker Desktop for Windows](https://hub.docker.com/editions/community/docker-ce-desktop-windows)
    - A **Storage Account** on Azure - which is needed by any Azure function App
      - Logic App (*aka Azure Function*) would use this storage to cache its state
    - VS Code Extension for [Standard Logic App](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurelogicapps#:~:text=Azure%20Logic%20Apps%20for%20Visual,Apps%20directly%20from%20VS%20Code.)
    - VS Code Extension for [Azure Function](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions) 
    - VS Code extension for [Docker](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-docker)
      - This is Optional but recommended; it makes life easy while dealing with *Dockerfile* and *Docker CLI* commands

  - Create a Local folder to host all files related Logic App - viz. *LogicContainerApp*

  - Open the folder in VS Code

  - Create a *New Logic App Project* in this Folder 

    - Choose *Stateful* workflow in the process and name accordingly - viz. *httperesflow*

    - This generates all necessary files and sub-folders within the current folder

      - A folder named *httpresflow* is also added which contains the workflow.json file

      - This describes the Logic App Actions/triggers

      - This example uses a Http Request/Response type Logic App for simplicity

      - The Logic App would accept a Post body as below and would return back the same as response

        ```json
        {
            "Zip": "testzip-2011.zip"
        }
        ```

        

      ![logicapp-folder-structure](./Assets/logicapp-folder-structure.png)

      - Right click on the *workflow.json* file and Open the *Logic App Designer* - *this might take few seconds to launch*

      - Add Http Request trigger

        ![logicapp-designer-request](./Assets/logicapp-designer-request.png)

      - Add Http Respoinse Action

        ![logicapp-designer-response](./Assets/logicapp-designer-response.png)

        ![logicapp-designer-httpresflow](./Assets/logicapp-designer-httpresflow.png)

      - Save the Designer changes

      - Right click on the empty area on the workspace folder structure and Open the Context menu

        - Select the menu options that says - *Convert to Nuget-based Logic App project*

          ![logicapp-nuget-menu](./Assets/logicapp-nuget-menu.png)

          - This would generate .NET specific files - along with a *LogicContainerApp.csproj* file

        - Open the **local.settings.json** file

          - Replace the value of AzureWebJobsStorage variable with the value from *Storage Account Connection string* created earlier

        - Add a **Dockerfile** in the workspace

          ```bash
          FROM mcr.microsoft.com/azure-functions/node:3.0
          
          ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
               AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
               FUNCTIONS_V2_COMPATIBILITY_MODE=true \     
               AzureWebJobsStorage='' \
               AZURE_FUNCTIONS_ENVIRONMENT=Development \
               WEBSITE_HOSTNAME=localhost \
               WEBSITE_SITE_NAME=logiccontainerapp
          
          COPY ./bin/Debug/netcoreapp3.1 /home/site/wwwroot
          ```

          - **WEBSITE_SITE_NAME** - this is the name by which entries are created in Storage Account by the Logic App while caching its state

        - **Build** docker image

          ```bash
        docker build -t <repo_name>/<image_name>:<tag> .
          ```
        
        - **Create** the Logic App Container

          ```bash
        docker run --name logiccontainerapp -e AzureWebJobsStorage=$azureWebJobsStorage -d -p 8080:80 <repo_name>/<image_name>:<tag>
          ```
        
        - Let us now **Run** the logic app locally as a Docker container

          - Open the Storage account created earlier

            - Open the *Containers*

            - Open *azure-webjobs-secrets* blob

              ![logicapp-webjobs-secrets-1](./Assets/logicapp-webjobs-secrets-1.png)

              ![logicapp-webjobs-secrets-1](./Assets/logicapp-webjobs-secrets-2.png)

              ![logicapp-webjobs-secrets-1](./Assets/logicapp-webjobs-secrets-3.png)

              

            - Get the value of the **master** key in the **host.json** file

              ![logicapp-host-json](/Users/monojitdattams/Development/Projects/Workshops/AKSWorkshop/ContainerApps/Assets/logicapp-host-json.png)

              

          - Open *POSTMAN* or any Rest client of choice like **curl**

            ```bash
            http://localhost:8080/runtime/webhooks/workflow/api/management/workflows/httpresflow/triggers/manual/listCallbackUrl?api-version=2020-05-01-preview&code=<master_key_value_from_storage_account>
            ```
        
            - This would return the Post callback Url for Http triggered Logic App

              ```json
            {
                  "value": "https://localhost:443/api/httpresflow/triggers/manual/invoke?api-version=2020-05-01-preview&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=<value>",
                  "method": "POST",
                  "basePath": "https://localhost/api/httpresflow/triggers/manual/invoke",
                  "queries": {
                      "api-version": "2020-05-01-preview",
                      "sp": "/triggers/manual/run",
                      "sv": "1.0",
                      "sig": "<value>"
                  }
              }
              ```
        
            - Copy the value of the **value** parameter from the json response

          - Make following Http call

            ```bash
            http://localhost:8080/api/httpresflow/triggers/manual/invoke?api-version=2020-05-01-preview&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=<value>
            ```
        
          - Post Body

            ```json
            {
                "Zip": "testzip-2011.zip"
            }
            ```
        
          - Check the response coming back from Logic App as below

            ```json
          {
                "Zip": "testzip-2011.zip"
            }
            ```
          
            
        
        #### Setup Azure Container App

        - Create *Virtual Network* to inject Container Apps

          ```bash
          containerAppVnetId=$(az network vnet show -n $containerAppVnetName --resource-group $resourceGroup --query="id" -o tsv)
          
          controlPlaneSubnetId=$(az network vnet subnet show -n $controlPlaneSubnetName --vnet-name $containerAppVnetName --resource-group $resourceGroup --query="id" -o tsv)
          
          appsSubnetId=$(az network vnet subnet show -n $appsSubnetName --vnet-name $containerAppVnetName --resource-group $resourceGroup --query="id" -o tsv)
          
          ```
        
        
        
      - Create a *Secured Environment* for Azure Container Apps with this *Virtual Network*
        
          ```bash
          az containerapp env create --name $securedEnvironment --resource-group $resourceGroup \
            --logs-workspace-id $logWorkspaceId --logs-workspace-key $logWorkspaceSecret --location $location \
            --controlplane-subnet-resource-id $controlPlaneSubnetId \
            --app-subnet-resource-id $appsSubnetId
          ```
        
        
        
      #### Logic App as Azure Container App
        
      - Let us now deploy the logic app container onto Azure as Container App
        
      - Push Logic App container image to *Azure Container Registry*
        
          ```bash
          # If Container image is already created and tested, use Docker CLI
          docker push <repo_name>/<image_name>:<tag>
            
            OR
            
          # Use Azure CLI command for ACR to build and push
          az acr build -t <repo_name>/<image_name>:<tag> -r $acrName .
          ```
          
      - Create Azure Container App with this image
        
          ```bash
          logicappImageName="$registryServer/logiccontainerapp:v1.0.0"
                azureWebJobsStorage="<storage_account_connection_string"
                
          az containerapp create --name logicontainerapp --resource-group $resourceGroup \
              --image $logicappImageName --environment $securedEnvironment \
              --registry-login-server $registryServer --registry-username $registryUserName \
              --registry-password $registryPassword \
              --ingress external --target-port 80 --transport http \
              --secrets azurewebjobsstorage=$azureWebJobsStorage \
              --environment-variables "AzureWebJobsStorage=secretref:azurewebjobsstorage"
          ```
          
      - Note down the Logic App ingress url
        
        ![httplogic-container-overview](./Assets/httplogic-container-overview.png)
          
        ![logic-container-ingress](./Assets/logic-container-ingress.png)
          
          ![httplogic-container-secrets](./Assets/httplogic-container-secrets.png)
    

    

    
  #### Deploy an Azure Function App as Container App
    
  This function will be triggerred by a http Post call
    
  - This is going to invoke Logic App internally
    
  - Return the response back to the caller
    
  - Before we Deploy the function app, let us look at its code
    
    
          
    ```c#
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    namespace HttpContainerApps
      {
          public static class HttpContainerApps
          {
              [FunctionName("container")]
              public static async Task<IActionResult> Run(
                  [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
                  ILogger log)
              {
                  log.LogInformation("C# HTTP trigger function processed a request.");
      
                  var name = req.Query["name"];
                  var cl = new HttpClient();
      
                  var uri = $"http://httpcontainerapp-secured.internal.greensea-4ecd9ebc.eastus.azurecontainerapps.io/api/container?name={name}";
                  var res = await cl.GetAsync(uri);
                  var response = await res.Content.ReadAsStringAsync();
                  log.LogInformation($"Status:{res.StatusCode}");
                  log.LogInformation($"Response:{response}-v1.0.4");
                  response = $"Hello, {response}-v1.0.4";
                  // var response = $"Secured, {name}-v1.0.3";
                  return new OkObjectResult(response);
              }
          }
      }      
    ```
  - Deploy Azure Function app as Container App
    
    ```bash
     
    httpImageName="$registryServer/httplogiccontainerapp:v1.0.5" logicAppCallbackUrl="https://<logicontainerapp_internal_ingress_url>/runtime/webhooks/workflow/api/management/workflows/httpresflow/triggers/manual/listCallbackUrl?api-version=2020-05-01-preview&code=<master_key_value_from_storage_account>"
      
      logicAppPostUrl="https://<logicontainerapp_internal_ingress_url>/api/httpresflow/triggers/manual/invoke?api-version=2020-05-01-preview&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig={0}"
      
      az containerapp create --name httplogiccontainerapp --resource-group $resourceGroup \
        --image $httpImageName --environment $securedEnvironment \
        --registry-login-server $registryServer --registry-username $registryUserName \
        --registry-password $registryPassword \
        --ingress internal --target-port 80 --transport http \
        --secrets azurewebjobsstorage=$azureWebJobsStorage,logicappcallbackurl=$logicAppCallbackUrl,logicappposturl=$logicAppPostUrl \
        --environment-variables "AzureWebJobsStorage=secretref:azurewebjobsstorage,LOGICAPP_CALLBACK_URL=secretref:logicappcallbackurl,LOGICAPP_POST_URL=secretref:logicappposturl"
    ```
    
    - This Container App is with Ingress type **Internal** so this would be at exposed publicly      

    

    #### Deploy APIM as Container App

    - Select gateway option in APIM in the Azure Portal

        ![apim-gateway-1](./Assets/apim-gateway-1.png)

    - Get the *Endpoint Url* and *Auth Token* from the portal

      ![apim-gateway-2](./Assets/apim-gateway-2.png)

    - Define ARM template for APIM Container App

    ```json
  {
              "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
              "contentVersion": "1.0.0.0",
              "parameters": {
                  "containerappName": {
                      "defaultValue": "apimcontainerapp",
                      "type": "String"
                  },
                  "location": {
                      "defaultValue": "eastus",
                      "type": "String"
                  },
                  "environmentName": {
                      "defaultValue": "secure-env",
                      "type": "String"
                  },
                  "serviceEndpoint": {
                      "defaultValue": "",
                      "type": "String"
                  },
                  "serviceAuth": {
                      "defaultValue": "",
                      "type": "String"
                  }
              },
              "variables": {},
              "resources": [
                  {
                      "apiVersion": "2021-03-01",
                      "type": "Microsoft.Web/containerApps",
                      "name": "[parameters('containerappName')]",
                      "location": "[parameters('location')]",
                      "properties": {
                          "kubeEnvironmentId": "[resourceId('Microsoft.Web/kubeEnvironments', parameters('environmentName'))]",
                          "configuration": {                  
                              "ingress": {
                                  "external": true,
                                  "targetPort": 8080,
                                  "allowInsecure": false,
                                  "traffic": [
                                      {
                                          "latestRevision": true,
                                          "weight": 100
                                      }
                                  ]
                              }
                          },
                          "template": {
                              // "revisionSuffix": "revapim",
                              "containers": [
                                  {
                                      "name": "conainerapp-apim-gateway",
                                      "image": "mcr.microsoft.com/azure-api-management/gateway:latest",                            
                                      "env": [
                                          {
                                              "name": "config.service.endpoint",
                                              "value": "[parameters('serviceEndpoint')]"
                                          },
                                          {
                                              "name": "config.service.auth",
                                              "value": "[parameters('serviceAuth')]"
                                          }
                                      ],
                                      "resources": {
                                          "cpu": 0.5,
                                          "memory": "1Gi"
                                      }
                                  }
                              ],
                              "scale": {
                                  "minReplicas": 1,
                                  "maxReplicas": 3
                              }
                          }
                      }
                  }
              ]
          }
    ```
    - Deploy APIM as Container App
    
    ```bash
    apimappImageName="mcr.microsoft.com/azure-api-management/gateway:latest"
  serviceEndpoint="<service_Endpoint>"
    serviceAuth="<service_Auth>"
    
    az deployment group create -f ./api-deploy.json -g $resourceGroup \
      --parameters serviceEndpoint=$serviceEndpoint serviceAuth=$serviceAuth
    ```
    - Add Container Apps as APIM back end
    
      ![apim-api-main](./Assets/apim-api-1.png)
    
        ![apim-api-main](./Assets/apim-api-2.png)

        ![apim-api-main](./Assets/apim-api-3.png)

      - The Web Service URL would be the *Internal Ingress* url of the *Http Container App*

    
    - This would call the *Logic Containr App* internaly and retun back teh response
    
       ![apim-container-app](./Assets/apim-container-app.png)
    

  

  ## References

  - [Azure Container Apps](https://docs.microsoft.com/en-us/azure/container-apps/overview)					
  - [Logic App Standard](https://docs.microsoft.com/en-us/azure/logic-apps/single-tenant-overview-compare)
  - Azure APIM [Self-hosted Gateway](https://docs.microsoft.com/en-us/azure/api-management/self-hosted-gateway-overview)
  - [Source Repo](https://github.com/monojit18/ContainerApps.git)	

