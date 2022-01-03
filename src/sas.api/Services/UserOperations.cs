using Microsoft.Graph;
using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Identity;
using Microsoft.AspNetCore.Http;
using System.Linq;

public class UserOperations
{
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