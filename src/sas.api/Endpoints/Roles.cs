using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace sas.api
{
	public static class Roles
	{
		[FunctionName("RolesGET")]
		public static IActionResult RolesGET([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "Roles")]
			HttpRequest req, ILogger log)
		{
			// Request body is supposed to contain the user's access token
			//string Body = await new StreamReader(req.Body).ReadToEndAsync();

			log.LogInformation($"Looking for custom roles to assign to '...'.");

			RolesResult rr = new RolesResult()
			{
				Roles = new string[] { "some-fake-role" }
			};

			return new OkObjectResult(rr);
		}
	}

	public class RolesResult
	{
		public string[] Roles { get; set; }
	}
}