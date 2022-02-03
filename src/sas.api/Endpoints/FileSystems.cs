using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using sas.api.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace sas.api
{
    public static class FileSystems
    {
        [FunctionName("FileSystems")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "POST", "GET", Route = "FileSystems/{account}")]
            HttpRequest req, ILogger log, String account)
        {
            if (req.Method == HttpMethods.Post)
                return await FileSystemsPOST(req, log, account);

            if (req.Method == HttpMethods.Get)
                return FileSystemsGET(req, log, account);

            return null;
        }

        private static IActionResult FileSystemsGET(HttpRequest req, ILogger log, string account)
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

            // Calculate UPN
            var upn = claimsPrincipal.Identity.Name.ToLowerInvariant();

            // Get the Containers for a upn from each storage account
            var accounts = SasConfiguration.GetConfiguration().StorageAccounts;
            if (account != null) accounts = accounts.Where(a => a.ToLowerInvariant() == upn).ToArray();

            var result = new List<FileSystemResult>();
            foreach (var acct in accounts)
            {
                // TODO: Centralize this to account for other clouds
                var serviceUri = new Uri($"https://{acct}.dfs.core.windows.net");
                var adls = new FileSystemOperations(serviceUri, log);

                List<string> containers;
                try
                {
                    containers = adls.GetContainersForUpn(upn).ToList();
                }
                catch (Exception ex)
                {
                    log.LogError(ex, ex.Message);
                    containers = new List<string>() { ex.Message };
                }
                result.Add(new FileSystemResult() { Name = account, FileSystems = containers });
            }

            //
            return new OkObjectResult(result);
        }

        private static async Task<IActionResult> FileSystemsPOST(HttpRequest req, ILogger log, string account)
        {
            //Extracting body object from the call and deserializing it.
            var tlfp = await GetFileSystemParameters(req, log);
            if (tlfp == null)
                return new BadRequestErrorMessageResult($"{nameof(FileSystemParameters)} is missing.");

            // Add Route Parameters
            tlfp.StorageAcount ??= account;

            // Check Parameters
            string error = null;
            if (Extensions.AnyNull(tlfp.FileSystem, tlfp.Owner, tlfp.FundCode, tlfp.StorageAcount))
                error = $"{nameof(FileSystemParameters)} is malformed.";

            // Call each of the steps in order and error out if anytyhing fails
            var storageUri = new Uri($"https://{tlfp.StorageAcount}.dfs.core.windows.net");
            var fileSystemOperations = new FileSystemOperations(storageUri, log);

            // Create File System
            Result result = null;
            result = await fileSystemOperations.CreateFileSystem(tlfp.FileSystem, tlfp.Owner, tlfp.FundCode);
            if (!result.Success)
                return new BadRequestErrorMessageResult(result.Message);

            // Get Blob Owner
            var ownerId = await UserOperations.GetObjectIdFromUPN(tlfp.Owner);

            // Add Blob Owner
            var roleOperations = new RoleOperations(log);
            roleOperations.AssignRoles(tlfp.StorageAcount, tlfp.FileSystem, ownerId);

            // Get Root Folder Details
            var folderOperations = new FolderOperations(storageUri, tlfp.FileSystem, log);
            var folderDetail = folderOperations.GetFolderDetail("/");

            return new OkObjectResult(folderDetail);
        }

        internal static async Task<FileSystemParameters> GetFileSystemParameters(HttpRequest req, ILogger log)
        {
            string body = string.Empty;
            using (var reader = new StreamReader(req.Body, Encoding.UTF8))
            {
                body = await reader.ReadToEndAsync();
                if (string.IsNullOrEmpty(body))
                {
                    log.LogError("Body was empty coming from ReadToEndAsync");
                }
            }
            var bodyDeserialized = JsonConvert.DeserializeObject<FileSystemParameters>(body);
            return bodyDeserialized;
        }

        private class FileSystemResult
        {
            //[{name: 'adlstorageaccountname', fileSystems: [{name: 'file system name'}]]
            public string Name { get; set; }
            public List<string> FileSystems { get; set; }
        }

        internal class FileSystemParameters
        {
            public string StorageAcount { get; set; }

            public string FileSystem { get; set; }

            public string FundCode { get; set; }

            public string Owner { get; set; }        // Probably will not stay as a string
        }
    }
}
