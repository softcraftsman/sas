import React from 'react'
import { useMsal } from '@azure/msal-react'
import Button from '@mui/material/Button'

/**
 * Renders a button which, when selected, will open a popup for logout
 */
export const SignOutButton = () => {
    const { instance } = useMsal()

    const handleLogout = async (instance) => {
        try {
            await instance.logoutPopup()
        } catch (e) {
            console.error(e)
        }
    }
    
    return (
        <Button variant='contained' color='warning' onClick={() => handleLogout(instance)}>Sign out</Button>
    )
}
