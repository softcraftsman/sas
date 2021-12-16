import React, { useEffect, useState } from "react"
import { useAuthentication } from "../../hooks/useAuthentication"
import { getStorageAccounts } from "../../services/StorageAccounts"
import { getContainers } from "../../services/StorageAccountSdk"
import StorageAccountSelector from "../StorageAccountSelector"
import FileSystems from "../FileSystems"

/**
 * Renders list of Storage Accounts
 */
const StorageAccountsPage = () => {
    const { managementToken } = useAuthentication()
    const [storageAccounts, setStorageAccounts] = useState([])
    const [selectedStorageAccount, setSelectedStorageAccount] = useState('')
    const [fileSystems, setFileSystems] = useState()

    // Retrieve the list of Storage Accounts
    useEffect(() => {
        managementToken && getStorageAccounts(managementToken)
            .then(response => {
                const supportedStorageAccounts = response.value.filter((item) => {
                    // Only interested in Azure Data Lake Storage Gen2 accounts that have Hierarchial Namespace enabled
                    return item.kind === 'StorageV2' && item.properties.isHnsEnabled
                })

                setStorageAccounts(supportedStorageAccounts)
            })
    }, [managementToken])


    // Retrieve the list of File Systems
    useEffect(() => {
        const retrieveFileSystems = async id => {
            const iter = await getContainers(selectedStorageAccount)
            const _fileSystems = []
            let i = 1
            let container = await iter.next()

            while (!container.done) {
                console.log(`Container ${i++}: ${container.value.name}`);
                _fileSystems.push(container.value.name)
                container = await iter.next()
            }

            setFileSystems(_fileSystems)
        }

        selectedStorageAccount && retrieveFileSystems()
    }, [selectedStorageAccount])


    const handleStorageAccountChange = id => {
        setSelectedStorageAccount(id)
    }


    return (
        <>
            <h3>Storage Account</h3>
            <StorageAccountSelector accounts={storageAccounts} onChange={handleStorageAccountChange} />
            <FileSystems items={fileSystems} />
        </>
    )
}

export default StorageAccountsPage
