import React from "react";
import ProptTypes from "prop-types";
import { Stack } from "@mui/material";

function Layout({ children, image: Image }) {
  return (
    <Stack direction="row" justifyContent="center" alignItems="center">
      <div className="w-50 pl-12">
        <div className="pl-12">{children}</div>
      </div>
      {Image && (
        <div className="w-50">
          <Image className="w-100 h-100" />
        </div>
      )}
    </Stack>
  );
}

Layout.propTypes = {
  image: ProptTypes.elementType.isRequired,
};

export default Layout;
