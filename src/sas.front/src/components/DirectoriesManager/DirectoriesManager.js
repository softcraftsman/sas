import React, { useState } from 'react'
import PropTypes from 'prop-types'
import useAuthentication from '../../hooks/useAuthentication'
import Alert from '@mui/material/Alert'
import Button from '@mui/material/Button'
import AddIcon from '@mui/icons-material/AddOutlined'
import Snackbar from '@mui/material/Snackbar'
import DirectoriesTable from '../DirectoriesTable'
import DirectoryEditorModal from '../DirectoryEditorModal'
import DirectoryDetailsModal from '../DirectoryDetailsModal'
import { createFolder } from '../../services/StorageManager.service'
import './DirectoriesManager.css'

const DirectoriesManager = ({ data, storageAccount, fileSystem }) => {
    const { auth } = useAuthentication()

    const [editor, setEditor] = useState({ show: false, data: {}, isNew: true })
    const [details, setDetails] = useState({ show: false, data: {} })

    const [toastMessage, setToastMessage] = useState()
    const [isToastOpen, setToastOpen] = useState(false)


    const handleAdd = () => {
        setEditor({ show: true, data: {}, isNew: true })
    }


    const handleCancelDetails = () => {
        setDetails({ show: false, data: {} })
    }


    const handleCancelEdit = () => {
        setEditor({ show: false, data: {}, isNew: true })
    }
    

    const handleCreateDirectory = (data) => {
        // Calls the API to save the directory
        createFolder(auth.accessToken, data, storageAccount, fileSystem)
            .then(() => { })
            .catch(error => console.log(error))

        // Hide the editor modal
        setEditor({ show: false, data: {}, isNew: true })

        // Display a toast
        displayToast(`Directory '${data.name}' Created!`)
    }


    const handleDetails = (rowData) => {
        setDetails({ show: true, data: rowData })
    }


    const handleEdit = (rowData) => {
        setEditor({ show: true, data: rowData, isNew: false })
    }


    const handleUpdateDirectory = (data) => {
        // Calls the API to save the directory

        // Hide the editor modal
        setEditor({ show: false, data: {}, isNew: true })

        // Display a toast
        displayToast(`Directory '${data.name}' Updated!`)
    }


    const displayToast = message => {
        setToastMessage(message)
        setToastOpen(true)
    }

    return (
        <div className='directoriesManager'>
            <div className='actionsBar'>
                <Button variant='contained' startIcon={<AddIcon />} onClick={handleAdd}>
                    New Folder
                </Button>
            </div>

            <DirectoriesTable
                data={data}
                onAdd={handleAdd}
                onDetails={handleDetails}
                onEdit={handleEdit} />

            {editor.show &&
                <DirectoryEditorModal
                    data={editor.data}
                    isNew={editor.isNew}
                    onCancel={handleCancelEdit}
                    onCreate={handleCreateDirectory}
                    onUpdate={handleUpdateDirectory}
                    open={editor.show}
                />
            }

            {details.show &&
                <DirectoryDetailsModal
                    data={details.data}
                    onCancel={handleCancelDetails}
                    open={details.show}
                />
            }

            <Snackbar
                open={isToastOpen}
                anchorOrigin={{ vertical: 'bottom', horizontal: 'left' }}
                autoHideDuration={5000}
                onClose={() => setToastOpen(false)}
            >
                <Alert severity="success">{toastMessage}</Alert>
            </Snackbar>
        </div>
    )
}


DirectoriesManager.propTypes = {
    data: PropTypes.array,
    storageAccount: PropTypes.string,
    fileSystem: PropTypes.string
}


DirectoriesManager.defaultProps = {
    data: []
}

export default DirectoriesManager
