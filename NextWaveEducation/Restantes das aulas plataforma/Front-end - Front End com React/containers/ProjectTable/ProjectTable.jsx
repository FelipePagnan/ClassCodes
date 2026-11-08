import React from "react";
import PropTypes from "prop-types";
import { Grid, Typography } from "@mui/material";
import { SANTAS_GRAY } from "commons/constants/colors";
import TableItem from "./TableItem";
import { TableContainerStyled } from "./styles";
import { ValueColunmStyled, ActionColunmStyled } from "./styles";

function ProjectTable({ projectData, onDelete }) {
  return (
    <TableContainerStyled>
      <Grid container>
        <Grid item xs={7}>
          <Typography color={SANTAS_GRAY} fontWeight="bold">Título</Typography>
        </Grid>
        <ValueColunmStyled item xs={3}>
          <Typography color={SANTAS_GRAY} fontWeight="bold">Custo total</Typography>
        </ValueColunmStyled>
        <ActionColunmStyled item xs={2}>
          <Typography color={SANTAS_GRAY} fontWeight="bold">Ações</Typography>
        </ActionColunmStyled>
      </Grid>
      {projectData?.map(({ title, description, value, id }) => (
        <TableItem
          key={title}
          title={title}
          description={description}
          value={value}
          onDelete={onDelete}
          idProject={id}
        />
      ))}
    </TableContainerStyled>
  );
}

ProjectTable.propTypes = {
  projectData: PropTypes.arrayOf(
    PropTypes.shape({
      title: PropTypes.string.isRequired,
      description: PropTypes.string.isRequired,
      value: PropTypes.number.isRequired,
    })
  ).isRequired,
  onDelete: PropTypes.func.isRequired
};

export default ProjectTable;
