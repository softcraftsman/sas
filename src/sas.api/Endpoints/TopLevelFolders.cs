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
using System.Security.Claims;
using System.Web.Http;
using System.Diagnostics;

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
                return await GetTopLevelFolders(req, log);
            }

            // POST - Method
            if (req.Method == HttpMethods.Post)
            {
                return await CreateFolder(req, log);
            }

            log.LogError("Internal Server Error - 500. Only GET and POST are acceptable.");
            return new BadRequestResult();
        }

        private static async Task<IActionResult> GetTopLevelFolders(HttpRequest req, ILogger log)
        {
            // Check for logged in user
            ClaimsPrincipal claimsPrincipal;
            try
            {
                claimsPrincipal = UserOperations.GetClaimsPrincipal(req);
                if (Extensions.AnyNull(claimsPrincipal, claimsPrincipal.Identity))
                    return new BadRequestErrorMessageResult("Call requires an authenticated user.");
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                return new BadRequestErrorMessageResult("Unable to authenticate user.");
            }

            // Find out user who is calling
            var tlfp = await GetTopLevelFolderParameters(req).ConfigureAwait(true);
            if (tlfp == null)
                return new BadRequestErrorMessageResult($"{nameof(TopLevelFolderParameters)} is missing.");
            var storageUri = new Uri($"https://{tlfp.StorageAcount}.dfs.core.windows.net");
            var folderOperations = new FolderOperations(storageUri, tlfp.Container, log);
            var folders = folderOperations.GetAccessibleFolders(tlfp.FolderOwner);
            return new OkObjectResult(folders);
        }

        private static IActionResult SendInstructions(ILogger log)
        {
            var tlfp = JsonConvert.SerializeObject(new TopLevelFolderParameters());
            var help = $"Use a POST method with the body containing the properties of {nameof(TopLevelFolderParameters)}\n{tlfp}";
            return new OkObjectResult(help);
        }

        private static async Task<IActionResult> CreateFolder(HttpRequest req, ILogger log)
        {
            //Extracting body object from the call and deserializing it.
            var tlfp = await GetTopLevelFolderParameters(req);
            if (tlfp == null)
                return new BadRequestErrorMessageResult($"{nameof(TopLevelFolderParameters)} is missing.");

            // Check Parameters
            string error = null;
            if (Extensions.AnyNull(tlfp.Container, tlfp.Folder, tlfp.FolderOwner, tlfp.FundCode, tlfp.StorageAcount))
                error = $"{nameof(TopLevelFolderParameters)} is malformed.";

            // Call each of the steps in order and error out if anytyhing fails
            var storageUri = new Uri($"https://{tlfp.StorageAcount}.dfs.core.windows.net");
            var fileSystemOperations = new FileSystemOperations(storageUri, log);
            var folderOperations = new FolderOperations(storageUri, tlfp.Container, log);

            Result result = null;
            result = await fileSystemOperations.AddsFolderOwnerToContainerACLAsExecute(tlfp.Container, tlfp.FolderOwner);
            if (!result.Success)
                return new BadRequestErrorMessageResult(result.Message);

            result = await folderOperations.CreateNewFolder(tlfp.Folder);
            if (!result.Success)
                return new BadRequestErrorMessageResult(result.Message);
            result = await folderOperations.AddFundCodeToMetaData(tlfp.Folder, tlfp.FundCode);
            if (!result.Success)
                return new BadRequestErrorMessageResult(result.Message);

            result = await folderOperations.AssignFullRwx(tlfp.Folder, tlfp.FolderOwner);
            if (!result.Success)
                return new BadRequestErrorMessageResult(result.Message);

            return new OkResult();
        }

        internal static async Task<TopLevelFolderParameters> GetTopLevelFolderParameters(HttpRequest req)
        {
            string body = string.Empty;
            if (req is null)
                throw new Exception("Request is null");
            using (StreamReader reader = new(req.Body, Encoding.UTF8))
            {
                body = await reader.ReadToEndAsync().ConfigureAwait(true);
                if (string.IsNullOrEmpty(body))
                    throw new Exception("Body was empty coming from ReadToEndAsync");
            }
            var bodyDeserialized = JsonConvert.DeserializeObject<TopLevelFolderParameters>(body);
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