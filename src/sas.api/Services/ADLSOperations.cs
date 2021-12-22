using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;

public class ADLSOperations
{
    public static bool AddsFolderOwnerToContainerACLAsExecute(string folderOwner, string container, bool isDefaultScope)
    {
        //Retrieve both Storage Account Name and Key from environment variables
        var storageAccountName = ReturnStorageAccountName();
        var storageAccountKey = ReturnStorageAccountKey();
        var storageAccountUri = ReturnStorageAccountUri();

        //Creates a shared key credential to access the storage account on behalf of the application
        StorageSharedKeyCredential sharedKeyCredential = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);

        //Create DataLakeServiceClient using StorageSharedKeyCredentials
        DataLakeServiceClient serviceClient = new DataLakeServiceClient(new Uri(storageAccountUri), sharedKeyCredential);

        // Get a reference to the root file system (container)
        DataLakeDirectoryClient directoryClient = serviceClient.GetFileSystemClient(container).GetDirectoryClient("");

        List<PathAccessControlItem> accessControlListUpdate = new List<PathAccessControlItem>()
        {
            new PathAccessControlItem(AccessControlType.User, RolePermissions.Execute, isDefaultScope,entityId: folderOwner)
        };

        //Update root container's ACL
        var result = directoryClient.UpdateAccessControlRecursive(accessControlListUpdate, null);
 
        if(result.Value.BatchFailures.Length == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool CreatesNewFolder(string folder)
    {
        return true;
    }

    public static bool AssignsRWXToFolderOwner(string folderOwner, string folder)
    {
        return true;
    }

    public static bool SavesFundCodeIntoContainerMetadata(string fundCode, string container)
    {
        return true;
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

    private static string ReturnStorageAccountName()
    {
        return System.Environment.GetEnvironmentVariable("storageAccountName", EnvironmentVariableTarget.Process);
    }

    private static string ReturnStorageAccountKey()
    {
        return System.Environment.GetEnvironmentVariable("storageAccountKey", EnvironmentVariableTarget.Process);
    }

    private static string ReturnStorageAccountUri()
    {
        return System.Environment.GetEnvironmentVariable("serviceUri", EnvironmentVariableTarget.Process);
    }

    private static string ReturnStorageAccountContainer()
    {
        return System.Environment.GetEnvironmentVariable("container", EnvironmentVariableTarget.Process);
    }
}