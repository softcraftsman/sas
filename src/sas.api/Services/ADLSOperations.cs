using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Auth;
using Azure.Identity;
using Microsoft.Identity.Client;
using System.Net;
using Microsoft.Graph;

public class ADLSOperations
{

    private readonly ILogger log;
    private readonly Uri containerUri;
    private readonly string clientToken;
    public ADLSOperations(Uri storageContainer, string token, ILogger log)
    {
        this.log = log;
        this.containerUri = storageContainer;
        this.clientToken = token;
    }

    #region Private Static Methods

    private static DataLakeDirectoryClient GetsReferenceToContainer(DataLakeServiceClient serviceClient, string storageRootContainer, string folder)
    {
        DataLakeDirectoryClient directoryClient = serviceClient.GetFileSystemClient(storageRootContainer).GetDirectoryClient(folder);

        return directoryClient;
    }

    private static DataLakeFileSystemClient GetsReferenceToFileSystem(DataLakeServiceClient serviceClient, string storageRootContainer)
    {
        DataLakeFileSystemClient fileSystem = serviceClient.GetFileSystemClient(storageRootContainer);

        return fileSystem;
    }
    #endregion

    #region Private Methods

    private DataLakeServiceClient CreateDlsClientFromToken()
    {
        log.LogInformation($"Retrieve Environment Variables");
        var tenantId = Environment.GetEnvironmentVariable("TENANT_ID");
        var clientId = Environment.GetEnvironmentVariable("APP_REGISTRATION_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
        var tokenCred = new ClientSecretCredential(tenantId, clientId, clientSecret);
        var dlsClient = new DataLakeServiceClient(containerUri, tokenCred);
        log.LogInformation($"Service Client Created");
        return dlsClient;
    }

    private BlobClient CreateBlobClientFromToken()
    {
        var tenantId = Environment.GetEnvironmentVariable("TENANT_ID");
        var clientId = Environment.GetEnvironmentVariable("APP_REGISTRATION_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
        var tokenCred = new ClientSecretCredential(tenantId, clientId, clientSecret);
        var blobClient = new BlobClient(containerUri, tokenCred);
        return blobClient;
    }
    #endregion

    #region Public Static Methods

    public static async Task ManageDirectoryACLs(DataLakeFileSystemClient fileSystemClient)
    {
        var directoryClient = fileSystemClient.GetDirectoryClient("");
        var directoryAccessControl = await directoryClient.GetAccessControlAsync();
        foreach (var item in directoryAccessControl.Value.AccessControlList)
        {
            Console.WriteLine(item.ToString());
        }

        var accessControlList = PathAccessControlExtensions.ParseAccessControlList("user::rwx,group::r-x,other::rw-");

        directoryClient.SetAccessControlList(accessControlList);

    }
    #endregion

    #region Public Methods
    public bool AddsFolderOwnerToContainerACLAsExecute(string folderOwner, string container, bool isDefaultScope, string storageRootContainer, out string error)
    {
        log.LogTrace($"Adding '{folderOwner}' (Fold Owner) to the container '{container}' as 'Execute'...");

        var serviceClient = CreateDlsClientFromToken();
        var directoryClient = GetsReferenceToContainer(serviceClient, storageRootContainer, String.Empty);

        var accessControlListUpdate = new List<PathAccessControlItem>()
        {
            new PathAccessControlItem(AccessControlType.User, RolePermissions.Execute, isDefaultScope, entityId: folderOwner)
        };
        try
        {
            // Update root container's ACL
            var response = directoryClient.UpdateAccessControlRecursive(accessControlListUpdate, null);
            var statusFlag = response.GetRawResponse().Status == ((int)HttpStatusCode.OK);

            if (statusFlag)
                error = null;
            else
                error = "Error on trying to add Folder Owner as Execute on the root Container. Error 500.";
            return statusFlag;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
    
    public bool CreatesNewFolder(string folder, string storageRootContainer, out string error)
    {
        error = null;
        log.LogTrace($"Creating the folder '{folder}' within the container '{storageRootContainer}'...");

        var serviceClient = this.CreateDlsClientFromToken();
        log.LogTrace("DlsClient created");
        var directoryFileSystem = GetsReferenceToFileSystem(serviceClient, storageRootContainer);
        log.LogTrace("Reference to file system obtained.");
        DataLakeDirectoryClient directory = directoryFileSystem.CreateDirectory(folder);
        log.LogTrace("Directory Client created.");

        var response = directory.Create();
        log.LogTrace("Directory created.");
        var statusFlag = response.GetRawResponse().Status == 201;
        if (!statusFlag)
            error = "Error trying to create the new folder. Error 500.";
        return statusFlag;
    }
    
    public bool AssignsRWXToFolderOwner(string folderOwner, string storageRootContainer, string folder, out string error)
    {
        log.LogTrace($"Assigning RWX permission to Folder Owner ({folderOwner}) at folder's ({folder}) level...");

        var storageClient = this.CreateDlsClientFromToken();
        var directoryClient = GetsReferenceToContainer(storageClient, storageRootContainer, folder);
        PathAccessControl directoryAccessControl = directoryClient.GetAccessControl();
        var accessControlListUpdate = (List<PathAccessControlItem>)directoryAccessControl.AccessControlList;

        int index = -1;
        foreach (var item in accessControlListUpdate)
        {
            if (item.EntityId == folderOwner)
            {
                index = accessControlListUpdate.IndexOf(item);
                break;
            }
        }

        if (index > -1)
        {
            var acType = accessControlListUpdate[index].AccessControlType;
            accessControlListUpdate[index] = new PathAccessControlItem(acType, RolePermissions.Read | RolePermissions.Write | RolePermissions.Execute, entityId: folderOwner);
        }

        var result = directoryClient.SetAccessControlList(accessControlListUpdate);
        var statusFlag = result.GetRawResponse().Status == (int)HttpStatusCode.OK;

        if (statusFlag)
            error = null;
        else
            error = "Error trying to assign the RWX permission to the folder. Error 500.";
        return statusFlag;
    }

    public bool SavesFundCodeIntoContainerMetadata(string fundCode, string container, string folder, out string error)
    {
        log.LogTrace($"Saving FundCode into container's metadata...");

        // Create a BlobServiceClient object which will be used to create a container client
        var blobClient = this.CreateBlobClientFromToken();

        try
        {
            IDictionary<string, string> metadata = new Dictionary<string, string>();

            // Add metadata to the dictionary by calling the Add method
            metadata.Add("FundCode", fundCode);

            var result = blobClient.SetMetadata(metadata);

            if (result.GetRawResponse().Status != 200)
            {
                error = "Error trying to save de Fund Code into Container's metadata. Error 500.";
                return false;
            }

            error = null;
            return true;
        }
        catch (RequestFailedException e)
        {
            error = e.Message;
            return false;
        }
    }

    public long CalculateFolderSize(string folder)
    {
        const string sizeCalcDateKey = "SizeCalcDate";
        const string sizeKey = "Size";
        log.LogTrace($"Calculating size for ({this.containerUri})/({folder})");

        var serviceClient = this.CreateDlsClientFromToken();
        var directoryClient = serviceClient.GetFileSystemClient(containerUri.ToString())
                                           .GetDirectoryClient(folder);

        // Check the Last Calculated Date from the Metadata
        var meta = directoryClient.GetProperties().Value.Metadata;
        var sizeCalcDate = meta.ContainsKey(sizeCalcDateKey)
            ?  DateTime.Parse(meta[sizeCalcDateKey])
            :  DateTime.MinValue;

        // If old calculate size again
        if (DateTime.UtcNow.Subtract(sizeCalcDate).TotalDays > 7)
        {
            var paths = directoryClient.GetPaths(true,false);
            long size = 0; 
            foreach ( var path in paths)
            {
                    size += (path.ContentLength.HasValue)?(int) path.ContentLength:0;
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
    #endregion
}