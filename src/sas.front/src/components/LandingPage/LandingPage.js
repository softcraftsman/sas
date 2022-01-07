import React from "react"
import Button from '@mui/material/Button'
import Link from "@mui/material/Link"
import LogInButton from "../LogInButton"
import './LandingPage.css'

const LandingPage = () => {
    return (
        <div className='landingpage'>
            <div className='access'>
                <div className='notes'>
                    To have access to the storage as a service platform, you need to use your corporative credentials.
                    If you have it handy, please click on the "Log in" button below.  If you don't, please click on
                    the "How to gain access" link below.
                </div>
                <div className='login'>
                    <LogInButton text='Log in' />
                </div>
                <div className='link'>
                    <Link href=''>How to gain access</Link>
                </div>
            </div>
            <div className='divider' />
            <div className='three'>
                <h5>
                    What you can do here?
                </h5>
                <ol className='cando'>
                    <li>Ask for a new space to store your data files in a highly scalable way in the cloud.</li>
                    <li>Manage who has access to the space you are creating.</li>
                    <li>Have a view about capacity of your space, cost and more.</li>
                    <li>Move data between different layers as the data changes in terms of priority.</li>
                    <li>Decommission the storage when it is no longer needed.</li>
                </ol>
            </div>
        </div>
    )
}

export default LandingPage
