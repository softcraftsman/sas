import URLS from '../config/urls'


/**
 * Returns the list of storage accounts
 */
export const getStorageAccounts = async accessToken => {
    const { endpoint, method } = URLS.storageAccounts
    const options = getOptions(method, accessToken)

    return fetch(endpoint, options)
        .then(response => {
            return response.json()
        })
        .catch(error => console.log(error))
}


/**
 * Create the options object to pass to the API call
 */
const getOptions = (method, accessToken) => {
    const headers = new Headers()
    headers.append('Authorization', `Bearer ${accessToken}`)

    const options = {
        method: method,
        //headers: headers
    }

    return options
}


/**
 * Create a new folder in the storage account container
 */
export const createFolder = async (accessToken, content, storageAccount, fileSystem) => {
    const { endpoint, method } = URLS.createFolder
    const options = getOptions(method, accessToken)
    options.body = JSON.stringify({
        StorageAcount: storageAccount,
        Container: fileSystem,
        Folder: content.name,
        FundCode: content.fundCode,
        FolderOwner: 'fredgohsman@microsoft.com'
    })

    return fetch(endpoint, options)
        .then(response => {
            return response.json()
        })
        .catch(error => console.log(error))
}


