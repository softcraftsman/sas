using Azure.Identity;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public async Task<Result> AddsFolderOwnerToContainerACLAsExecute(string fileSystem, string folderOwner)
        {
            var result = new Result();
            log.LogTrace($"Adding '{folderOwner}' (Folder Owner) to the container '{dlsClient}/{fileSystem}' as 'Execute'...");

            // Get Root Directory Client
            var directoryClient = dlsClient.GetFileSystemClient(fileSystem).GetDirectoryClient(string.Empty);
            var owner = folderOwner.Replace('@', '_').ToLower();
            var acl = (await directoryClient.GetAccessControlAsync(userPrincipalName: true)).Value.AccessControlList.ToList();
            var ownerAcl = acl.FirstOrDefault(p => p.EntityId != null && p.EntityId.Replace('@', '_').ToLower() == owner);
            if (ownerAcl != null)
            {
                if (ownerAcl.Permissions.HasFlag(RolePermissions.Execute))
                {
                    result.Success = true;
                    return result;                    // Exit Early, no changes needed
                }
                ownerAcl.Permissions = RolePermissions.Execute;
            }
            else
                acl.Add(new PathAccessControlItem(AccessControlType.User, RolePermissions.Execute, false, folderOwner));

            try
            {
                // Update root container's ACL
                var response = directoryClient.SetAccessControlList(acl);
                var statusFlag = response.GetRawResponse().Status == ((int)HttpStatusCode.OK);
                result.Message = !statusFlag ? null : "Error on trying to add Folder Owner as Execute on the root Container. Error 500.";
                return result;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                return result;
            }
        }

        public IEnumerable<string> GetContainersForUpn(string upn)
        {
            upn = upn.Replace('@', '_').ToLower();     // Translate for guest accounts
            List<FileSystemItem> fileSystems;
            try
            {
                fileSystems = dlsClient.GetFileSystems().ToList();

            }
            catch( Exception ex)
            {
                Debug.WriteLine(ex.Message);
                yield break;
            }

            foreach (var filesystem in fileSystems)
            {
                var fsClient = dlsClient.GetFileSystemClient(filesystem.Name);
                var rootClient = fsClient.GetDirectoryClient(string.Empty);  // container (root)
                var acl = rootClient.GetAccessControl(userPrincipalName: true);

                if (acl.Value.AccessControlList.Any(
                    p => p.EntityId is not null 
                    && p.EntityId.Replace('@', '_').ToLower().StartsWith(upn)))
                {
                    yield return filesystem.Name;
                }
            }
        }
    }

}

