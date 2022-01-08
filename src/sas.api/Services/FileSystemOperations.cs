using Azure.Identity;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace sas.api.Services
{
    internal class FileSystemOperations
    {
        private readonly ILogger log;
        private readonly DataLakeServiceClient dlsClient;
        public FileSystemOperations(Uri storageUri, ILogger log)
        {
            this.log = log;

            var tenantId = Environment.GetEnvironmentVariable("TENANT_ID");
            var clientId = Environment.GetEnvironmentVariable("APP_REGISTRATION_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
            var tokenCred = new ClientSecretCredential(tenantId, clientId, clientSecret);
            dlsClient = new DataLakeServiceClient(storageUri, tokenCred);
        }

        public bool AddsFolderOwnerToContainerACLAsExecute(string fileSystem, string folderOwner, bool isDefaultScope, out string error)
        {
            log.LogTrace($"Adding '{folderOwner}' (Folder Owner) to the container '{dlsClient}/{fileSystem}' as 'Execute'...");

            // Get Root Directory Client
            var directoryClient = dlsClient.GetFileSystemClient(fileSystem).GetDirectoryClient(string.Empty);
            var accessControlListUpdate = new List<PathAccessControlItem>()
            {
                new PathAccessControlItem(AccessControlType.User, RolePermissions.Execute, isDefaultScope, entityId: folderOwner)
            };
            try
            {
                // Update root container's ACL
                var response = directoryClient.UpdateAccessControlRecursive(accessControlListUpdate);
                var statusFlag = response.GetRawResponse().Status == ((int)HttpStatusCode.OK);
                error = statusFlag ? null : "Error on trying to add Folder Owner as Execute on the root Container. Error 500.";
                return statusFlag;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public IEnumerable<string> GetContainersForUpn(string upn)
        {
            upn = upn.Replace('@', '_').ToLower();     // Translate for guest accounts

            var fileSystems = dlsClient.GetFileSystems();

            foreach (var filesystem in fileSystems)
            {
                var fsClient = dlsClient.GetFileSystemClient(filesystem.Name);
                var rootClient = fsClient.GetDirectoryClient(string.Empty);  // container (root)
                var acl = rootClient.GetAccessControl(userPrincipalName: true);
                foreach (var ac in acl.Value.AccessControlList)
                {
                    log.LogInformation(ac.EntityId);
                }

                if (acl.Value.AccessControlList.Any(
                    p => p.EntityId is not null && p.EntityId.ToLower().StartsWith(upn)))
                {
                    log.LogInformation(filesystem.Name);
                    yield return filesystem.Name;
                }
            }
        }
    }

}

