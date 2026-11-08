import axios from 'axios'
import { parse, stringify } from 'qs'
import { onResponseSuccess, onResponseError } from './interceptors'

const api = axios.create({
  baseURL: 'http://localhost:8080/api',
  paramsSerializer: {
    encode: parse,
    serialize: stringify,
  },
})

api.interceptors.response.use(onResponseSuccess, onResponseError)

api.defaults.headers.common.Accept = 'application/json'
api.defaults.headers.common['Content-Type'] = 'application/json'
api.defaults.headers.common.credentials = 'include'

export default api
