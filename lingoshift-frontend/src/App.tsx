import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import theme from './styles/theme';

import HomePage from './pages/HomePage';
import SettingsPage from './pages/SettingsPage';

const queryClient = new QueryClient();
function App() {
  return (
    <>
      <h1>Vite + React</h1>
    </>
  )
}

export default App
