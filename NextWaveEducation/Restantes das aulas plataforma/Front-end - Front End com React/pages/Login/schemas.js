import * as yup from "yup"

export const validateForm = yup.object().shape({
  password: yup.string().required("Campo obrigatório"),
  email: yup.string().required("Campo obrigatório"),
})
