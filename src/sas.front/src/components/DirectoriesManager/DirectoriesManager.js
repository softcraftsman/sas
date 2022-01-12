import React, { useState } from 'react'
import PropTypes from 'prop-types'
import Button from '@mui/material/Button'
import AddIcon from '@mui/icons-material/AddOutlined'
import DirectoriesTable from '../DirectoriesTable'
import DirectoryEditorModal from '../DirectoryEditorModal'
import DirectoryDetailsModal from '../DirectoryDetailsModal'
import './DirectoriesManager.css'

const DirectoriesManager = ({ data, onCreateDirectory }) => {
    const [editor, setEditor] = useState({ show: false, data: {}, isNew: true })
    const [details, setDetails] = useState({ show: false, data: {} })

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
        onCreateDirectory && onCreateDirectory(data)
        
        // Hide the editor modal
        setEditor({ show: false, data: {}, isNew: true })
    }

    const handleDetails = (rowData) => {
        setDetails({ show: true, data: rowData })
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
            />

            {editor.show &&
                <DirectoryEditorModal
                    data={editor.data}
                    isNew={editor.isNew}
                    onCancel={handleCancelEdit}
                    onCreate={handleCreateDirectory}
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
