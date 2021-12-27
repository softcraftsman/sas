import { useEffect, useState } from 'react'
import Snackbar from '@material-ui/core/Snackbar';
import IconButton from '@material-ui/core/IconButton';
import CloseIcon from '@material-ui/icons/Close';

const Toaster = ({ message }) => {
    const [messages, setMessages] = useState([])
    const [isOpen, setIsOpen] = useState(false)

    useEffect(() => {
        setMessages({ ...messages, message })
    }, [message])

    const handleClose = (event) => {
        setIsOpen(false)
    }

    return (
        <Snackbar
            open={isOpen}
            autoHideDuration={5000}
            onClose={handleClose}
            message={message}
            action={<IconButton
                size='small'
                aria-label='close'
                color='inherit'
                onClick={handleClose}
            >
                <CloseIcon fontSize='small' />
            </IconButton>}
        />
    )
}

export default Toaster
