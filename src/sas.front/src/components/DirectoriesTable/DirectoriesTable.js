import React from 'react'
import PropTypes from 'prop-types'
import Chip from '@mui/material/Chip'
import Table from 'react-bootstrap/Table'
import { Pencil } from 'react-bootstrap-icons';
import './DirectoriesTable.css'

export const DirectoriesTable = ({ data, onEdit }) => {
    return (
        <Table striped bordered hover className='directoriesTable'>
            <thead>
                <tr>
                    <th>Space Name</th>
                    <th>Space Used</th>
                    <th>Monthly Cost</th>
                    <th>Who Has Access</th>
                    <th>Storage Type</th>
                    <th>Actions</th>
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
                                <Pencil onClick={() => onEdit(row)} className='action' />
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
    onAdd: PropTypes.func,
    onEdit: PropTypes.func,
}

DirectoriesTable.defaultProps = {
    data: [],
    onAdd: () => { },
    onEdit: () => { }
}

export default DirectoriesTable
