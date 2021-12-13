using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;

namespace sas.api
{
    public static class ListStorageAccounts
    {
        [FunctionName("ListStorageAccounts")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req, ILogger log)
        {
            var dlsa = Environment.GetEnvironmentVariable("DATALAKE_STORAGE_ACCOUNTS");
            log.LogInformation($"Storage accounts from configuration: {dlsa}");
            if (dlsa is null) {
                dlsa = "Error: DATALAKE_STORAGE_ACCOUNTS is not set in application config. For local development use: local.settings.json";
                return new BadRequestObjectResult(dlsa);
            }

            var accounts = dlsa.Replace(',',';').Split(';');

            return new OkObjectResult(accounts);
        }
    }
}
