import React from 'react'
import { ThemeProvider as StyledThemeProvider } from 'styled-components'
import { ThemeProvider, createTheme } from '@mui/material'
import CssBaseline from '@mui/material/CssBaseline'
import baseTheme from 'commons/styles/theme'
import GlobalStyle from 'commons/styles/global-styles'

export const theme = createTheme(baseTheme)

function AppTheme ({ children }) {
  return (
    <StyledThemeProvider theme={theme}>
      <ThemeProvider theme={theme}>
        <GlobalStyle />
        <CssBaseline />
        {children}
      </ThemeProvider>
    </StyledThemeProvider>
  )
}

export default AppTheme