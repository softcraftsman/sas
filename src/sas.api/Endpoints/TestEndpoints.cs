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

namespace sas.api
{
    public static class TestEndpoints
    {
        [FunctionName("ConvertUPNtoObject")]
        public static async Task<IActionResult> ConvertUPNtoObject(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            
            // Get Caller Access Token
            var accessToken = req.Headers.First(x => x.Key == "Authorization").Value.First().Split(' ').LastOrDefault();

            // Get Object ID
            var objId = await UserOperations.GetObjectIdFromUPN(accessToken, "fabricio.sanchez@americasuniversity.net");


            return new OkObjectResult(objId);
        }

        [FunctionName("CalculateFolderSize")]
        public static IActionResult CalculateFolderSize(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            //Extracting body object from the call and deserializing it.
            var tlfp = CreateTopLevelFolder.GetTopLevelFolderParameters(req, out string error);
            if (error != null)
            {
                log.LogInformation(error);
                return new BadRequestObjectResult(error);
            }

            var containerUri = new Uri($"https://{tlfp.StorageAcount}.dfs.core.windows.net/{tlfp.Container}");
            var adls = new ADLSOperations(containerUri, null, log);
            var size = adls.CalculateFolderSize(tlfp.Folder);

            return new OkObjectResult($" {containerUri}/{tlfp.Folder} size is {size} KB");
        }
    }
}
