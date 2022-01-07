import React, { useEffect } from "react"
import PropTypes from 'prop-types'
import InputLabel from "@mui/material/InputLabel"
import FormControl from "@mui/material/FormControl"
import MenuItem from "@mui/material/MenuItem"
import Select from "@mui/material/Select"

/**  
 * Renders list of items in a drop down selector
 */
const Selector = ({ items, id, onChange, selectedItem, strings }) => {
    useEffect(() => {
        const selected = items[0]
        onChange && onChange(selected)
    }, [items, onChange])

    const handleChange = event => {
        onChange && onChange(event.target.value)
    }

    return (
        <FormControl fullWidth>
            <InputLabel id={`${id}-select-label`}>{strings.label}</InputLabel>
            <Select
                labelId={`${id}-select-label`}
                id={id}
                label={strings.label}
                value={selectedItem}
                onChange={handleChange}
            >
                {items.map(item => <MenuItem key={item} value={item}>{item}</MenuItem>)}
            </Select>
        </FormControl>
    )
}

Selector.propTypes = {
    id: PropTypes.string,
    items: PropTypes.array,
    label: PropTypes.string,
    onChange: PropTypes.func,
    selectedItem: PropTypes.string,
    strings: PropTypes.shape({
        label: PropTypes.string
    })
}

Selector.defaultProps = {
    id: 'Selector',
    items: [],
    label: '',
    selectedItem: '',
    strings: {
        label: ''
    }
}

export default Selector