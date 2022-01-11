import React, { useState } from 'react'
import PropTypes from 'prop-types'
import Chip from '@mui/material/Chip'
import Table from 'react-bootstrap/Table'
import InfoIcon from '@mui/icons-material/InfoOutlined'
import PencilIcon from '@mui/icons-material/EditOutlined'
import './DirectoriesTable.css'
import { Popover } from '@mui/material'

export const DirectoriesTable = ({ data, onDetails, onEdit, strings }) => {
    const [anchorEl, setAnchorEl] = useState(null)

    const handlePopoverClose = () => {
        setAnchorEl(null)
    }

    const handlePopoverOpen = event => {
        setAnchorEl(event.currentTarget)
    }

    const open = Boolean(anchorEl)

    const renderMembers = members => {
        if (members.length > 2) {
            return (
                <>
                    <div onMouseEnter={handlePopoverOpen} onMouseLeave={handlePopoverClose}>
                        <Chip className='member-chip' label={`${members.length} ${strings.members}`} />
                    </div>
                    <Popover
                        anchorEl={anchorEl}
                        anchorOrigin={{ vertical: 'bottom', horizontal: 'left' }}
                        disableRestoreFocus
                        onClose={handlePopoverClose}
                        open={open}
                        sx={{ pointerEvents: 'none' }}
                        transformOrigin={{
                            vertical: 'top',
                            horizontal: 'left',
                        }}
                    >
                        <div className='members-popover'>
                            {members.map(item => (<div key={item} className='member-text'>{item}</div>))}
                        </div>
                    </Popover>
                </>
            )
        } else {
            return members.map(item => (<Chip key={item} className='member-chip' label={`${item}`} />))
        }
    }

    return (
        <Table striped bordered hover className='directoriesTable'>
            <thead>
                <tr>
                    <th>{strings.folderLabel}</th>
                    <th>{strings.spaceUsedLabel}</th>
                    <th>{strings.monthlyCostLabel}</th>
                    <th>{strings.whoHasAccessLabel}</th>
                    <th>{strings.fundCodeLabel}</th>
                    <th>{strings.actionsLabel}</th>
                </tr>
            </thead>
            <tbody>
                {data.map(row => {
                    return (
                        <tr key={row.name}>
                            <td className='name'>{row.name}</td>
                            <td className='spaceused'>{row.size}</td>
                            <td className='costs'>{row.cost}</td>
                            <td className='owner'>{renderMembers(row.userAccess)}</td>
                            <td className='fundcode'>{row.fundCode}</td>
                            <td className='actions'>
                                {onEdit && <PencilIcon onClick={() => onEdit(row)} className='action' />}
                                {onDetails && <InfoIcon onClick={() => onDetails(row)} className='action' />}
                            </td>
                        </tr>
                    )
                })}
            </tbody>
        </Table>
    )
}

DirectoriesTable.propTypes = {
    data: PropTypes.array,
    onDetails: PropTypes.func,
    onEdit: PropTypes.func,
    strings: PropTypes.shape({
        actionsLabel: PropTypes.string,
        folderLabel: PropTypes.string,
        fundCodeLabel: PropTypes.string,
        members: PropTypes.string,
        monthlyCostLabel: PropTypes.string,
        spaceUsedLabel: PropTypes.string,
        whoHasAccessLabel: PropTypes.string,
    })
}

DirectoriesTable.defaultProps = {
    data: [],
    strings: {
        actionsLabel: 'Actions',
        folderLabel: 'Folder',
        fundCodeLabel: 'Fund Code',
        members: 'members',
        monthlyCostLabel: 'Monthly Cost',
        spaceUsedLabel: 'Space Used',
        whoHasAccessLabel: 'Who Has Access?',
    }
}

export default DirectoriesTable
