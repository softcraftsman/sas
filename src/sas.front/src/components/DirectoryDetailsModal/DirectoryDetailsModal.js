import React from 'react'
import PropTypes from 'prop-types'
import Button from '@mui/material/Button'
import CancelIcon from '@mui/icons-material/CancelOutlined'
import Dialog from '@mui/material/Dialog'
import DialogActions from '@mui/material/DialogActions'
import DialogContent from '@mui/material/DialogContent'
import DialogTitle from '@mui/material/DialogTitle'
import DirectoryDetails from '../DirectoryDetails'
import ConnectDetails from '../ConnectDetails/ConnnectDetails'

const DirectoryDetailsModal = ({ data, onCancel, open, strings }) => {

    const handleClose = () => {
        onCancel && onCancel()
    }

    return (
        <Dialog onClose={handleClose} open={open} maxWidth='lg'>
            <DialogTitle>{strings.directoryDetailsTitle}</DialogTitle>

            <DialogContent>
                <DirectoryDetails data={data} strings={strings} />
                <ConnectDetails />
            </DialogContent>
            
            <DialogActions>
                <Button variant='outlined' startIcon={<CancelIcon />} onClick={handleClose}>{strings.close}</Button>
            </DialogActions>
        </Dialog>
    )
}

DirectoryDetailsModal.propTypes = {
    data: PropTypes.object,
    onCancel: PropTypes.func,
    open: PropTypes.bool,
    strings: PropTypes.shape({
        accessTierLabel: PropTypes.string,
        close: PropTypes.string,
        costLabel: PropTypes.string,
        createdLabel: PropTypes.string,
        departmentLabel: PropTypes.string,
        directoryDetailsTitle: PropTypes.string,
        folderLabel: PropTypes.string,
        fundCodeLabel: PropTypes.string,
        ownerLabel: PropTypes.string,
        regionLabel: PropTypes.string,
        sizeLabel: PropTypes.string,
        totalFilesLabel: PropTypes.string
    })
}

DirectoryDetailsModal.defaultProps = {
    data: {},
    onCancel: () => { },
    open: false,
    strings: {
        accessTierLabel: 'Storage type',
        close: 'Close',
        costLabel: 'Monthly cost',
        createdLabel: 'Created on',
        departmentLabel: 'Department',
        directoryDetailsTitle: 'Directory Details',
        folderLabel: 'Folder',
        fundCodeLabel: 'Fund code',
        ownerLabel: 'Owner',
        regionLabel: 'Region',
        sizeLabel: 'Total size',
        totalFilesLabel: 'Total files'
    }
}

export default DirectoryDetailsModal
