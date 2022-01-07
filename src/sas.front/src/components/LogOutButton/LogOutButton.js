import React from 'react'
import PropTypes from 'prop-types'
import { useMsal } from '@azure/msal-react'
import Button from '@mui/material/Button'

/**
 * Renders a button which, when selected, will open a popup for logout
 */
const LogOutButton = ({ strings }) => {
    const { instance } = useMsal()

    const handleLogout = async (instance) => {
        try {
            await instance.logoutPopup()
        } catch (e) {
            console.error(e)
        }
    }

    return (
        <Button variant='text' onClick={() => handleLogout(instance)}>
            {strings.logOut}
        </Button>
    )
}

LogOutButton.propTypes = {
    strings: PropTypes.shape({
        logOut: PropTypes.string
    })
}

LogOutButton.defaultProps = {
    strings: {
        logOut: 'Log out'
    }
}

export default LogOutButton
