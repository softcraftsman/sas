using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Management.Authorization;
using Microsoft.Azure.Management.Authorization.Models;
using Microsoft.Azure.Management.ResourceGraph;
using Microsoft.Azure.Management.ResourceGraph.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using System;
using System.Linq;

namespace sas.api.Services
{
    internal class RoleOperations
    {
        // https://blogs.aaddevsup.xyz/2020/05/using-azure-management-libraries-for-net-to-manage-azure-ad-users-groups-and-rbac-role-assignments/

        private readonly ILogger log;
        private TokenCredentials tokenCredentials;

        public RoleOperations(ILogger log)
        {
            this.log = log;
        }

        private void VerifyToken()
        {
            if (tokenCredentials == null)
            {
                var tokenRequestContext = new TokenRequestContext(new[] { "https://management.azure.com/.default" });
                var accessToken = new DefaultAzureCredential().GetToken(tokenRequestContext);
                tokenCredentials = new TokenCredentials(accessToken.Token);
            }
        }

        private string GetAccountResourceId(string account)
        {
            string accountResourceId = string.Empty;
            try
            {
                VerifyToken();
                var resourceGraphClient = new ResourceGraphClient(tokenCredentials);

                var query = new QueryRequest();
                query.Query = $"resources | where name == '{account}' | project id";
                var queryResponse = resourceGraphClient.Resources(query);
                if (queryResponse.Count > 0)
                {
                    dynamic data = queryResponse;
                    Console.WriteLine(data);

                    var data2 = (Newtonsoft.Json.Linq.JArray)queryResponse.Data;
                    var data3 = data2.First;
                    accountResourceId = data3.SelectToken("id").ToString();

                }
            }
            catch (Exception)
            {
                throw;
            }

            return accountResourceId;
        }


        public void AssignRoles(string account, string container, string ownerId)
        {

            // Get Storage Account Resource ID
            var accountResourceId = GetAccountResourceId(account);

            // Create Role Assignments
            string containerScope = $"{accountResourceId}/blobServices/default/containers/{container}";

            // Get Role Definitions
            var amClient = new AuthorizationManagementClient(tokenCredentials);
            var roleDefinitions = amClient.RoleDefinitions.List(containerScope);
            var rdDataOwner = roleDefinitions.First(x => x.RoleName == "Storage Blob Data Owner");
            var rdUserAccessAdministrator = roleDefinitions.First(x => x.RoleName == "User Access Administrator");
            var racp = new RoleAssignmentCreateParameters(rdDataOwner.Id, ownerId);
            var roleAssignment = amClient.RoleAssignments.Create(containerScope, "mytest", racp);

            Console.WriteLine(roleAssignment);




            //// test
            //var credentials = SdkContext.AzureCredentialsFactory
            //    .FromServicePrincipal("clientid", "clientSecret", "tenantId", AzureEnvironment.AzureGlobalCloud);

            //// authenticate to Azure AD
            //var authenticated = Microsoft.Azure.Management.Fluent.Azure
            //            .Configure()
            //            .Authenticate(credentials);

            //// Find User
            //var querieduser = authenticated.ActiveDirectoryUsers
            //                    .GetById("user.Id");

            //var x= Microsoft.Azure.Management.Graph.RBAC.Fluent.RoleAssignmentHelper;
            //x.WithAccessTo

            //var scope = "subscriptions/<Subscription ID>/resourceGroups/<Resource Group Name>/providers/Microsoft.Storage/storageAccounts/<Storage Account Name>";
            //authenticated.RoleAssignments
            //    .ListByScope(scope)
            //    .
            //    .Where( s => s.)


            //// Create new RBAC Role Assignment
            //IRoleAssignment roleAssignment = authenticated.RoleAssignments
            //    .Define("raName")
            //    .ForUser(querieduser)
            //    .WithRoleDefinition("/subscriptions/<Subscription ID>/resourceGroups/<Resource Group Name>/providers/Microsoft.Storage/storageAccounts/<Storage Account Name>/blobServices/default/containers/<Blob Container Name>/providers/Microsoft.Authorization/roleDefinitions/<RBAC Role ID from step 4 above>")
            //    .WithScope("subscriptions/<Subscription ID>/resourceGroups/<Resource Group Name>/providers/Microsoft.Storage/storageAccounts/<Storage Account Name>/blobServices/default/containers/<Blob Container Name>")
            //    .Create();


            //Console.WriteLine(roleAssignment);
        }


    }
}
