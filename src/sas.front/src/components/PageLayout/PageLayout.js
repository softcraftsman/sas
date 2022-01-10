import React from "react"
import CssBaseline from '@mui/material/CssBaseline';
import Container from '@mui/material/Container'
import Header from "../Header"
import Footer from "../Footer"
import './PageLayout.css'

/**
 * Renders the navbar component with a sign-in button if a user is not authenticated
 */
export const PageLayout = ({ children }) => {
    return (
        <>
            <CssBaseline />
            <div className='Page'>
                <Header />
                <div className='sectiondivider' />
                <Container className='Content'>
                    {children}
                </Container>
                <div className='sectiondivider' />
                <Footer company='Duke University' />
            </div>
        </>
    )
}
