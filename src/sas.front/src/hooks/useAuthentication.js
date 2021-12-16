import { useEffect, useState } from 'react';
import { useMsal } from '@azure/msal-react'
import { loginRequest } from '../config/authConfig'

const useAuthentication = () => {
    const { instance, accounts } = useMsal()
    const [auth, setAuth] = useState(null);
    const [managementToken, setManagementToken] = useState()
    const [storageToken, setStorageToken] = useState()

    useEffect(() => {
        const request = { ...loginRequest, account: accounts[0] }
        const managementRequest = { ...request, scopes: ['https://management.azure.com/user_impersonation'] }
        const storageRequest = { ...request, scopes: ['https://storage.azure.com/user_impersonation'] }

        const setTokens = async managementResponse => {
            setAuth(managementResponse)
            setManagementToken(managementResponse.accessToken)

            // Retrieve the Storage Account Token
            const storageAccountResponse = await instance.acquireTokenSilent(storageRequest)
            setStorageToken(storageAccountResponse.accessToken)
        }

        const getToken = async () => {
            // Retrieve the management access token
            try {
                const silentResponse = await instance.acquireTokenSilent(managementRequest)
                setTokens(silentResponse)
            } catch (e) {
                const popupResponse = await instance.acquireTokenPopup(managementRequest)
                setTokens(popupResponse)
            }
        }

        getToken()

    }, [accounts, instance]);

    return { account: accounts[0], auth, managementToken, storageToken };
}

export { useAuthentication }