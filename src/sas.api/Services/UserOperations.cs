using Microsoft.Graph;
using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Identity;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Azure.Identity;
using Azure.Core;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Identity.Client;

public class UserOperations
{
    public static async Task<string> GetApiToken(HttpRequest req)
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

    public static string GetAccessTokenFromRequest(HttpRequest req)
    {
        // Get Caller Access Token
        var accessToken = req.Headers.First(x => x.Key == "Authorization").Value.First().Split(' ').LastOrDefault();
        
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
}