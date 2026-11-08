import { Button } from "@mui/material";

Button.defaultProps = {
  disableElevation: true,
  variant: "contained",
  style: {
    height: "40px",
    borderRadius: "40px",
    textTransform: "inherit",
    fontSize: "14px",
    fontWeight: "bold"
  }
};

export default Button
