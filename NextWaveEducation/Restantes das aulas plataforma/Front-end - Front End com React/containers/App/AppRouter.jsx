import React from "react";
import { Routes, Route } from "react-router-dom";
import AppLayout from "./AppLayout";
import listRoutes from "routes";

function AppRouter() {
  return (
    <AppLayout>
      <Routes>
        <>
          {listRoutes.map(({ path, component, exact }) => {
            const Component = component;

            return (
              <Route
                key={path}
                path={path}
                exact={exact}
                element={
                  <React.Suspense fallback={<>Carregando...</>}>
                    <Component />
                  </React.Suspense>
                }
              />
            );
          })}
        </>
      </Routes>
    </AppLayout>
  );
}

export default AppRouter;
