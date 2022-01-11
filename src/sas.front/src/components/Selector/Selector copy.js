import React, { useEffect } from "react"
import PropTypes from 'prop-types'
import { FloatingLabel, Form } from "react-bootstrap"

/**  
 * Renders list of items in a drop down selector
 */
const Selector = ({ items, id, label, onChange }) => {
    useEffect(() => {
        const selected = items[0]
        onChange && onChange(selected)
    }, [items, onChange])

    const handleChange = event => {
        onChange && onChange(event.target.value)
    }

    return (
        <FloatingLabel controlId={id} label={label}>
            <Form.Select aria-label={`Select ${label}`} onChange={handleChange}>
                {items.map(item => <option key={item} value={item}>{item}</option>)}
            </Form.Select>
        </FloatingLabel>
    )
}

Selector.propTypes = {
    id: PropTypes.string,
    items: PropTypes.array,
    label: PropTypes.string,
    onChange: PropTypes.func
}

Selector.defaultProps = {
    id: 'Selector',
    items: [],
    label: ''
}

export default Selector