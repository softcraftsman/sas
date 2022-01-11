import React from 'react'
import PropTypes from 'prop-types'
import Button from "@mui/material/Button"
import CloudDownload from '@mui/icons-material/CloudDownload'
import './ConnectDetails.css'

const ConnectDetails = ({ strings }) => {
    return (
        <div className='connectDetails'>
            <div className='title'>
                {strings.connectTitle}
            </div>
            <div className='cli'>
                <div className='cli-title'>{strings.cliLabel}:</div> <Button variant='text' href=''>{strings.cliLinkText}</Button>
            </div>
            <div className='storageExplorer'>
                <div className='storageExplorer-title'>
                    {strings.storageExplorerLabel}:
                </div>
                <div className='storageExplorer-steps'>
                    <div className='step1'>
                        <div>
                            {strings.step1Label}
                        </div>
                        <div>
                            <Button href='' variant='outlined' startIcon={<CloudDownload />}>{strings.download}</Button>
                        </div>
                    </div>
                    <div className='step-divider' />
                    <div className='step2'>
                        <div>
                            {strings.step2Label}
                        </div>
                        <div>
                            <img src='' alt='' height={150} width={250} />
                        </div>
                    </div>
                    <div className='step-divider' />
                    <div className='step3'>
                        <div>
                            {strings.step3Label}
                        </div>
                        <div>
                            <img src='' alt='' height={150} width={250} />
                        </div>
                    </div>
                    <div className='step-divider' />
                    <div className='step4'>
                        <div>
                            {strings.step4Label}
                        </div>
                        <div>
                            <img src='' alt='' height={150} width={250} />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    )
}

ConnectDetails.propTypes = {
    strings: PropTypes.shape({
        cliLabel: PropTypes.string,
        cliLinkText: PropTypes.string,
        connectTitle: PropTypes.string,
        download: PropTypes.string,
        step1Label: PropTypes.string,
        step2Label: PropTypes.string,
        step3Label: PropTypes.string,
        step4Label: PropTypes.string,
        storageExplorerLabel: PropTypes.string,
    })
}

ConnectDetails.defaultProps = {
    data: {},
    strings: {
        cliLabel: 'Via Azure CLI',
        cliLinkText: 'click to see',
        connectTitle: 'How to connect?',
        download: 'Download',
        step1Label: '1. Download Storage Explorer',
        step2Label: '2. In SE, right click on "Storage Accounts", under "Local and Attached"',
        step3Label: '3. Select "ADLS Gen2 container or directory',
        step4Label: '4. Select "Sign in using Azure Active Directory',
        storageExplorerLabel: 'Via Storage Explorer',
    }
}

export default ConnectDetails