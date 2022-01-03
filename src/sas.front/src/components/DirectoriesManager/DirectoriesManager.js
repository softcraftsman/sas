import React, { useState } from 'react'
import PropTypes from 'prop-types'
import Alert from '@mui/material/Alert'
import Button from 'react-bootstrap/Button'
import { Plus } from 'react-bootstrap-icons'
import Snackbar from '@mui/material/Snackbar'
import DirectoriesTable from '../DirectoriesTable'
import DirectoryEditor from '../DirectoryEditor'
import './DirectoriesManager.css'

const DirectoriesManager = ({ data }) => {
    const [editor, setEditor] = useState({ show: false, data: {}, isNew: true })
    const [toastMessage, setToastMessage] = useState()
    const [isToastOpen, setToastOpen] = useState(false)


    const handleAdd = () => {
        setEditor({ show: true, data: {}, isNew: true })
    }


    const handleCancelEdit = () => {
        setEditor({ show: false, data: {}, isNew: true })
    }


    const handleCreateDirectory = (data) => {
        // Calls the API to save the directory

        // Hide the editor modal
        setEditor({ show: false, data: {}, isNew: true })

        // Display a toast
        displayToast(`Directory '${data.name}' Created!`)
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
                <Button onClick={handleAdd}>
                    <Plus size={24} /> Add
                </Button>
            </div>

            <DirectoriesTable data={data} onAdd={handleAdd} onEdit={handleEdit} />

            {editor.show &&
                <DirectoryEditor
                    data={editor.data}
                    isNew={editor.isNew}
                    onCancel={handleCancelEdit}
                    onCreate={handleCreateDirectory}
                    onUpdate={handleUpdateDirectory}
                    open={editor.show}
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
    data: PropTypes.array
}


DirectoriesManager.defaultProps = {
    data: []
}

export default DirectoriesManager
