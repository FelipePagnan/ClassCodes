import { lazy } from 'react'
import { ROUTE_PATHS } from 'commons/constants/routes-paths'

const UserRegister = lazy(async () => await import('pages/Users/Register'))

const user = [
  {
    path: `${ROUTE_PATHS.user}/novo-usuario`,
    component: UserRegister,
    exact: true
  }
]

export default user
