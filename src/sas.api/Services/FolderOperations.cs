using Azure.Identity;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace sas.api.Services
{
    internal class FolderOperations
    {
        private readonly ILogger log;
        private readonly DataLakeFileSystemClient dlfsClient;
        private readonly decimal costPerTB;
        public FolderOperations(Uri storageUri, string fileSystem, ILogger log)
        {
            this.log = log;
            var costPerTB = Environment.GetEnvironmentVariable("COST_PER_TB");
            if (costPerTB != null)
                decimal.TryParse(costPerTB, out this.costPerTB);

            var tenantId = Environment.GetEnvironmentVariable("TENANT_ID");
            var clientId = Environment.GetEnvironmentVariable("APP_REGISTRATION_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
            var tokenCred = new ClientSecretCredential(tenantId, clientId, clientSecret);
            dlfsClient = new DataLakeServiceClient(storageUri, tokenCred).GetFileSystemClient(fileSystem);
        }

        internal async Task<Result> CreateNewFolder(string folder)
        {
            var result = new Result();
            log.LogTrace($"Creating the folder '{folder}' within the container '{dlfsClient.Uri}'...");

            try
            {
                var directoryClient = dlfsClient.GetDirectoryClient(folder);
                var response = await directoryClient.CreateAsync();
                result.Success = response.GetRawResponse().Status == 201;
                if (!result.Success)
                {
                    result.Message = "Error trying to create the new folder. Error 500.";
                    log.LogError(result.Message);
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                log.LogError(result.Message);
            }
            return result;
        }

        internal async Task<Result> AssignFullRwx(string folder, string folderOwner)
        {
            log.LogTrace($"Assigning RWX permission to Folder Owner ({folderOwner}) at folder's ({folder}) level...");

            var result = new Result();
            var directoryClient = dlfsClient.GetDirectoryClient(folder);
            var accessControlListUpdate = new List<PathAccessControlItem>
            {
                new PathAccessControlItem(
                    accessControlType: AccessControlType.User,
                    permissions: RolePermissions.Read | RolePermissions.Write | RolePermissions.Execute,
                    entityId: folderOwner),
                new PathAccessControlItem(
                    accessControlType: AccessControlType.User,
                    permissions: RolePermissions.Read | RolePermissions.Write | RolePermissions.Execute,
                    entityId: folderOwner,
                    defaultScope: true)
            };

            // Send up changes
            var resultACL = await directoryClient.UpdateAccessControlRecursiveAsync(accessControlListUpdate);
            result.Success = resultACL.GetRawResponse().Status == (int)HttpStatusCode.OK;
            result.Message = result.Success ? null : "Error trying to assign the RWX permission to the folder. Error 500.";
            return result;
        }

        internal async Task<Result> AddFundCodeToMetaData(string folder, string fundCode)
        {
            log.LogTrace($"Saving FundCode into container's metadata...");
            try
            {
                var directoryClient = dlfsClient.GetDirectoryClient(folder);

                // Check the Last Calculated Date from the Metadata
                var meta = (await directoryClient.GetPropertiesAsync()).Value.Metadata;

                // Add Fund Code
                meta.Add("FundCode", fundCode);

                // Strip off a readonly item
                meta.Remove("hdi_isfolder");

                // Save back into the Directory Metadata
                directoryClient.SetMetadata(meta);
            }catch (Exception ex)
            {
                return new Result { Success = false, Message = ex.Message };
            }
            return new Result() { Success = true };
        }

        internal async Task<long> CalculateFolderSize(string folder)
        {
            const string sizeCalcDateKey = "SizeCalcDate";
            const string sizeKey = "Size";
            log.LogTrace($"Calculating size for ({dlfsClient.Uri})/({folder})");

            var directoryClient = dlfsClient.GetDirectoryClient(folder);

            // Check the Last Calculated Date from the Metadata
            var meta = (await directoryClient.GetPropertiesAsync()).Value.Metadata;
            var sizeCalcDate = meta.ContainsKey(sizeCalcDateKey)
                ? DateTime.Parse(meta[sizeCalcDateKey])
                : DateTime.MinValue;

            // If old calculate size again
            if (DateTime.UtcNow.Subtract(sizeCalcDate).TotalDays > 7)
            {
                var paths = directoryClient.GetPaths(true, false);
                long size = 0;
                foreach (var path in paths)
                {
                    size += (path.ContentLength.HasValue) ? (int)path.ContentLength : 0;
                }
                meta[sizeCalcDateKey] = DateTime.UtcNow.ToString();
                meta[sizeKey] = size.ToString();

                // Strip off a readonly item
                meta.Remove("hdi_isfolder");

                // Save back into the Directory Metadata
                directoryClient.SetMetadata(meta);
            }

            return long.Parse(meta[sizeKey]);
        }
        private static string Simplify(string s)
        {
            return s.Replace('@', '_').ToLower();
        }

        internal FolderDetail GetFolderDetail(string folder)
        {
            var rootClient = dlfsClient.GetDirectoryClient(folder);  // container (root)
            var meta = rootClient.GetProperties().Value.Metadata;
            var acl = rootClient.GetAccessControl(userPrincipalName: true).Value.AccessControlList;
            FolderDetail fd = BuildFolderDetail(folder, meta, acl);
            return fd;
        }

        internal IEnumerable<FolderDetail> GetAccessibleFolders(string upn)
        {
            // Get all Top Level Folders
            var folders = dlfsClient.GetPaths().Where<PathItem>(
                pi => pi.IsDirectory != null && (bool)pi.IsDirectory)
                .ToList();

            // Translate for guest accounts
            upn = Simplify(upn);

            // Find folders that have ACL entries for upn
            foreach (var folder in folders)
            { 
                var rootClient = dlfsClient.GetDirectoryClient(folder.Name);  // container (root)
                var meta = rootClient.GetProperties().Value.Metadata;
                var acl = rootClient.GetAccessControl(userPrincipalName: true).Value.AccessControlList;
                if (acl.Any( p => p.EntityId is not null && Simplify(p.EntityId).StartsWith(upn)
                        && p.Permissions.HasFlag(RolePermissions.Read)    ))
                {
                    FolderDetail fd = BuildFolderDetail(folder.Name, meta, acl);
                    yield return fd;
                }
            }
        }

        private FolderDetail BuildFolderDetail(string folder, IDictionary<string, string> meta, IEnumerable<PathAccessControlItem> acl)
        {
            // Get Metadata
            long? size = meta.ContainsKey("Size") ? long.Parse(meta["Size"]) : null;
            decimal? cost = (size == null) ? null : size * costPerTB / 1000000000000;

            // Calculate UserAccess
            var userAccess = acl
                .Where(p => p.AccessControlType == AccessControlType.User && p.EntityId != null && !p.DefaultScope)
                .Select(p => p.EntityId)
                .ToList();

            // Create Folder Details
            var fd = new FolderDetail()
            {
                Name = folder,
                Size = meta.ContainsKey("Size") ? meta["Size"] : null,
                Cost = cost.HasValue ? cost.Value.ToString() : null,
                FundCode = meta.ContainsKey("FundCode") ? meta["FundCode"] : null,
                UserAccess = userAccess
            };
            return fd;
        }

        internal class FolderDetail
        {
            public string Name { get; set; }
            public string Size { get; set; }
            public string Cost { get; set; }
            public string FundCode { get; set; }
            public IList<string> UserAccess { get; set; }
        }
    }
}
