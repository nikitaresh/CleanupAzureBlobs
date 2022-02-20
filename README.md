This repo contains Azure Timer Trigger function project to cleanup Azure Blob Storage.

To be able to build and publish this Function you need to install Visual Studio Azure Function Tools as described in [Develop Azure Functions using Visual Studio](https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-vs) tutorial.

Once build and published the project successfully you need to add `CONTAINER_NAME` and `BLOB_EXPIRATION_MINUTES` variables for the Function App in Azure Portal (Function App -> Configuration -> Application setting -> New application setting).