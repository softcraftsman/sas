import URLS from '../config/urls'
import { DataLakeServiceClient } from '@azure/storage-file-datalake'
import { InteractiveBrowserCredential } from '@azure/identity'

/**
 * Returns the list of File Systems for the Azure DataLake Storage Account
 */
export const getFileSystems = async (storageAccount) => {
    const endpoint = URLS.fileSystems.endpoint.replace('{name}', storageAccount)
    const credential = new InteractiveBrowserCredential({
        tenantId: process.env.REACT_APP_TENANT_ID,
        clientId: process.env.REACT_APP_CLIENT_ID
    })
    const dataLakeServiceClient = new DataLakeServiceClient(endpoint, credential)
    return dataLakeServiceClient.listFileSystems()
}


/**
 * Returns the list of Directories for the File System
 */
export const getDirectories = async (storageAccount, fileSystem) => {
    const endpoint = URLS.fileSystems.endpoint.replace('{name}', storageAccount)
    const credential = new InteractiveBrowserCredential({
        tenantId: process.env.REACT_APP_TENANT_ID,
        clientId: process.env.REACT_APP_CLIENT_ID
    })
    const dataLakeServiceClient = new DataLakeServiceClient(endpoint, credential)
    const fileSystemClient = dataLakeServiceClient.getFileSystemClient(fileSystem)

    const _fileSystems = []
    const iter = fileSystemClient.listPaths()

    for await (const fs of iter) {
        // Get additional information
        const directoryClient = fileSystemClient.getDirectoryClient(fs.name)
        const properties = await directoryClient.getProperties()

        // Nothing really useful is returned by getAccessControl
        //const accessControl = await directoryClient.getAccessControl()

        _fileSystems.push({ ...fs, accessTier: properties.accessTier, fundCode: properties.metadata.fundcode })
    }

    return _fileSystems
}
