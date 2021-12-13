import URLS from "../config/urls"


/**
 * Returns the list of storage accounts
 */
export const getStorageAccounts = async accessToken => {
    const options = getOptions("GET", accessToken)
    const endpoint = URLS.storageAccounts.endpoint.replace('{subscriptionId}', '3bd58b39-1778-4f39-aa7d-3a6563020b52')

    return fetch(endpoint, options)
        .then(response => {
            return response.json()
        })
        .catch(error => console.log(error))
}


/**
 * Returns the list of folders for the storage account
 *
 export const getFileSystems = async (accessToken, storageAccountId) => {
    const options = getOptions("GET", accessToken)
    //const endpoint = URLS.storageAccounts.endpoint.replace('{subscriptionId}', '3bd58b39-1778-4f39-aa7d-3a6563020b52')
    const endpoint = URLS.fileSystems.endpoint

    return fetch(endpoint, options)
        .then(response => {
            return response.json()
        })
        .catch(error => console.log(error))
}
*/

/**
 * Create the options object to pass to the API call
 */
 const getOptions = (method, accessToken) => {
    const headers = new Headers()
    headers.append("Authorization", `Bearer ${accessToken}`)
    headers.append("Content-Type", "application/json")
    headers.append("x-ms-version", "2021-02-12")

    const options = {
        method: "GET",
        headers: headers
    }

    return options
}
