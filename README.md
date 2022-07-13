| [master](https://github.com/Kontent-ai-Learn/kontent-ai-learn-github-reader/tree/master) | [develop](https://github.com/Kontent-ai-Learn/kontent-ai-learn-github-reader/tree/develop) |
|:---:|:---:|
| ![master](https://github.com/Kontent-ai-Learn/kontent-ai-learn-github-reader/actions/workflows/master_kcd-github-service-live-master.yml/badge.svg) | ![develop](https://github.com/Kontent-ai-Learn/kontent-ai-learn-github-reader/actions/workflows/develop_kcd-github-service-live-dev.yml/badge.svg) |

# Kontent.ai Learn - GitHub Reader

Backend service for [Kontent.ai Learn](https://kontent.ai/learn/).

Together with [Samples Manager](https://github.com/Kontent-ai-Learn/kontent-ai-learn-samples-manager), this service is responsible for maintaining Kontent.ai content items that represent code samples used in Kontent.ai documentation portal.

Github Reader responds to commits made to the [kontent-docs-samples](https://github.com/Kontent-ai-Learn/kontent-ai-learn-samples) repository, which contains all the code samples that are used by the documentation portal.

## Overview

1. This project is a C# Azure Functions application.
2. It receives a webhook with names of changed files after each commit to GitHub [repository](https://github.com/Kontent-ai-Learn/kontent-ai-learn-samples).
3. After receiving a webhook, the service fetches content of each file from GitHub, extracts the marked code samples and stores them in its own local storage.
4. Finally, it stores the code samples in the [Azure Blob Storage](https://azure.microsoft.com/en-us/services/storage/blobs/), where the [Samples Manager](https://github.com/Kontent-ai-Learn/kontent-ai-learn-samples-manager) can access them.

## Setup

### Prerequisites

1. Visual Studio 2017 with [Azure Functions and Web Jobs Tools](https://marketplace.visualstudio.com/items?itemName=VisualStudioWebandAzureTools.AzureFunctionsandWebJobsTools) installed
2. Subscriptions on MS Azure and GitHub

### Instructions

1. Clone the project repository and open it in Visual Studio.
2. Install all the necessary nugget packages.
3. Set the required keys.
4. Run the service locally in Visual Studio, or
5. Deploy the service to a new Azure Functions App instance in your Azure subscription.

#### Required Keys

* `Github.AccessToken` - GitHub account personal access token
* `Github.RepositoryName` - Name of GitHub repository with code samples
* `Github.RepositoryOwner` - Owner of GitHub repository with code samples
* `Repository.ConnectionString` - Connection string for the Azure Storage account

## Testing

Unit tests are located in `GithubService.Services.Tests` folder.

## How To Contribute

Feel free to open a new issue where you describe your proposed changes, or even create a new pull request from your branch with proposed changes.

## License

All the source codes are published under MIT license.
