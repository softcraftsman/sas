import URLS from '../config/urls'


/**
 * Returns the list of storage accounts
 */
export const getStorageAccounts = async () => {
    const { endpoint, method } = URLS.storageAccounts
    const options = getOptions(method)

    return fetch(endpoint, options)
        .then(response => {
            return response.json()
        })
        .catch(error => console.log(error))
}


/**
 * Returns the list of storage accounts and their file systems
 */
export const getFileSystems = async () => {
    return new Promise(resolve => resolve([{ name: 'adlfredgohsman', items: [{ name: 'numberuno' }, { name: 'files' }] }]))

    /*
    const { endpoint, method } = URLS.fileSystems
    const options = getOptions(method)

    return fetch(endpoint, options)
        .then(response => {
            return response.json()
        })
        .catch(error => console.log(error))
        */
}


/**
 * Returns the list of directories
 */
export const getDirectories = async () => {
    return new Promise(resolve => resolve([
        { name: 'folder one', size: '1', cost: '0.01', fundCode: 'abcdefg', userAccess: ['fredgohsman@microsoft.com', 'johnbrown@microsoft.com', 'fabriciosanchez@microsoft.com'] },
        { name: 'directory two', size: '2', cost: '0.2', fundCode: 'cdefghi', userAccess: ['fredgohsman@microsoft.com'] }
    ]))

    /*
    const { endpoint, method } = URLS.directories
    const options = getOptions(method)

    return fetch(endpoint, options)
        .then(response => {
            return response.json()
        })
        .catch(error => console.log(error))
        */
}


/**
 * Create the options object to pass to the API call
 */
const getOptions = (method) => {
    const options = {
        method: method,
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


