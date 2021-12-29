import React from 'react'
import { useMsal } from '@azure/msal-react'
import { loginRequest } from '../../config/authConfig'
import Button from '@mui/material/Button'

/**
 * Renders a button which, when selected, will open a popup for login
 */
export const SignInButton = () => {
    const { instance } = useMsal()

    const handleLogin = async (instance) => {
        try {
            await instance.loginPopup(loginRequest)
        } catch (e) {
            console.error(e)
        }
    }
    
    return (
        <Button variant='contained' color='warning' onClick={() => handleLogin(instance)}>Sign in</Button>
    )
}
