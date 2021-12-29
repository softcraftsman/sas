import React, { useState } from 'react'
import MaterialTable from 'material-table'
import Alert from '@mui/material/Alert'
import Chip from '@mui/material/Chip'
import Snackbar from '@mui/material/Snackbar'
import DirectoryEditor from '../DirectoryEditor'
import './DirectoriesTable.css'

const columns = [
    {
        title: 'Space Name',
        field: 'name'
    },
    {
        title: 'Space Used',
        field: 'spaceUsed'
    },
    {
        title: 'Monthly Cost',
        field: 'monthlyCost'
    },
    {
        title: 'Who Has Access',
        field: 'members',
        render: rowData => {
            return rowData.members.map(item => (<Chip key={item} className='member-chip' label={`${item}`} />))
        }
    },
    {
        title: 'Storage Type',
        field: 'storageType'
    }
]


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
        <div className='directoriesTable'>
            <MaterialTable
                title='Your containers'
                columns={columns}
                data={data}
                actions={actions}
                options={{ actionsColumnIndex: -1, paging: false }}
            />
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

DirectoriesTable.defaultProps = {
    data: []
}

export default DirectoriesTable
