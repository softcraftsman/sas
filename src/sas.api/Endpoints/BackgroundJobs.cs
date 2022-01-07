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

namespace sas.api
{
    public static class BackgroundJobs
    {
        /**
            <summary>Weekly Timer Method to calculate all folder sizes.
            </summary>
            <example>
                Manually run by calling
                  POST https://{host}/api/CalculateAllFolderSizes
                  Header - x-functions-key = to the function key
                  Content-Type = application/json
                  body set as raw = { }
            </example>
        **/
        [FunctionName("CalculateAllFolderSizes")]
        public static void CalculateAllFolderSizes(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req, ILogger log)
        {
            var configResult = SasConfiguration.GetConfiguration();

            foreach (var account in configResult.StorageAccounts)
            {
                var serviceUri = new Uri($"https://{account}.dfs.core.windows.net");
                var serviceCLient = CreateDlsClientForUri(serviceUri);
                var fileSystems = serviceCLient.GetFileSystems();

                log.LogInformation($"Analyzing {account}");

                foreach(var filesystem in fileSystems)
                {
                    var containerUri = new Uri($"https://{account}.dfs.core.windows.net/{filesystem.Name}");
                    var containerClient = CreateDlsClientForUri(serviceUri);
                    var fileSystemClient = containerClient.GetFileSystemClient(filesystem.Name);
                    var folders = fileSystemClient.GetPaths().Where<PathItem>( 
                        pi => pi.IsDirectory == null ? false: (bool) pi.IsDirectory);
                    
                    var adls = new ADLSOperations(containerUri,null, log);
                    
                    long size = 0;
                    foreach( var folder in folders) {
                        size += adls.CalculateFolderSize(folder.Name);
                    }
                    log.LogInformation($"  {filesystem} aggregate size {size} KB");
                }
            }
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
