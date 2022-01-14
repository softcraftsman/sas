using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sas.api
{
	public static class Roles
	{
		[FunctionName("RolesGET")]
		public static RolesResult RolesGET([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "Roles")]
			HttpRequest req, ILogger log)
		{
			// Request body is supposed to contain the user's access token
			//string Body = await new StreamReader(req.Body).ReadToEndAsync();

			log.LogInformation($"Looking for custom roles to assign to '...'.");

			return new RolesResult()
			{
				Roles = new List<string>() { "some-fake-role" }.ToArray()
			};
		}
	}

	public class RolesResult
	{
		public string[] Roles { get; set; }
	}
}