import React from "react";
import { useNavigate } from "react-router-dom";
import { Grid, Typography } from "@mui/material";
import { Formik, Form } from "formik";
import { ReactComponent as RegisterProjectBG } from "assets/images/bg-register-project.svg";
import ProjectServices from 'services/projects'
import { ROUTE_PATHS } from "commons/constants/routes-paths";
import Layout from "containers/Layout/Layout";
import Button from "components/Button";
import Input from "components/Form/Input";
import { INITIAL_VALUES } from '../constants'
import { validateForm } from '../schemas'

function RegisterProject() {
  const navigate = useNavigate();

  const onSubmit = (values) => {
    ProjectServices.saveProjects({ ...values, value: parseInt(values.value) })
      .then(() => {
        navigate(ROUTE_PATHS.project);
      })
      .catch(error => alert(error.message))
  };

  return (
    <Layout image={RegisterProjectBG}>
      <Formik
        onSubmit={onSubmit}
        initialValues={INITIAL_VALUES}
        validationSchema={validateForm}
      >
        {({ handleSubmit }) => (
          <Form onSubmit={handleSubmit}>
            <div className="w-75 mb-3">
              <Typography variant="h2" fontWeight="bold">
                Vamos cadastrar seu novo projeto!
              </Typography>
            </div>
            <div className="w-75 mb-3">
            <Grid container spacing={2}>
                <Grid item xs={12}>
                  <Input name="title" label="Título" />
                </Grid>
                <Grid item xs={12}>
                  <Input name="value" label="Custo total" />
                </Grid>
                <Grid item xs={12}>
                  <Input name="description" label="Descrição" />
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
                Cadastrar
              </Button>
              <Button
                className="py-3"
                size="large"
                variant="text"
                onClick={() => navigate(ROUTE_PATHS.project)}
                fullWidth
              >
                Voltar
              </Button>
            </div>
          </Form>
        )}
      </Formik>
    </Layout>
  );
}

export default RegisterProject;
