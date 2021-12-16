import URLS from "../config/urls"
import { BlobServiceClient } from "@azure/storage-blob"
import { InteractiveBrowserCredential } from "@azure/identity"


/**
 * Returns the list of folders for the azure datalake storage account
 */
export const getContainers = async (storageAccountId) => {
    const name = storageAccountId.substring(storageAccountId.lastIndexOf('/') + 1)
    const endpoint = URLS.fileSystems.endpoint.replace('{name}', name)
    const credential = new InteractiveBrowserCredential({
        tenantId: "c09c10ee-c0ca-4e10-80c2-d171cddcde6c",
        clientId: "29dfc401-84f4-4ca4-9786-e150dd341eb9"
    })
    const blobServiceClient = new BlobServiceClient(endpoint, credential)

    return blobServiceClient.listContainers()
}