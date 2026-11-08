export const onResponseSuccess = (response) => {
  return {
    data: response.data
  }
}

export const onResponseError = (error) => {
  return Promise.reject(error?.response?.data)
}
