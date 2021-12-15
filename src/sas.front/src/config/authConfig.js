export const msalConfig = {
    auth: {
        clientId: "29dfc401-84f4-4ca4-9786-e150dd341eb9",
        authority: "https://login.microsoftonline.com/c09c10ee-c0ca-4e10-80c2-d171cddcde6c", // This is a URL (e.g. https://login.microsoftonline.com/{your tenant ID})
        redirectUri: process.env.REACT_APP_BASE_URL,
    },
    cache: {
        cacheLocation: "sessionStorage", // This configures where your cache will be stored
        storeAuthStateInCookie: false, // Set this to "true" if you are having issues on IE11 or Edge
    }
}

// Add scopes here for ID token to be used at Microsoft identity platform endpoints.
export const loginRequest = {
    //scopes: ["https://management.azure.com/user_impersonation", "https://storage.azure.com/user_impersonation"]
    scopes: []
}
