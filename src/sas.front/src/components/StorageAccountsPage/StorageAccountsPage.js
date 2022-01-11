import React, { useCallback, useEffect, useState } from 'react'
import PropTypes from 'prop-types'
import useAuthentication from '../../hooks/useAuthentication'
import { getFileSystems, getDirectories } from '../../services/StorageManager.service'
import Container from '@mui/material/Container'
import Grid from '@mui/material/Grid'
import DirectoriesManager from '../DirectoriesManager'
import Selector from '../Selector'

/**
 * Renders list of Storage Accounts
 */
const StorageAccountsPage = ({ strings }) => {
    const { isAuthenticated } = useAuthentication()

    const [selectedStorageAccount, setSelectedStorageAccount] = useState('')
    const [selectedFileSystem, setSelectedFileSystem] = useState('')

    const [storageAccounts, setStorageAccounts] = useState([])
    const [directories, setDirectories] = useState()


    // Retrieve the list of File Systems for the selected Azure Data Lake Storage Account
    useEffect(() => {
        const retrieveStorageAccounts = async () => {
            try {
                const results = await getFileSystems()
                setStorageAccounts(results)
            }
            catch (error) {
                console.log(error)
            }
        }

        isAuthenticated && retrieveStorageAccounts()
    }, [isAuthenticated])


    // Retrieve the list of Directories for the selected File System
    useEffect(() => {
        const retrieveDirectories = async (storageAccount, fileSystem) => {
            //const toSpace = kb => `${kb} KB`

            try {
                const _directories = await getDirectories(storageAccount, fileSystem)
                setDirectories(_directories)
            }
            catch (error) {
                console.log(error)
            }
        }

        selectedFileSystem && retrieveDirectories(selectedStorageAccount, selectedFileSystem)
    }, [selectedStorageAccount, selectedFileSystem])


    const handleStorageAccountChange = useCallback(id => {
        setSelectedStorageAccount(id)
    }, [])


    const handleFileSystemChange = useCallback(id => {
        setSelectedFileSystem(id)
    }, [])


    const storageAccountItems = storageAccounts.map(account => account.name)
    const fileSystemItems = selectedStorageAccount ? storageAccounts.find(account => account.name === selectedStorageAccount).items.map(item => item.name) : []


    return (
        <Container>
            <Grid container spacing={2} sx={{ justifyContent: 'center', marginBottom: '10px' }}>
                <Grid item md={6}>
                    <Selector
                        id='storageAccountSelector'
                        items={storageAccountItems}
                        onChange={handleStorageAccountChange}
                        selectedItem={selectedStorageAccount}
                        strings={{ label: strings.storageAccountLabel }}
                    />
                </Grid>
                <Grid item md={6}>
                    <Selector
                        id='fileSystemSelector'
                        items={fileSystemItems}
                        onChange={handleFileSystemChange}
                        selectedItem={selectedFileSystem}
                        strings={{ label: strings.fileSystemLabel }}
                    />
                </Grid>
                <Grid item>
                    <DirectoriesManager
                        data={directories}
                        storageAccount={selectedStorageAccount}
                        fileSystem={selectedFileSystem} />
                </Grid>
            </Grid>
        </Container>
    )
}

StorageAccountsPage.propTypes = {
    strings: PropTypes.shape({
        fileSystemLabel: PropTypes.string,
        storageAccountLabel: PropTypes.string
    })
}

StorageAccountsPage.defaultProps = {
    strings: {
        fileSystemLabel: 'File System',
        storageAccountLabel: 'Storage Account'
    }
}

export default StorageAccountsPage
