import React from 'react'
import PropTypes from 'prop-types'
import CopyrightIcon from '@mui/icons-material/Copyright'
import './Footer.css'

const Footer = ({strings}) => {
    return (
        <section className='footer'>
            <CopyrightIcon />{new Date().getFullYear()} - {strings.companyName} - All rights reserved
        </section>
    )
}

Footer.propTypes = {
    company: PropTypes.string
}

export default Footer
