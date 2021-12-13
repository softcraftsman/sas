// Add the endpoints here for Microsoft Graph API services you'd like to use.
const URLS = {
    graphMe: {
        method: 'GET',
        endpoint: 'https://graph.microsoft.com/v1.0/me'
    },
    storageAccounts: {
        method: 'GET',
        endpoint: 'https://management.azure.com/subscriptions/{subscriptionId}/providers/Microsoft.Storage/storageAccounts?api-version=2021-04-01'
    },
    fileSystems: {
        method: 'GET',
        endpoint: 'https://{name}.blob.core.windows.net/?comp=list'
    }
}

export default URLS