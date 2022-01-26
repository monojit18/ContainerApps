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

- How to Setup Azure Container Apps using Azure CLI
- How to Deploy a containerized *Azure Function* as Azure Container App
- How to Deploy a containerized *Logic App* as Azure Container App
- Deploy the *Self-hosted Gateway* component of an APIM instance as a Container App itself
- Manage the two Container Apps using APIM Container App

## What are we going to build?