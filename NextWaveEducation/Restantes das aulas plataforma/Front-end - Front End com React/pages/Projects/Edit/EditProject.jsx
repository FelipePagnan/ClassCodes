import React, { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { Grid, Typography } from "@mui/material";
import { Formik, Form } from "formik";
import { ReactComponent as EditProjectBG } from "assets/images/bg-edit-project.svg";
import ProjectServices from "services/projects";
import { ROUTE_PATHS } from "commons/constants/routes-paths";
import Layout from "containers/Layout/Layout";
import Button from "components/Button";
import Input from "components/Form/Input";
import { INITIAL_VALUES } from "../constants";
import { validateForm } from "../schemas";

function EditProject() {
  const navigate = useNavigate();
  const { idProject } = useParams()
  const [selectedProject, setSelectedProject] = useState();

  useEffect(() => {
    ProjectServices.getProjectById(idProject)
      .then(({ data }) => {
        setSelectedProject(data)
      })
      .catch((error) => console.log("Erro na requisição: " , error))
  }, [idProject])

  const onSubmit = (values) => {
    ProjectServices.updateProject(idProject, { ...values, value: parseInt(values.value) })
      .then(() => {
        navigate(ROUTE_PATHS.project);
      })
      .catch(error => alert(error.message))
  };

  return (
    <Layout image={EditProjectBG}>
      <Formik
        onSubmit={onSubmit}
        initialValues={selectedProject ?? INITIAL_VALUES}
        validationSchema={validateForm}
        enableReinitialize
      >
        {({ handleSubmit }) => (
          <Form onSubmit={handleSubmit}>
            <div className="w-75 mb-3">
              <Typography variant="h2" fontWeight="bold">
                Editar projeto
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
                Salvar
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

export default EditProject;
