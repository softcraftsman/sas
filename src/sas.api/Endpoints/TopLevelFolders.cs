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
using sas.api.Services;
using System.Linq;

namespace sas.api
{
    public static class TopLevelFolders
    {
        [FunctionName("TopLevelFolders")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            // GET - Send Instructions back to calling client
            if (req.Method == HttpMethods.Get)
            {
                // Find out user who is calling
                var tlfp = GetTopLevelFolderParameters(req, out string error);
                var storageUri = new Uri($"https://{tlfp.StorageAcount}.dfs.core.windows.net");
                var folderOperations = new FolderOperations(storageUri, tlfp.Container, log);
                var folders = folderOperations.GetAccessibleFolders(tlfp.FolderOwner);

                // Build a 
                return new OkObjectResult(folders);
            }

            // POST - Method
            if (req.Method == HttpMethods.Post)
            {
                return await CreateFolder(req, log);
            }

            log.LogInformation("Internal Server Error - 500. Only GET and POST are acceptable.");
            return new BadRequestResult();
        }

        private static IActionResult SendInstructions(ILogger log)
        {
            log.LogInformation("CreateTopLevelFolder GET method called.");
            var tlfp = JsonConvert.SerializeObject(new TopLevelFolderParameters());
            var help = $"Use a POST method with the body containing the properties of {nameof(TopLevelFolderParameters)}\n{tlfp}";
            return new OkObjectResult(help);
        }

        private static async Task<IActionResult> CreateFolder(HttpRequest req, ILogger log)
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
                var apiAccessToken = await UserOperations.GetApiToken(req, log);
                if (apiAccessToken != null)
                    tlfp.FolderOwner = await UserOperations.GetObjectIdFromUPN(apiAccessToken, tlfp.FolderOwner);
            }

            // Call each of the steps in order and error out if anytyhing fails
            var storageUri = new Uri($"https://{tlfp.StorageAcount}.dfs.core.windows.net");
            var fileSystemOperations = new FileSystemOperations(storageUri, log);
            var folderOperations = new FolderOperations(storageUri, tlfp.Container, log);

            if (fileSystemOperations.AddsFolderOwnerToContainerACLAsExecute(tlfp.Container, tlfp.FolderOwner, false, out error)
                && folderOperations.CreateNewFolder(tlfp.Folder, out error)
                && folderOperations.AddFundCodeToMetaData(tlfp.Folder, tlfp.FundCode, out error)
                && folderOperations.AssignFullRwx(tlfp.Folder, tlfp.FolderOwner, out error)
               )
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

        internal class TopLevelFolderParameters
        {
            public string StorageAcount { get; set; }

            public string Container { get; set; }

            public string Folder { get; set; }

            public string FundCode { get; set; }

            public string FolderOwner { get; set; }        // Probably will not stay as a string
        }
    }
}