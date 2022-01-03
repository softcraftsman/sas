import React, { useCallback, useEffect, useState } from 'react'
import { useAuthentication } from '../../hooks/useAuthentication'
import { getStorageAccounts } from '../../services/StorageManager.service'
import { getFileSystems, getDirectories } from '../../services/DataLake.service'
import Container from '@mui/material/Container'
import Grid from '@mui/material/Grid'
import DirectoriesManager from '../DirectoriesManager'
import Selector from '../Selector'

/**
 * Renders list of Storage Accounts
 */
const StorageAccountsPage = () => {
    const { auth } = useAuthentication()

    const [selectedStorageAccount, setSelectedStorageAccount] = useState('')
    const [selectedFileSystem, setSelectedFileSystem] = useState('')

    const [storageAccounts, setStorageAccounts] = useState([])
    const [fileSystems, setFileSystems] = useState()
    const [directories, setDirectories] = useState()

    // Retrieve the list of Storage Accounts
    useEffect(() => {
        auth && auth.accessToken && getStorageAccounts(auth.accessToken)
            .then(response => {
                setStorageAccounts(response)
            })
    }, [auth])


    // Retrieve the list of File Systems for the selected Azure Data Lake Storage Account
    useEffect(() => {
        const retrieveFileSystems = async storageAccount => {
            let iter = await getFileSystems(storageAccount)
            const _fileSystems = []

            for await (const fs of iter) {
                _fileSystems.push(fs.name)
            }

            setFileSystems(_fileSystems)
        }

        selectedStorageAccount && retrieveFileSystems(selectedStorageAccount)
    }, [selectedStorageAccount])


    // Retrieve the list of Directories for the selected File System
    useEffect(() => {
        const retrieveDirectories = async (storageAccount, fileSystem) => {
            const toSpace = kb => `${kb} KB`
            const list = await getDirectories(storageAccount, fileSystem)
            const _directories = list.map(item => ({
                fundCode: '123456',
                members: ['John', 'Paul', 'George', 'Ringo'],
                monthlyCost: 'free',
                name: item.name,
                policy: '9',
                region: 'WestUS',
                spaceUsed: toSpace(item.contentLength),
                storageType: item.accessTier
            }))

            setDirectories(_directories)
        }

        selectedFileSystem && retrieveDirectories(selectedStorageAccount, selectedFileSystem)
    }, [selectedStorageAccount, selectedFileSystem])


    const handleStorageAccountChange = useCallback(id => {
        setSelectedStorageAccount(id)
    }, [])


    const handleFileSystemChange = useCallback(id => {
        setSelectedFileSystem(id)
    }, [])


    return (
        <Container>
            <h3>Storage Account</h3>
            <Grid container spacing={2} sx={{ justifyContent: 'center', marginBottom: '10px' }}>
                <Grid item md={6}>
                    <Selector id='storageAccountSelector' items={storageAccounts} label='Storage Account' onChange={handleStorageAccountChange} />
                </Grid>
                <Grid item md={6}>
                    <Selector id='fileSystemSelector' items={fileSystems} label='File System' onChange={handleFileSystemChange} />
                </Grid>
                <Grid item>
                    <DirectoriesManager data={directories} />
                </Grid>
            </Grid>
        </Container>
    )
}

export default StorageAccountsPage
