using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using sas.api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

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
			//var accountContainers = new Dictionary<string, IList<string>>();

			// Get the Containers for a upn from each storage account
			var config = SasConfiguration.GetConfiguration();
			var result = new List<FileSystemResult>();
			foreach (var account in config.StorageAccounts)
			{
				// TODO: Centralize this to account for other clouds
				var serviceUri = new Uri($"https://{account}.dfs.core.windows.net");
				var adls = new FileSystemOperations(serviceUri, log);
				var containers = adls.GetContainersForUpn(upn).ToList();
				//accountContainers.Add(account, containers);

				result.Add(new FileSystemResult() { Name = account, FileSystems = containers });
			}

			//
			return new OkObjectResult(result);
		}

		// TODO: Move to separate file
		private class FileSystemResult
		{
			//[{name: 'adlstorageaccountname', fileSystems: [{name: 'file system name'}]]
			public string Name { get; set; }
			public List<string> FileSystems { get; set; }
		}
	}
}
