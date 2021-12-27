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
    private static DataLakeServiceClient CreatesDataLakeConnection()
    {
        //Retrieving environment variables
        var storageAccountName = System.Environment.GetEnvironmentVariable("storageAccountName");
        var storageAccountKey = System.Environment.GetEnvironmentVariable("storageAccountKey");
        var storageServiceUri = System.Environment.GetEnvironmentVariable("storageServiceUri");

        //Creates a shared key credential to access the storage account on behalf of the application
        StorageSharedKeyCredential sharedKeyCredential = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);

        //Create DataLakeServiceClient using StorageSharedKeyCredentials
        DataLakeServiceClient serviceClient = new DataLakeServiceClient(new Uri(storageServiceUri), sharedKeyCredential);

        return serviceClient;
    }

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
        var tenantId = Environment.GetEnvironmentVariable("TENANT_ID");
        var clientId = Environment.GetEnvironmentVariable("APP_REGISTRATION_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
        var tokenCred = new OnBehalfOfCredential(tenantId, clientId, clientSecret, clientToken);
        var dlsClient = new DataLakeServiceClient(containerUri, tokenCred);
        return dlsClient;
    }
    #endregion

    #region Public Static Methods
    public static bool AddsFolderOwnerToContainerACLAsExecute(string folderOwner, string container, bool isDefaultScope, string storageRootContainer)
    {
        var serviceClient = CreatesDataLakeConnection();
        
        var directoryClient = GetsReferenceToContainer(serviceClient, storageRootContainer, "");

        List<PathAccessControlItem> accessControlListUpdate = new List<PathAccessControlItem>()
        {
            new PathAccessControlItem(AccessControlType.User, RolePermissions.Execute, isDefaultScope, entityId: folderOwner)
        };

        //Update root container's ACL
        var result = directoryClient.UpdateAccessControlRecursive(accessControlListUpdate, null);
 
        if(result.GetRawResponse().Status != 200)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public static bool CreatesNewFolder(string folder, string storageRootContainer)
    {
        var serviceClient = CreatesDataLakeConnection();

        var directoryFileSystem = GetsReferenceToFileSystem(serviceClient, storageRootContainer);

        DataLakeDirectoryClient directory = directoryFileSystem.CreateDirectory(folder);
        
        var response = directory.Create();

        if(response.GetRawResponse().Status != 201)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public static bool AssignsRWXToFolderOwner(string folderOwner, string storageRootContainer, string folder)
    {
        var storageClient = CreatesDataLakeConnection();

        var directoryClient = GetsReferenceToContainer(storageClient, storageRootContainer, folder);

        PathAccessControl directoryAccessControl = directoryClient.GetAccessControl();

        List<PathAccessControlItem> accessControlListUpdate = (List<PathAccessControlItem>)directoryAccessControl.AccessControlList;

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
            switch (accessControlListUpdate[index].AccessControlType)
            {
                case AccessControlType.User:
                    accessControlListUpdate[index] = new PathAccessControlItem(AccessControlType.User, RolePermissions.Read | RolePermissions.Write | RolePermissions.Execute, entityId: folderOwner);
                break;

                case AccessControlType.Group:
                    accessControlListUpdate[index] = new PathAccessControlItem(AccessControlType.Group, RolePermissions.Read | RolePermissions.Write | RolePermissions.Execute, entityId: folderOwner);
                break;

                case AccessControlType.Other:
                    accessControlListUpdate[index] = new PathAccessControlItem(AccessControlType.Other, RolePermissions.Read | RolePermissions.Write | RolePermissions.Execute, entityId: folderOwner);
                break;
                
                case AccessControlType.Mask:
                    accessControlListUpdate[index] = new PathAccessControlItem(AccessControlType.Mask, RolePermissions.Read | RolePermissions.Write | RolePermissions.Execute, entityId: folderOwner);
                break;
            }
        }

        var result = directoryClient.SetAccessControlList(accessControlListUpdate);

        if(result.GetRawResponse().Status != 200)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public static bool SavesFundCodeIntoContainerMetadata(string fundCode, string container, string folder)
    {
        var storageAccountConnectionString = System.Environment.GetEnvironmentVariable("storageConnectionString");

        // Create a BlobServiceClient object which will be used to create a container client
        BlobClient blob = new BlobClient(storageAccountConnectionString, container, folder);

        try
        {
            IDictionary<string, string> metadata = new Dictionary<string, string>();

            // Add metadata to the dictionary by calling the Add method
            metadata.Add("FundCode", fundCode);

            var result = blob.SetMetadata(metadata);

            if (result.GetRawResponse().Status != 200)
            {
                return false;
            }

            return true;
        }
        catch (RequestFailedException e)
        {
            return false;
        }
    }

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
        var directoryClient = GetsReferenceToContainer(serviceClient, storageRootContainer, "");

        var accessControlListUpdate = new List<PathAccessControlItem>()
        {
            new PathAccessControlItem(AccessControlType.User, RolePermissions.Execute, isDefaultScope, entityId: folderOwner)
        };

        // Update root container's ACL
        var response = directoryClient.UpdateAccessControlRecursive(accessControlListUpdate, null);
        var statusFlag = response.GetRawResponse().Status == ((int)HttpStatusCode.OK);

        if (statusFlag)
            error = null;
        else
            error = "Error on trying to add Folder Owner as Execute on the root Container. Error 500.";
        return statusFlag;
    }
    
    public bool CreatesNewFolder(string folder, string storageRootContainer, out string error)
    {
        error = null;
        log.LogTrace($"Creating the folder '{folder}' within the container '{storageRootContainer}'...");

        var serviceClient = this.CreateDlsClientFromToken();
        var directoryFileSystem = GetsReferenceToFileSystem(serviceClient, storageRootContainer);
        DataLakeDirectoryClient directory = directoryFileSystem.CreateDirectory(folder);

        var response = directory.Create();
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

    public bool SavesFundCodeIntoContainerMetadata(string fundCode, string container, out string error)
    {
        log.LogTrace($"Saving FundCode into container's metadata...");
        var result = SavesFundCodeIntoContainerMetadata(fundCode, container);
        error = result ? null : "Error trying to save de Fund Code into Container's metadata. Error 500.";
        return result;
    }
    #endregion
}