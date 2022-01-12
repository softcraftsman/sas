using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Generic;
using System.Web.Http;
using System.Security.Claims;
using sas.api.Services;

namespace sas.api
{
    public static class FileSystems
    {
        [FunctionName("FileSystems")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "GET", Route = null)]
            HttpRequest req, ILogger log)
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
            var upn = claimsPrincipal.Identity.Name;

            // Dictionary for
            var accountContainers = new Dictionary<string, IList<string>>();

            // Get the Containers for a upn from each storage account
            var config = SasConfiguration.GetConfiguration();
            var result = new List<FileSystemResult>();
            foreach (var account in config.StorageAccounts)
            {
                var serviceUri = new Uri($"https://{account}.dfs.core.windows.net");
                var adls = new FileSystemOperations(serviceUri,log);
                var containers = adls.GetContainersForUpn(upn).ToList();
                accountContainers.Add(account, containers);

                result.Add( new FileSystemResult() { Name = account, FileSystems = containers });
            }

            // 
            return new OkObjectResult(result);
        }

        private class FileSystemResult
        {
            //[{name: 'adlstorageaccountname', fileSystems: [{name: 'file system name'}]]
            public string Name { get; set; }
            public List<string> FileSystems { get; set; }
        }
    }
}
