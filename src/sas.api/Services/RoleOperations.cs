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

        private RoleAssignment AddRoleAssignment(string scope, string roleName, string principalId)
        {
            VerifyToken();
            var amClient = new AuthorizationManagementClient(tokenCredentials);
            var roleDefinitions = amClient.RoleDefinitions.List(scope);
            var roleDefinition = roleDefinitions.First(x => x.RoleName == roleName);

            // TODO: Add OData Filter
            var roleAssignments = amClient.RoleAssignments.ListForScope(scope);
            var roleAssignment = roleAssignments.FirstOrDefault(ra => ra.PrincipalId == principalId && ra.RoleDefinitionId == roleDefinition.Id);

            // Create New Role Assignment
            if (roleAssignment == null)
            {
                var racp = new RoleAssignmentCreateParameters(roleDefinition.Id, principalId);
                var roleAssignmentId = Guid.NewGuid().ToString();
                roleAssignment = amClient.RoleAssignments.Create(scope, roleAssignmentId, racp);
            }
            return roleAssignment;
        }

        public void AssignRoles(string account, string container, string ownerId)
        {
            try
            {
                // Get Storage Account Resource ID
                var accountResourceId = GetAccountResourceId(account);

                // Create Role Assignments
                string containerScope = $"{accountResourceId}/blobServices/default/containers/{container}";

                // Allow user to manage ACL for container
                var ra = AddRoleAssignment(containerScope, "Storage Blob Data Owner", ownerId);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public void GetRoles(string account, string container, string ownerId)
        {
            try
            {
                // Get Storage Account Resource ID
                var accountResourceId = GetAccountResourceId(account);

                // Create Role Assignments
                string containerScope = $"{accountResourceId}/blobServices/default/containers/{container}";

                // Allow user to manage ACL for container
                //var ra = AddRoleAssignment(containerScope, "Storage Blob Data Owner", ownerId);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private RoleAssignment GetRoleAssignment(string scope, string roleName, string principalId)
        {
            VerifyToken();
            var amClient = new AuthorizationManagementClient(tokenCredentials);
            var roleDefinitions = amClient.RoleDefinitions.List(scope);
            var roleDefinition = roleDefinitions.First(x => x.RoleName == roleName);

            // TODO: Add OData Filter
            var roleAssignments = amClient.RoleAssignments.ListForScope(scope);
            var roleAssignment = roleAssignments.FirstOrDefault(ra => ra.PrincipalId == principalId && ra.RoleDefinitionId == roleDefinition.Id);

            return roleAssignment;
        }
    }
}
