import React, { useEffect } from "react"
import PropTypes from 'prop-types'
import { FloatingLabel, Form } from "react-bootstrap"

/**
 * Renders list of Storage Accounts
 */
const StorageAccountSelector = ({ accounts, onChange }) => {

    useEffect(() => {
        const selected = accounts[0] && accounts[0].id
        onChange && onChange(selected)
    }, [accounts, onChange])

    const handleChange = event => {
        onChange && onChange(event.target.value)
    }

    return (
        <FloatingLabel controlId="storageAccountSelector" label="Storage Account">
            <Form.Select aria-label="Select Storage Account" onChange={handleChange}>
                {accounts.map(item => <option key={item.id} value={item.id}>{item.name}</option>)}
            </Form.Select>
        </FloatingLabel>
    )
}

StorageAccountSelector.propTypes = {
    accounts: PropTypes.array,
    onChange: PropTypes.func
}

export default StorageAccountSelector
