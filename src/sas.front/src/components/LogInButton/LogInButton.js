import React from 'react'
import PropTypes from 'prop-types'
import { useMsal } from '@azure/msal-react'
import { loginRequest } from '../../config/authConfig'
import Button from '@mui/material/Button'
import Person from '@mui/icons-material/PersonTwoTone'

/**
 * Renders a button which, when selected, will open a popup for login
 */
const LogInButton = ({ text }) => {
    const { instance } = useMsal()

    const handleLogin = async (instance) => {
        try {
            await instance.loginPopup(loginRequest)
        } catch (e) {
            console.error(e)
        }
    }

    return (
        <Button variant='outlined' startIcon={<Person />} onClick={() => handleLogin(instance)}>
            {text}
        </Button>
    )
}

LogInButton.propTypes = {
    text: PropTypes.string
}

LogInButton.defaultProps = {
    text: 'Log In'
}

export default LogInButton
