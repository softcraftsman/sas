import React, { useEffect, useState } from 'react';
import PropTypes from 'prop-types';
import Button from '@mui/material/Button'
import Box from '@mui/material/Box'
import Chip from '@mui/material/Chip'
import Dialog from '@mui/material/Dialog'
import DialogActions from '@mui/material/DialogActions'
import DialogContent from '@mui/material/DialogContent'
import DialogTitle from '@mui/material/DialogTitle'
import FormControl from '@mui/material/FormControl'
import FormLabel from '@mui/material/FormLabel'
import FormGroup from '@mui/material/FormGroup'
import Grid from '@mui/material/Grid'
import InputLabel from '@mui/material/InputLabel'
import MenuItem from '@mui/material/MenuItem'
import Select from '@mui/material/Select'
import TextField from '@mui/material/TextField'
import './DirectoryEditor.css'

const DirectoryEditor = ({ data, isNew, onCancel, onCreate, onUpdate, open }) => {
    const [formData, setFormData] = useState({})

    // Set the default form values
    useEffect(() => {
        setFormData({
            name: data.name,
            storageType: data.storageType,
            fundCode: data.fundCode,
            policy: data.policy,
            region: data.region,
            members: data.members
        })

    }, [data])


    const handleCreateClick = () => {
        onCreate && onCreate(formData)
    }


    const handleClose = () => {
        onCancel && onCancel()
    }


    const handleDeleteMember = (id, memberToDelete) => {
        const members = formData.members.filter(member => member !== memberToDelete)
        updateState(id, members)
    }


    const handleInputChange = (event) => {
        updateState(event.target.name, event.target.value)
    }


    const handleMemberTextChange = (event) => {
        if (event.key === 'Enter') {
            const members = [...formData.members, event.target.value]
            updateState(event.target.name, members)
        }
    }

    const handleUpdateClick = () => {
        onUpdate && onUpdate(formData)
    }


    const updateState = (id, value) => {
        setFormData({
            ...formData,
            [id]: value
        })
    }


    const actionButton = isNew ?
        (<Button onClick={handleCreateClick}>Create</Button>) :
        (<Button onClick={handleUpdateClick}>Update</Button>)


    const title = isNew ? 'Creating a new Space' : 'Update an existing Space'


    return (
        <Dialog onClose={handleClose} open={open} >
            <DialogTitle>{title}</DialogTitle>

            <DialogContent>
                <Grid container spacing={2}>
                    <Grid item xs={12}>
                        <TextField
                            autoFocus
                            id='name'
                            name='name'
                            label="Space's Name"
                            fullWidth
                            variant='standard'
                            defaultValue={data.name}
                            onChange={handleInputChange}
                        />
                    </Grid>
                    <Grid item xs={6}>
                        <FormControl fullWidth>
                            <InputLabel id='storageType-label'>Space's storage type</InputLabel>
                            <Select
                                labelId='storageType-label'
                                id='storageType'
                                name='storageType'
                                label="Space's storage type"
                                defaultValue={data.storageType}
                                onChange={handleInputChange}
                            >
                                <MenuItem value='Hot'>Hot</MenuItem>
                                <MenuItem value='Cold'>Cold</MenuItem>
                                <MenuItem value='Archive'>Archive</MenuItem>
                            </Select>
                        </FormControl>
                    </Grid>
                    <Grid item xs={6}>
                        <TextField
                            id='fundCode'
                            name='fundCode'
                            label='Fund code'
                            fullWidth
                            variant='standard'
                            defaultValue={data.fundCode}
                            onChange={handleInputChange}
                        />
                    </Grid>
                    <Grid item xs={6}>
                        <FormControl fullWidth>
                            <InputLabel id='policy-label'>Files policies</InputLabel>
                            <Select
                                labelId='policy-label'
                                id='policy'
                                name='policy'
                                label='Files policies'
                                defaultValue={data.policy}
                                onChange={handleInputChange}
                            >
                                <MenuItem value='3'>3 months</MenuItem>
                                <MenuItem value='6'>6 months</MenuItem>
                                <MenuItem value='9'>9 months</MenuItem>
                                <MenuItem value='12'>1 year</MenuItem>
                            </Select>
                        </FormControl>
                    </Grid>
                    <Grid item xs={6}>
                        <FormControl fullWidth>
                            <InputLabel id='region-label'>Region</InputLabel>
                            <Select
                                labelId='region-label'
                                id='region'
                                name='region'
                                label='Region'
                                defaultValue={data.region}
                                onChange={handleInputChange}
                            >
                                <MenuItem value='EastUS'>East US</MenuItem>
                                <MenuItem value='WestUS'>West US</MenuItem>
                                <MenuItem value='WestEurope'>West Europe</MenuItem>
                            </Select>
                        </FormControl>
                    </Grid>
                    <Grid item xs={12}>
                        <Box>
                            <FormControl component='fieldset' style={{ width: '100%' }}>
                                <FormLabel component='legend'>
                                    Select who has access to it
                                </FormLabel>
                                <FormGroup>
                                    <Grid item xs={12}>
                                        <TextField
                                            margin='dense'
                                            id='members'
                                            name='members'
                                            label="Search for users within Duke's directory"
                                            fullWidth
                                            variant='standard'
                                            onKeyPress={handleMemberTextChange}
                                        />
                                    </Grid>
                                    <Grid item xs={12}>
                                        {formData.members && formData.members.map(item => (
                                            <Chip
                                                key={item}
                                                className='member-chip'
                                                label={item}
                                                onDelete={() => handleDeleteMember('members', item)}
                                            />
                                        ))}
                                    </Grid>
                                </FormGroup>
                            </FormControl>
                        </Box>
                    </Grid>
                </Grid>
            </DialogContent>

            <DialogActions>
                <Button onClick={handleClose}>Cancel</Button>
                {actionButton}
            </DialogActions>
        </Dialog>
    )
}

DirectoryEditor.propTypes = {
    data: PropTypes.object,
    onCancel: PropTypes.func,
    onCreate: PropTypes.func,
    onUpdate: PropTypes.func,
    open: PropTypes.bool
}

DirectoryEditor.defaultProps = {
    data: {},
    open: false
}

export default DirectoryEditor
