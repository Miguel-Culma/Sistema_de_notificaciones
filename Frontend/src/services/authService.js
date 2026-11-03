import { api } from './api'
import { pick } from './normalize'

// POST /api/Auth/login — devuelve el token JWT.
export async function login(username, password) {
  const data = await api.post('/api/Auth/login', { username, password }, { auth: false })
  const token = typeof data === 'string' ? data : pick(data, 'token', 'accessToken', 'jwt')
  if (!token) throw new Error('La API no devolvió un token de sesión.')
  return token
}

// POST /api/Auth/logout — invalida el token usado.
export const logout = () => api.post('/api/Auth/logout')
