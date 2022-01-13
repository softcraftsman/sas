import URLS from '../config/urls'


/**
 * Returns the list of storage accounts and their file systems
 */
export const getFileSystems = async () => {
    const { endpoint, method } = URLS.fileSystems
    const options = getOptions(method)

    return fetch(endpoint, options)
        .then(response => {
            return response.json()
        })
        .catch(error => {
            console.log(`Call to API (${endpoint}) failed with the following details:`)
            console.log(error)
            return []
        })
}


/**
 * Returns the list of directories
 */
export const getDirectories = async (storageAccount, fileSystem) => {
    const endpoint = URLS.listDirectories.endpoint.replace('{account}', storageAccount).replace('{filesystem}', fileSystem)
    const options = getOptions(URLS.listDirectories.method)

    return fetch(endpoint, options)
        .then(response => {
            return response.json()
        })
        .catch(error => {
            console.log(`Call to API (${endpoint}) failed with the following details:`)
            console.log(error)
            return []
        })
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
export const createFolder = async (storageAccount, fileSystem, owner, content) => {
    const endpoint = URLS.listDirectories.endpoint.replace('{account}', storageAccount).replace('{filesystem}', fileSystem)
    const options = getOptions(URLS.createFolder.method)
    options.body = JSON.stringify({
        Folder: content.name,
        FundCode: content.fundCode,
        FolderOwner: owner
    })

    return fetch(endpoint, options)
        .then(response => {
            return response.json()
        })
        .catch(error => console.log(error))
}


