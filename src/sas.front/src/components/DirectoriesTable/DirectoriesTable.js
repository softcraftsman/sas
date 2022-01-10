import React from 'react'
import PropTypes from 'prop-types'
import Chip from '@mui/material/Chip'
import Table from 'react-bootstrap/Table'
import InfoIcon from '@mui/icons-material/InfoOutlined'
import PencilIcon from '@mui/icons-material/EditOutlined'
import './DirectoriesTable.css'

export const DirectoriesTable = ({ data, onDetails, onEdit, strings }) => {
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
                            <td className='owner'>{row.userAccess.map(item => (<Chip key={item} className='member-chip' label={`${item}`} />))}</td>
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
        folderLabel: PropTypes.string,
        spaceUsedLabel: PropTypes.string,
        monthlyCostLabel: PropTypes.string,
        whoHasAccessLabel: PropTypes.string,
        fundCodeLabel: PropTypes.string,
        actionsLabel: PropTypes.string,
    })
}

DirectoriesTable.defaultProps = {
    data: [],
    strings: {
        folderLabel: 'Folder',
        spaceUsedLabel: 'Space Used',
        monthlyCostLabel: 'Monthly Cost',
        whoHasAccessLabel: 'Who Has Access?',
        fundCodeLabel: 'Fund Code',
        actionsLabel: 'Actions',
    }
}

export default DirectoriesTable
