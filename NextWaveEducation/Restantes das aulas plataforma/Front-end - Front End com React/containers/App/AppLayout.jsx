import React from "react";
import Header from "components/Header";

function AppLayout({ children }) {
  return (
    <>
      <Header />
      {children}
    </>
  );
}

export default AppLayout;
