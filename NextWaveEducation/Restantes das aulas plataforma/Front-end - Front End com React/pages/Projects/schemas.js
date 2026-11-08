import * as yup from 'yup'

export const validateForm = yup.object().shape({
  title: yup.string().required("Campo obrigatório"),
  value: yup.number("É necessário um número inteiro").required("Campo obrigatório"),
  description: yup.string().required("Campo obrigatório")
})
