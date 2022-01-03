import React, { useState } from 'react'
import Table from 'react-bootstrap/Table'
import Alert from '@mui/material/Alert'
import Chip from '@mui/material/Chip'
import Snackbar from '@mui/material/Snackbar'
import DirectoryEditor from '../DirectoryEditor'
import { Pencil } from 'react-bootstrap-icons';
import './DirectoriesTable.css'

export const DirectoriesTable = ({ data }) => {
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


    const handleEdit = (event, rowData) => {
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


    const actions = [
        {
            icon: 'addbox',
            toolTip: 'New Space',
            onClick: handleAdd,
            isFreeAction: true,
        },
        {
            icon: 'edit',
            toolTip: 'Edit Space',
            onClick: handleEdit
        }
    ]

    return (
        <>
            <Table striped bordered hover className='directoriesTable'>
                <thead>
                    <tr>
                        <th>Space Name</th>
                        <th>Space Used</th>
                        <th>Monthly Cost</th>
                        <th>Who Has Access</th>
                        <th>Storage Type</th>
                        <th>&nbsp;</th>
                    </tr>
                </thead>
                <tbody>
                    {data.map(row => {
                        return (
                            <tr key={row.name}>
                                <td>{row.name}</td>
                                <td>{row.spaceUsed}</td>
                                <td>{row.monthlyCost}</td>
                                <td>{row.members.map(item => (<Chip key={item} className='member-chip' label={`${item}`} />))}</td>
                                <td>{row.storageType}</td>
                                <td>
                                    <Pencil onClick={() => handleEdit(null, row)} className='action' />
                                </td>
                            </tr>
                        )
                    })}
                </tbody>
            </Table>
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
        </>
    )
}

DirectoriesTable.defaultProps = {
    data: []
}

export default DirectoriesTable
