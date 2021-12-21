# SAS for EDU
SAS is a storage as a Service platform designed to automate storage allocation in EDU institutions. Its main goal is to provide agility to stakeholders on having access to object storage infrastructure.

# Deploy SAS for EDU
In order to deploy this solution to your environment, you'll need to setup some variable in the build process. To accomplish this, do the following:

* [Fork the code](#fork-the-code)
* [Create a Static Web App](#create-a-static-web-app)
* [Create an application](#Create-an-application)
* [Add secrets](#Add-secrets)

## Fork the code
Fork the code into your github repository. You can name the repo whatevery you like.

## Create a Static Web App
Create a Static Web App in the Azure Portal. Name it anything you like. Choose whichever plan you like at this time, though you'll probably need the Standard plan when you wish to apply your own domain name. ***Important***, when choosing the GitHub repo, choose your repo instead of the source one.

Copy the Static Web App URL for use later.
Copy the deployment token (Click on Manage deployment token) for use later.

Add the App Settings under the Static Web App using Settings -> Configuration. Add a new application setting called DATALAKE_STORAGE_ACCOUNTS. List the name of the storage accounts to use. Just the name is adequate. Separate the accounts by comma or semicolon.

![App Settings](./assets/app-settings.png)

## Create an application
In the Azure portal, go to the Azure Active Directory. Add a new App Registration.
* Provide an Application Name
* Choose the single tenant
* Redirect URI
    * Choose the Single-page application
    * Provide the Static Web App URL

Copy the Directory (tenant) ID for use later.
Copy the Application (client) ID for use later.

## Add secrets
The GitHub workflow has a few required secrets that need to be created to enable it properly. Create the following secrets by going Settings -> Secrets.

- SAS_DEPLOYMENT_TOKEN
- TENANT_ID
- CLIENT_ID

![App Settings](./assets/aad-settings.png)

## Build
Now that all of the pieces are present, go to Actions in GitHub and run the Azure SWA Deploy workflow (It should automatically run when code is committed as well).

[![Azure Static Web Apps CI/CD](../../sas/actions/workflows/azure-swa-deploy.yml/badge.svg)](../../actions/workflows/azure-swa-deploy.yml)
