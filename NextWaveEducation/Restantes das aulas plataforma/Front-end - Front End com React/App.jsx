import React from "react";
import { BrowserRouter } from "react-router-dom";
import AppRouter from "containers/App/AppRouter";
import AppTheme from "containers/App/AppTheme";

function App() {
  return (
    <AppTheme>
      <BrowserRouter>
        <AppRouter />
      </BrowserRouter>
    </AppTheme>
  );
}

export default App;
