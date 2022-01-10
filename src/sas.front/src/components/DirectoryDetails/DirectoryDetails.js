import React from 'react'
import PropTypes from 'prop-types'
import Grid from '@mui/material/Grid'
import './DirectoryDetails.css'

const DirectoryDetails = ({ data, strings }) => {

    return (
        <Grid container className='directoryDetails'>
            <Grid item md={12} className='title'>
                {strings.folderLabel}: {data.name}
            </Grid>
            <Grid item md={4}>
                {strings.totalFilesLabel}: {data.totalFiles}
            </Grid>
            <Grid item md={4}>
                {strings.departmentLabel}: {data.department}
            </Grid>
            <Grid item md={4}>
                {strings.fundCodeLabel}: {data.fundCode}
            </Grid>
            <Grid item md={4}>
                {strings.createdLabel}: {data.createdDate}
            </Grid>
            <Grid item md={4}>
                {strings.ownerLabel}: {data.owner}
            </Grid>
            <Grid item md={4}>
                {strings.costLabel}: {data.cost}
            </Grid>
            <Grid item md={4}>
                {strings.accessTierLabel}: {data.accessTier}
            </Grid>
            <Grid item md={4}>
                {strings.regionLabel}: {data.region}
            </Grid>
            <Grid item md={4}>
                {strings.sizeLabel}: {data.size}
            </Grid>
        </Grid>
    )
}

DirectoryDetails.propTypes = {
    data: PropTypes.object,
    strings: PropTypes.shape({
        accessTierLabel: PropTypes.string,
        costLabel: PropTypes.string,
        createdLabel: PropTypes.string,
        departmentLabel: PropTypes.string,
        folderLabel: PropTypes.string,
        fundCodeLabel: PropTypes.string,
        ownerLabel: PropTypes.string,
        regionLabel: PropTypes.string,
        sizeLabel: PropTypes.string,
        totalFilesLabel: PropTypes.string
    })
}

DirectoryDetails.defaultProps = {
    data: {},
    strings: {
        accessTierLabel: 'Storage type',
        costLabel: 'Monthly cost',
        createdLabel: 'Created on',
        departmentLabel: 'Department',
        folderLabel: 'Folder',
        fundCodeLabel: 'Fund code',
        ownerLabel: 'Owner',
        regionLabel: 'Region',
        sizeLabel: 'Total size',
        totalFilesLabel: 'Total files'
    }
}

export default DirectoryDetails
