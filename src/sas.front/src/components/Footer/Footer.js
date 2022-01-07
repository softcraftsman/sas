import React from 'react'
import PropTypes from 'prop-types'
import './Footer.css'

const Footer = ({company}) => {
    return (
        <section className='footer'>
            {new Date().getFullYear()} - Duke University - All rights reserved
        </section>
    )
}

Footer.propTypes = {
    company: PropTypes.string
}

export default Footer
