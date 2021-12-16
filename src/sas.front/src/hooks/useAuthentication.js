import { useEffect, useState } from 'react';
import { useMsal } from "@azure/msal-react"
import { loginRequest } from "../config/authConfig"

const useAuthentication = () => {
    const { instance, accounts } = useMsal()
    const [auth, setAuth] = useState(null);
    const [managementToken, setManagementToken] = useState()
    const [storageToken, setStorageToken] = useState()

    useEffect(() => {
        const request = { ...loginRequest, account: accounts[0] }
        const managementRequest = { ...request, scopes: ["https://management.azure.com/user_impersonation"] }
        const storageRequest = { ...request, scopes: ["https://storage.azure.com/user_impersonation"] }

        const setTokens = response => {
            setAuth(response)
            setManagementToken(response.accessToken)
    
            // Retrieve the Storage Account Token
            instance.acquireTokenSilent(storageRequest)
                .then(response => {
                    setStorageToken(response.accessToken)
                })
        }
    
        // Retrieve the management access token
        instance.acquireTokenSilent(managementRequest)
            .then(response => setTokens(response))
            .catch((e) => {
                instance.acquireTokenPopup(managementRequest)
                    .then(response => setTokens(response))
            })


    }, [accounts, instance]);

    return { account: accounts[0], auth, managementToken, storageToken };
}

export { useAuthentication }