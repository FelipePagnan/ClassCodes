import React from "react";
import { TextField, Typography } from "@mui/material";
import { Field } from "formik";

function Input(props) {
  return (
    <Field {...props}>
      {({ field, meta }) => (
        <div className="mb-1">
          <TextField
            label={props.label}
            inputProps={{ ...field, ...props }}
            className="w-100"
            variant="standard"
          />
          {meta.touched && meta.error && (
            <div className="p-relative">
              <Typography className="p-absolute" variant="caption" color="error">
                {meta.error}
              </Typography>
            </div>
          )}
        </div>
      )}
    </Field>
  );
}

export default Input;
