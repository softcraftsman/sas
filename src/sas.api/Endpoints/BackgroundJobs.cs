using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Azure.Identity;
using sas.api.Services;

namespace sas.api
{
    public static class BackgroundJobs
    {
        [FunctionName("CalculateAllFolderSizes")]
        public static async Task<IActionResult> CalculateAllFolderSizes(
            [HttpTrigger(AuthorizationLevel.Function, "POST", Route = "Configuration/CalculateFolderSizes")]
            HttpRequest req, ILogger log)
        {
            var configResult = SasConfiguration.GetConfiguration();
            var sb = new System.Text.StringBuilder();
            foreach (var account in configResult.StorageAccounts)
            {
                var serviceUri = new Uri($"https://{account}.dfs.core.windows.net");
                var serviceCLient = CreateDlsClientForUri(serviceUri);
                var fileSystems = serviceCLient.GetFileSystems();

                var msg = $"Analyzing {account}";
                log.LogInformation(msg);
                sb.AppendLine(msg);

                foreach(var filesystem in fileSystems)
                {
                    var containerUri = new Uri($"https://{account}.dfs.core.windows.net/{filesystem.Name}");
                    var containerClient = CreateDlsClientForUri(serviceUri);
                    var fileSystemClient = containerClient.GetFileSystemClient(filesystem.Name);
                    var folders = fileSystemClient.GetPaths().Where<PathItem>( 
                        pi => pi.IsDirectory == null ? false: (bool) pi.IsDirectory);

                    var folderOperations = new FolderOperations(serviceUri, filesystem.Name, log);

                    long size = 0;
                    foreach( var folder in folders) {
                        size += await folderOperations.CalculateFolderSize(folder.Name);
                    }
                    
                    msg = $"  {filesystem.Name} aggregate size {size} bytes";
                    log.LogInformation(msg);
                    sb.AppendLine(msg);
                }
            }

            return new OkObjectResult(sb.ToString());
        }

        private static DataLakeServiceClient CreateDlsClientForUri(Uri containerUri)
        {
            var tenantId = Environment.GetEnvironmentVariable("TENANT_ID");
            var clientId = Environment.GetEnvironmentVariable("APP_REGISTRATION_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
            var tokenCred = new ClientSecretCredential(tenantId, clientId, clientSecret);
            var dlsClient = new DataLakeServiceClient(containerUri, tokenCred);
            return dlsClient;
        }
    }
}
