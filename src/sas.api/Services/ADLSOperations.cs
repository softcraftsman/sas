public class ADLSOperations
{
    public static bool AddsFolderOwnerToContainerACLAsExecute(string container, string folderOwner)
    {
        return true;
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
}