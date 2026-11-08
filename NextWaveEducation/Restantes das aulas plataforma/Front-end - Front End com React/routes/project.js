import { lazy } from 'react'
import { ROUTE_PATHS } from 'commons/constants/routes-paths'

const RegisterProject = lazy(async () => await import('pages/Projects/Register'))
const EditProject = lazy(async () => await import('pages/Projects/Edit'))
const ListProject = lazy(async () => await import('pages/Projects/List'))

const project = [
  {
    path: `${ROUTE_PATHS.project}`,
    component: ListProject,
    exact: true
  },
  {
    path: `${ROUTE_PATHS.project}/novo-projeto`,
    component: RegisterProject,
    exact: true
  },
  {
    path: `${ROUTE_PATHS.project}/:idProject`,
    component: EditProject,
    exact: true
  }
]

export default project
