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
using sas.api.Services;

namespace sas.api
{
    public static class TestEndpoints
    {
        [FunctionName("TestConvertUPNtoObject")]
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

        [FunctionName("TestCalculateFolderSize")]
        public static IActionResult CalculateFolderSize(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            //Extracting body object from the call and deserializing it.
            var tlfp = TopLevelFolders.GetTopLevelFolderParameters(req, out string error);
            if (error != null)
            {
                log.LogInformation(error);
                return new BadRequestObjectResult(error);
            }

            var adls = new FolderOperations(new Uri($"https://{tlfp.StorageAcount}.dfs.core.windows.net"), tlfp.Container, log);
            var size = adls.CalculateFolderSize(tlfp.Folder);

            return new OkObjectResult($" {tlfp.Container}/{tlfp.Folder} size is {size} KB");
        }
    }
}
