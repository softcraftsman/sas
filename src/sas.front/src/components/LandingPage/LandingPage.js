import React from "react"
import { Link } from "react-router-dom"
import Button from '@mui/material/Button'
import WhatCanIDo from "../WhatCanIDo"
import HowToGainAccess from "../HowToGainAccess"

const LandingPage = () => {
    return (
        <>
            <div style={{ margin: '10px' }}>
                <Button variant="contained" color='primary'>
                    <Link to="/storage" style={{ textDecoration: 'none', color: '#FFF' }}>Storage Accounts</Link>
                </Button>
            </div>
            <WhatCanIDo />
            <HowToGainAccess />
        </>
    )
}

export default LandingPage
