import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import ProjectServices from "services/projects";
import { ROUTE_PATHS } from "commons/constants/routes-paths";
import PageTitle from "components/PageTitle";
import ProjectTable from "containers/ProjectTable";

function ListProject() {
  const navigate = useNavigate();
  const [projectList, setProjectList] = useState([]);

  const getProjects = () => {
    ProjectServices.getProjects()
      .then(({ data }) => {
        setProjectList(data)
      })
      .catch((error) => console.log("Erro na requisição: " , error))
  }

  useEffect(() => {
    getProjects()
  }, [])

  const actionButtonProps = {
    label: "Novo projeto",
    action: () => navigate(`${ROUTE_PATHS.project}/novo-projeto`),
  };

  const onDelete = (idProject) => {
    ProjectServices.deleteProject(idProject)
      .then(() => {
        getProjects()
      })
      .catch((error) => console.log("error", error))
  }

  return (
    <div style={{ display: "flex", justifyContent: "center", width: "100%" }}>
      <div style={{ width: "800px", paddingTop: "48px" }}>
        <PageTitle title="Projetos" actionButton={actionButtonProps} />
        <ProjectTable projectData={projectList} onDelete={onDelete} />
      </div>
    </div>
  );
}

export default ListProject;
