using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class UserOperations
{
	public static async Task<string> GetApiToken(HttpRequest req, ILogger log)
	{
		try
		{
			var accessToken = GetAccessTokenFromRequest(req);
			var userAssertion = new UserAssertion(accessToken);

			ConfidentialClientApplicationOptions options = new()
			{
				TenantId = Environment.GetEnvironmentVariable("TENANT_ID"),
				ClientId = Environment.GetEnvironmentVariable("APP_REGISTRATION_CLIENT_ID"),
				ClientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET"),
			};
			var app = ConfidentialClientApplicationBuilder.CreateWithApplicationOptions(options).Build();

			var scopes = new string[] { "https://graph.microsoft.com" };

			var authResult = await app.AcquireTokenOnBehalfOf(scopes, userAssertion).ExecuteAsync();

			return authResult.AccessToken;
		}
		catch (Exception ex)
		{
			log.LogError(ex.Message);
			return null;
		}
	}

	public static string GetAccessTokenFromRequest(HttpRequest req)
	{
		// Get Caller Access Token
		string accessToken = null;
		if (req.Headers.ContainsKey("Authorization"))
			accessToken = req.Headers.FirstOrDefault(x => x.Key == "Authorization")
				.Value.First().Split(' ').LastOrDefault();
		return accessToken;

	}
	public static async Task<string> GetObjectIdFromUPN(string accessToken, string upn)
	{
		try
		{
			var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
			{
				requestMessage
					.Headers
					.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

				return Task.FromResult(0);
			}));

			// Retrieve a user by userPrincipalName
			var user = await graphClient
				.Users[upn]
				.Request()
				.GetAsync();

			return user.Id;
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
			return null;
		}
	}

	public static ClaimsPrincipal GetClaimsPrincipal(HttpRequest req)
	{
		var principal = new ClientPrincipal();

		if (req.Headers.TryGetValue("x-ms-client-principal", out var header))
		{
			var data = header[0];
			var decoded = Convert.FromBase64String(data);
			var json = Encoding.UTF8.GetString(decoded);
			principal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		}

		// TODO: Document why the 'anonymous' role is being excluded?
		principal.UserRoles = principal.UserRoles?.Except(new string[] { "anonymous" }, StringComparer.CurrentCultureIgnoreCase);

		// If there are no roles left after removing the 'anonymous' role
		if (!principal.UserRoles?.Any() ?? true)
		{
			// Return a default ClaimsPrincipal
			return new ClaimsPrincipal();
		}

		// There are role(s) other than 'anonymous' in the claim
		var identity = new ClaimsIdentity(principal.IdentityProvider);
		identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.UserId));
		identity.AddClaim(new Claim(ClaimTypes.Name, principal.UserDetails));
		identity.AddClaims(principal.UserRoles.Select(r => new Claim(ClaimTypes.Role, r)));

		return new ClaimsPrincipal(identity);
	}

	// TODO: Move to separate class?
	private class ClientPrincipal
	{
		public string IdentityProvider { get; set; }
		public string UserId { get; set; }
		public string UserDetails { get; set; }
		public IEnumerable<string> UserRoles { get; set; }
	}
}
