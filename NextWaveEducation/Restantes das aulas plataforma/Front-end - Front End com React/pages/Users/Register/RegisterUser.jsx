import React from "react";
import { useNavigate } from "react-router-dom";
import {
  FormControl,
  Typography,
  RadioGroup,
  FormControlLabel,
  Radio,
  TextField,
} from "@mui/material";
import { ReactComponent as UserRegisterBG } from "assets/images/bg-user.svg";
import { ROUTE_PATHS } from "commons/constants/routes-paths";
import Layout from "containers/Layout/Layout";
import Button from "components/Button";

function RegisterUser() {
  const navigate = useNavigate();

  return (
    <Layout image={UserRegisterBG}>
      <div className="w-100 mb-3">
        <Typography variant="h2" fontWeight="bold">
          Olá!
        </Typography>
        <Typography variant="h4" fontWeight="bold">
          Vamos Criar seu cadastro.
        </Typography>
      </div>
      <div className="w-100 mb-3">
        <Typography variant="body1" fontWeight="bold">
          Qual o seu perfil?
        </Typography>
        <FormControl>
          <RadioGroup row>
            <FormControlLabel value="dev" control={<Radio />} label="Dev" />
            <FormControlLabel
              value="customer"
              control={<Radio />}
              label="Cliente"
            />
          </RadioGroup>
        </FormControl>
      </div>
      <div className="w-75 mb-3">
        <TextField
          className="w-100 mb-2"
          label="Nome completo"
          variant="standard"
        />
        <TextField
          className="w-100 mb-2"
          label="Data de nacimento"
          variant="standard"
        />
        <TextField className="w-100 mb-2" label="Email" variant="standard" />
        <TextField className="w-100 mb-2" label="Senha" variant="standard" />
      </div>
      <div className="w-75 mb-3">
        <Button
          className="py-3"
          size="large"
          fullWidth
          onClick={() => navigate(ROUTE_PATHS.project)}
        >
          Cadastrar
        </Button>
      </div>
    </Layout>
  );
}

export default RegisterUser;
