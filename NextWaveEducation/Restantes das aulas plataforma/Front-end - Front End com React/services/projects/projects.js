import api from '../core/core-api'

const basepath = '/projects'

const projects = {
  getProjects() {
    return api.get(`${basepath}`)
  },
  getProjectById(projectId) {
    return api.get(`${basepath}/${projectId}`)
  },
  saveProjects(projectData) {
    return api.post(`${basepath}`, { ...projectData })
  },
  deleteProject(idProject) {
    return api.delete(`${basepath}/${idProject}`)
  },
  updateProject(idProject, projectData) {
    return api.put(`${basepath}/${idProject}`, { ...projectData })
  },
}

export default projects
