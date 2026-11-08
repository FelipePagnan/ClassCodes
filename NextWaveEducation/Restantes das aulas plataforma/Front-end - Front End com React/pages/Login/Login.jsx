import React from "react";
import { useNavigate } from "react-router-dom";
import { Grid, Typography } from "@mui/material";
import { Formik, Form } from "formik";
import { ReactComponent as UserRegisterBG } from "assets/images/bg-user.svg";
import { ROUTE_PATHS } from "commons/constants/routes-paths";
import Input from "components/Form/Input";
import Layout from "containers/Layout/Layout";
import Button from "components/Button";
import { INITIAL_VALUES } from "./constants";
import { validateForm } from "./schemas";

function Login() {
  const navigate = useNavigate();

  const onSubmit = () => {
    navigate(ROUTE_PATHS.project);
  };

  return (
    <Layout image={UserRegisterBG}>
      <Formik
        onSubmit={onSubmit}
        initialValues={INITIAL_VALUES}
        validationSchema={validateForm}
      >
        {({ handleSubmit }) => (
          <Form onSubmit={handleSubmit}>
            <div className="w-100 mb-3">
              <Typography variant="h2" fontWeight="bold">
                Entre ou faça seu cadastro
              </Typography>
            </div>
            <div className="w-75 mb-3">
              <Grid container spacing={2}>
                <Grid item xs={12}>
                  <Input name="email" label="E-mail" />
                </Grid>
                <Grid item xs={12}>
                  <Input name="password" label="Senha" type="password" />
                </Grid>
              </Grid>
            </div>
            <div className="w-75 mb-3">
              <Button
                className="py-3 mb-3"
                size="large"
                type="submit"
                fullWidth
              >
                Entrar
              </Button>
              <Button
                className="py-3"
                size="large"
                variant="text"
                fullWidth
                onClick={() => navigate(`${ROUTE_PATHS.user}/novo-usuario`)}
              >
                Cadastrar
              </Button>
            </div>
          </Form>
        )}
      </Formik>
    </Layout>
  );
}

export default Login;
