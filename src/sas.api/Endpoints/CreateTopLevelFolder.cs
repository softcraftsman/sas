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
using sas.api;
using System.Text;
using Microsoft.Identity.Client;

namespace sas.api
{
    public static class CreateTopLevelFolder
    {
        [FunctionName("CreateTopLevelFolder")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            // GET - Send Instructions back to calling client
            if (req.Method == HttpMethods.Get) {
                log.LogInformation("CreateTopLevelFolder GET method called.");
                var tlfp = JsonConvert.SerializeObject(new TopLevelFolderParameters());
                var help = $"Use a POST method with the body containing the properties of {nameof(TopLevelFolderParameters)}\n{tlfp}";
                return new OkObjectResult(help);
            }

            // POST - Method
            if (req.Method == HttpMethods.Post)
            {
                //Extracting body object from the call and deserializing it.
                var tlfp = GetTopLevelFolderParameters(req, out string error);
                if (error != null)
                {
                    log.LogInformation(error);
                    return new BadRequestObjectResult(error);
                }

                // Get object ID from UPN
                if (tlfp.FolderOwner.Contains('@'))
                {
                    var apiAccessToken = await UserOperations.GetApiToken(req);
                    tlfp.FolderOwner = await UserOperations.GetObjectIdFromUPN(apiAccessToken, tlfp.FolderOwner);
                }

                // Call each of the steps in order and error out if anytyhing fails
                var containerUri = new Uri($"https://{tlfp.StorageAcount}.dfs.core.windows.net/{tlfp.Container}");
                var accessToken = UserOperations.GetAccessTokenFromRequest(req);

                var adlOps = new ADLSOperations(containerUri, accessToken, log);
                if (adlOps.AddsFolderOwnerToContainerACLAsExecute(tlfp.FolderOwner, tlfp.Container, true, tlfp.Container, out error)
                    && adlOps.CreatesNewFolder(tlfp.Folder, tlfp.Container, out error)
                    && adlOps.AssignsRWXToFolderOwner(tlfp.FolderOwner, tlfp.Container, tlfp.Folder, out error)
                    && adlOps.SavesFundCodeIntoContainerMetadata(tlfp.FundCode, tlfp.Container, tlfp.Folder, out error))
                {
                    log.LogInformation("success");
                }
                else
                {
                    log.LogError("Error: " + error);
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                log.LogInformation("Workflow completed successfully.");
                return new OkResult();
            }

            log.LogInformation("Internal Server Error - 500. Only GET and POST are acceptable.");
            return new BadRequestResult();
        }

        internal static TopLevelFolderParameters GetTopLevelFolderParameters(HttpRequest req, out string error)
        {
            string body = string.Empty;
            error = null;

            using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8))
            {
                body = reader.ReadToEnd();
            }

            var bodyDeserialized = JsonConvert.DeserializeObject<TopLevelFolderParameters>(body);

            //Performs call's body validation
            if (bodyDeserialized is null)
            {
                error = $"{nameof(TopLevelFolderParameters)} is missing.";
            }
            else
            {
                if (bodyDeserialized.Container == null || bodyDeserialized.Folder == null ||
                    bodyDeserialized.FolderOwner == null || bodyDeserialized.FundCode == null ||
                    bodyDeserialized.StorageAcount == null)
                {
                    error = $"{nameof(TopLevelFolderParameters)} is malformed.";
                }
            }

            return bodyDeserialized;
        }

    }
}