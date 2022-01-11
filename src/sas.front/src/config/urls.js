// Add the endpoints here for Microsoft Graph API services you'd like to use.
const URLS = {
    graphMe: {
        method: 'GET',
        endpoint: 'https://graph.microsoft.com/v1.0/me'
    },
    storageAccounts: {
        method: 'GET',
        endpoint: '/api/liststorageaccounts'
    },
    containers: {
        method: 'GET',
        endpoint: 'https://{name}.blob.core.windows.net'
    },
    fileSystems: {
        method: 'GET',
        endpoint: '/api/filesystems'
    },
    directories: {
        method: 'GET',
        endpoint: '/api/toplevelfolders'
    },
    createFolder: {
        method: 'POST',
        endpoint: '/api/CreateTopLevelFolder'
    }
}

export default URLS