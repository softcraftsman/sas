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
        [FunctionName("TestEndpoints")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            
            // Get Caller Access Token
            var accessToken = req.Headers.First(x => x.Key == "Authorization").Value.First().Split(' ').LastOrDefault();

            // Get Object ID
            var objId = await UserOperations.GetObjectIdFromUPN(accessToken, "fabricio.sanchez@americasuniversity.net");


            return new OkObjectResult(objId);
        }
    }
}
