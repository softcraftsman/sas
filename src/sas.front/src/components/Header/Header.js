import React from 'react'
import PropTypes from 'prop-types'
import useAuthentication from '../../hooks/useAuthentication'
import Avatar from '@mui/material/Avatar'
import LogOutButton from '../LogOutButton'
import './Header.css'

const Header = ({ strings }) => {
    const { account, isAuthenticated } = useAuthentication()

    return (
        <div className='header'>
            <div className='header-logo'>
                <img
                    alt='Logo'
                    src='https://sasfront.blob.core.windows.net/public/duke-logo.svg.png'
                    width='100'
                    height='45'
                />
                <div className='header-divider' />
                <h3>{strings.title}</h3>
            </div>
            <div className='header-profile'>
                {isAuthenticated &&
                    <>
                        <div className='header-profile-image'>
                            <Avatar alt={account.userDetails} />
                        </div>
                        <div className='header-profile-greeting'>
                            {strings.welcome}, {account.userDetails}<br />
                            <LogOutButton strings={strings} />
                        </div>
                    </>
                }
            </div>
        </div>
    )
}

Header.propTypes = {
    strings: PropTypes.shape({
        logOut: PropTypes.string,
        title: PropTypes.string,
        welcome: PropTypes.string,
    })
}

Header.defaultProps = {
    strings: {
        logOut: 'Log out',
        title: 'Storage as a Service',
        welcome: 'Welcome'
    }
}

export default Header
