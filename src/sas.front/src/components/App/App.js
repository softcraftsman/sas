import React from 'react'
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { AuthenticatedTemplate, UnauthenticatedTemplate } from '@azure/msal-react'

import PageLayout from '../PageLayout'
import LandingPage from '../LandingPage'
import StorageAccountsPage from '../StorageAccountsPage'

function App() {
  return (
    <PageLayout>
      <AuthenticatedTemplate>
        <BrowserRouter>
          <Routes>
            <Route path='/' element={<StorageAccountsPage />} />
          </Routes>
        </BrowserRouter>
      </AuthenticatedTemplate>
      <UnauthenticatedTemplate>
        <LandingPage />
      </UnauthenticatedTemplate>
    </PageLayout>
  )
}

export default App