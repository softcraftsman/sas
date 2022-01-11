import React from 'react'
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import useAuthentication from '../../hooks/useAuthentication'

import PageLayout from '../PageLayout'
import LandingPage from '../LandingPage'
import StorageAccountsPage from '../StorageAccountsPage'

function App() {
  const { isAuthenticated } = useAuthentication()

  const content = isAuthenticated ? (
    <BrowserRouter>
      <Routes>
        <Route path='/' element={<StorageAccountsPage />} />
      </Routes>
    </BrowserRouter>
  ) : (
    <LandingPage />
  )

  return (
    <PageLayout>
      {content}
    </PageLayout>
  )
}

export default App