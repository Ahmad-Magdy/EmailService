# Email MicroService
It's a small service to process emails coming from all kinds of clients using Microsoft Storage Queue and Azure functions.

### Why Queue and Azure Function?
The combination between Queue and Function will allow the service to scale up and down smoothly and fast, the integration contains a retry mechanism and can handle the fault.

### Email Services Implementation
The service implements 4 kinds of Email Providers:
1. SendGrid
2. AWS SES
3. MailGun
4. Fake Service, which is a fake implementation of the service for testing.

## Integrations
The service is integrated with Sentry for Error Capturing. Connected with Humio for logging in addition to Azure App Insights support for metrics and logging too.


## How to run:
### 1. Setting environment variables
You need to set these environment variables

```json
{
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "EmailServiceStorageCS": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "humio__token": "YOUR_TOKEN",
    "humio__ingesturl": "https://cloud.humio.com/api/v1/ingest/elastic-bulk",
    "sentry__dsn": "Your DSN",
    "emailproviders__sendgrid__apikey": "APIKEY",
    "emailproviders__mailgun__apikey": "API_KEY",
    "emailproviders__mailgun__domain": "yourdomain.mailgun.org",
    "emailproviders__mailgun__baseurl": "https://api.mailgun.net",
    "emailproviders__amazonses__keyid": "APIKEY",
    "emailproviders__amazonses__keysecret": "SECRET"
  }
```

### 2. Azure Functions Core Tools
You can get it from here [https://github.com/Azure/azure-functions-core-tools](https://github.com/Azure/azure-functions-core-tools). 
I used the runtime v2 which is the latest stable version. V3 still in preview and there is no guarantee that it will give the same behavior.


### 3. Storage 
You can create a storage account in Azure and set the connection string as an environment variable or you can run a storage emulator.
I recommend using [Azurite Emulator](https://github.com/Azure/Azurite), you need to have docker.
Use this command to run the storage with Blob and Queue
```shell
docker run --rm -it -p 10000:10000 -p 10001:10001 mcr.microsoft.com/azure-storage/azurite azurite --blobPort 10000 --blobHost 0.0.0.0 --queuePort 10001 --queueHost 0.0.0.0 --loose
```


## Insights 
When deploying The function in azure and connecting it with Azure Monitor(App Insights), you get a lot of metrics and stats out of the box, because there is a deeper connection between the function runtime, azure storage, and the function.
Examples
![Screenshot 2020-01-09 at 11 45 44 PM](https://user-images.githubusercontent.com/10447926/72111139-38a9e700-333a-11ea-900b-74357e51b42d.png)
![Screenshot 2020-01-09 at 11 47 16 PM](https://user-images.githubusercontent.com/10447926/72111550-65123300-333b-11ea-92ce-09318661ce54.png)
![Screenshot 2020-01-09 at 11 49 43 PM](https://user-images.githubusercontent.com/10447926/72111551-65123300-333b-11ea-9f02-5130e37d5b9a.png)
![Screenshot 2020-01-09 at 11 50 18 PM](https://user-images.githubusercontent.com/10447926/72111552-65aac980-333b-11ea-8c96-ce8e066af672.png)
![Screenshot 2020-01-09 at 11 56 02 PM](https://user-images.githubusercontent.com/10447926/72111612-9e4aa300-333b-11ea-846e-45edde2da58b.png)

**Note**
The metrics data can be viewed in Grafana with Azure Monitor plugin.
