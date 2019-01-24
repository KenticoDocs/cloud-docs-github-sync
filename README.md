[![Build Status](https://travis-ci.org/Kentico/kentico-cloud-docs-github-service.svg?branch=master)](https://travis-ci.org/Kentico/kentico-cloud-docs-github-service) [![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=kentico-cloud-docs-github-service&metric=alert_status)](https://sonarcloud.io/dashboard?id=kentico-cloud-docs-github-service)

# Kentico Cloud Documentation - Github Sync

Backend service for [Kentico Cloud](https://app.kenticocloud.com/) documentation portal.

The service is responsible for maintaining Kentico Cloud content items that represent code samples used in Kentico Cloud documentation portal.
It creates/updates content items in Kentico Cloud according to the commits made to the [kentico-cloud-docs-code-samples](https://github.com/Kentico/kentico-cloud-docs-samples) repository, which contains all the code samples that are used by the documentation portal.

## Service Overview
1. The service is written in C# and works as an Azure function that is deployed on Azure as well.
2. It receives a webhook with names of changed files after each commit to Github [repository](https://github.com/Kentico/kentico-cloud-docs-samples).
3. After receiving a webhook, the service fetches content of each file from Github, extracts the marked code samples and stores them in its own local storage.
4. Finally, it creates or modifies content items representing code samples in Kentico Cloud project using the [Kentico Cloud Content Management SDK](https://github.com/Kentico/content-management-sdk-net).

## Service Setup

### Prerequisites
1. Visual Studio 2017 with [Azure Functions and Web Jobs Tools](https://marketplace.visualstudio.com/items?itemName=VisualStudioWebandAzureTools.AzureFunctionsandWebJobsTools) installed
2. Subscriptions on MS Azure, Kentico Cloud and Github

### Instructions
1. Clone the project repository and open it in Visual Studio.
2. Install all the necessary nugget packages.
3. Set the required keys.
4. Run the service locally by running it in Visual Studio, or
5. Deploy the service to a new Azure App Service instance in your Azure subscription.

#### Required Keys
* `Github.AccessToken` - Github account personal access token
* `Github.RepositoryName` - Name of Github repository with code samples
* `Github.RepositoryOwner` - Owner of Github repository with code samples
* `KenticoCloud.ContentManagementApiKey` - Kentico Cloud Content Management API key
* `KenticoCloud.InternalApiKey` - Kentico Cloud internal API key (temporary - will be removed after [Automatic Publishing and Unpublishing via the CM API](https://kenticocloud.com/roadmap) feature gets released)
* `KenticoCloud.ProjectId` - Kentico Cloud project ID
* `Repository.ConnectionString` - Connection string for the local storage

## Testing
Unit tests are located in `GithubService.Services.Tests` folder.

## How To Contribute
Feel free to open a new issue where you describe your proposed changes, or even create a new pull request from your branch with proposed changes.

## Licence
All the source codes are published under MIT licence.
