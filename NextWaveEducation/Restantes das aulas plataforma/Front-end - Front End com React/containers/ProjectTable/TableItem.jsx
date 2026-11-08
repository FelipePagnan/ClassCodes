import React from "react";
import PropTypes from "prop-types";
import { IconButton, Grid, Typography } from "@mui/material";
import { useNavigate } from 'react-router-dom'
import { ReactComponent as PencilIcon } from "assets/images/icon-pencil.svg";
import { ReactComponent as TrashIcon } from "assets/images/icon-trash.svg";
import { formatCurrency } from "commons/utils/money";
import { ROUTE_PATHS } from "commons/constants/routes-paths";
import {
  TableLineStyled,
  ValueColunmStyled,
  ActionColunmStyled,
} from "./styles";

function TableItem({ title, description, value, onDelete, idProject }) {
  const navigate = useNavigate()

  return (
    <TableLineStyled container>
      <Grid item xs={7}>
        <Typography variant="h6" fontWeight="bold" gutterBottom>
          {title}
        </Typography>
        <Typography variant="body1">{description}</Typography>
      </Grid>
      <ValueColunmStyled item xs={3}>
        <Typography variant="body1" fontWeight="bold">
          {formatCurrency(value)}
        </Typography>
      </ValueColunmStyled>
      <ActionColunmStyled item xs={2}>
        <IconButton onClick={() => navigate(`${ROUTE_PATHS.project}/${idProject}`)}>
          <PencilIcon />
        </IconButton>
        <IconButton onClick={() => onDelete(idProject)}>
          <TrashIcon />
        </IconButton>
      </ActionColunmStyled>
    </TableLineStyled>
  );
}

TableItem.propTypes = {
  title: PropTypes.string.isRequired,
  description: PropTypes.string.isRequired,
  value: PropTypes.number.isRequired,
  idProject: PropTypes.string.isRequired,
  onDelete: PropTypes.func.isRequired
};

export default TableItem;
