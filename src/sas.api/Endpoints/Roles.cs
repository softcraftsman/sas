using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.IO;

namespace sas.api
{
	public static class Roles
	{
		[FunctionName("Roles")]
		public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "GET", "POST", Route = "Roles")]
			HttpRequest req, ILogger log)
		{
			RolesResult rr = new RolesResult();

			// Request body is supposed to contain the user's access token
			if (req.Body.Length > 0)
			{
				string Body = await new StreamReader(req.Body).ReadToEndAsync();

				//log.LogInformation($"Looking for custom roles to assign to '...'.");
				log.LogInformation($"Input\n{Body}");

				rr = new RolesResult()
				{
					Roles = new string[] { "some-fake-role", "second-fake-role" }
				};
			}

			return new OkObjectResult(rr);
		}
	}

	public class RolesResult
	{
		public string[] Roles { get; set; }
	}
}