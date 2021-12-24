import React, { useEffect, useState } from 'react'
import { useAuthentication } from '../../hooks/useAuthentication'
import { getStorageAccounts } from '../../services/StorageManager.service'
import { getFileSystems, getDirectories } from '../../services/DataLake.service'
import Selector from '../Selector'
import DirectoriesTable from '../DirectoriesTable/DirectoriesTable'

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
                console.log(`File System: ${fs.name}`);
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
                name: item.name,
                spaceUsed: toSpace(item.contentLength),
                monthlyCost: 'free',
                members: '?',
                storageType: item.accessTier
            }))

            setDirectories(_directories)
        }

        selectedFileSystem && retrieveDirectories(selectedStorageAccount, selectedFileSystem)
    }, [selectedStorageAccount, selectedFileSystem])

    const handleStorageAccountChange = id => {
        setSelectedStorageAccount(id)
    }

    const handleFileSystemChange = id => {
        setSelectedFileSystem(id)
    }


    return (
        <>
            <h3>Storage Account</h3>
            <Selector id='storageAccountSelector' items={storageAccounts} label='Storage Account' onChange={handleStorageAccountChange} />
            <Selector id='fileSystemSelector' items={fileSystems} label='File System' onChange={handleFileSystemChange} />
            <DirectoriesTable data={directories} />
        </>
    )
}

export default StorageAccountsPage
