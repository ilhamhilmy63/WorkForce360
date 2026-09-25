const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5170/api'
export async function request(path, options = {}) {
  const stored = localStorage.getItem('tripcraft_session') || sessionStorage.getItem('tripcraft_session')
  const token = stored ? JSON.parse(stored)?.accessToken : null
  const response = await fetch(`${API_URL}${path}`, { ...options, headers: { 'Content-Type': 'application/json', ...(token ? {Authorization:`Bearer ${token}`} : {}), ...options.headers } })
  const body = await response.json().catch(() => null)
  if (!response.ok) throw new Error(body?.detail || body?.title || 'Unable to connect. Please try again.')
  return body
}
export const authApi = {
  login: (email, password) => request('/auth/login', { method:'POST', body:JSON.stringify({email,password}) }),
  register: (fullName, email, password, profile = {}) => request('/auth/register', { method:'POST', body:JSON.stringify({fullName,email,password,...profile}) })
}
export const api = {
  get: path => request(path),
  post: (path, body) => request(path, {method:'POST', body:JSON.stringify(body)}),
  put: (path, body) => request(path, {method:'PUT', body:body===undefined?undefined:JSON.stringify(body)}),
  delete: path => request(path, {method:'DELETE'})
}
