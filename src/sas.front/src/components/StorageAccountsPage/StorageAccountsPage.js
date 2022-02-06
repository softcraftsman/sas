import React, { useCallback, useEffect, useState } from 'react'
import PropTypes from 'prop-types'
import useAuthentication from '../../hooks/useAuthentication'
import { getFileSystems, getDirectories } from '../../services/StorageManager.service'
import { createFolder } from '../../services/StorageManager.service'
import Alert from '@mui/material/Alert'
import Container from '@mui/material/Container'
import Grid from '@mui/material/Grid'
import Snackbar from '@mui/material/Snackbar'
import DirectoriesManager from '../DirectoriesManager'
import Selector from '../Selector'

/**
 * Renders list of Storage Accounts
 */
const StorageAccountsPage = ({ strings }) => {
	const { account, isAuthenticated } = useAuthentication()

	const [selectedStorageAccount, setSelectedStorageAccount] = useState('')
	const [selectedFileSystem, setSelectedFileSystem] = useState('')

	const [storageAccounts, setStorageAccounts] = useState([])
	const [directories, setDirectories] = useState([])

	const [toastMessage, setToastMessage] = useState()
	const [isToastOpen, setToastOpen] = useState(false)

	// Retrieve the list of File Systems for the selected Azure Data Lake Storage Account
	useEffect(() => {
		const retrieveStorageAccounts = async () => {
			try {
				const _storageAccounts = await getFileSystems()
				setStorageAccounts(_storageAccounts)
			}
			catch (error) {
				console.error(error);
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
				console.error(error);
			}
		}

		selectedFileSystem && retrieveDirectories(selectedStorageAccount, selectedFileSystem)
	}, [selectedStorageAccount, selectedFileSystem])


	const displayToast = message => {
		setToastMessage(message)
		setToastOpen(true)
	}


	const handleCreateDirectory = (data) => {
		// Calls the API to save the directory
		createFolder(selectedStorageAccount, selectedFileSystem, account.userDetails, data)
			.then((newDirectory) => {
				// TODO: Match sort order to sort order from API (by URI)
				const _directories = [...directories, newDirectory].sort((a, b) => a.name < b.name ? -1 : a.name > b.name ? 1 : 0)
				setDirectories(_directories)

				// Display a toast
				displayToast(strings.directoryCreated(newDirectory.name))
			})
			.catch(error => {
				console.error(error)
			})
	}


	const handleStorageAccountChange = useCallback(id => {
		setSelectedStorageAccount(id)
	}, [])


	const handleFileSystemChange = useCallback(id => {
		setSelectedFileSystem(id)
	}, [])


	const storageAccountItems = storageAccounts.map(account => account.name)
	const fileSystemItems = selectedStorageAccount ? storageAccounts.find(account => account.name === selectedStorageAccount).fileSystems : []


	return (
		<>
			<Container>
				<Grid container spacing={2} sx={{ justifyContent: 'center', marginBottom: '10px' }}>
					<Grid item md={6}>
						<Selector
							id='storageAccountSelector'
							items={storageAccountItems}
							label={strings.storageAccountLabel}
							onChange={handleStorageAccountChange}
							selectedItem={selectedStorageAccount}
						/>
					</Grid>
					<Grid item md={6}>
						<Selector
							id='fileSystemSelector'
							items={fileSystemItems}
							label={strings.fileSystemLabel}
							onChange={handleFileSystemChange}
							selectedItem={selectedFileSystem}
						/>
					</Grid>
					<Grid item>
						<DirectoriesManager
							data={directories}
							onCreateDirectory={handleCreateDirectory}
							strings={strings}
						/>
					</Grid>
				</Grid>
			</Container>

			<Snackbar
				open={isToastOpen}
				anchorOrigin={{ vertical: 'bottom', horizontal: 'left' }}
				autoHideDuration={5000}
				onClose={() => setToastOpen(false)}
			>
				<Alert severity='success'>{toastMessage}</Alert>
			</Snackbar>
		</>
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
