const BASE_URL = import.meta.env.VITE_API_URL
const TOKEN_KEY = 'auth_token'

export const tokenStorage = {
  get: () => localStorage.getItem(TOKEN_KEY),
  set: (token) => localStorage.setItem(TOKEN_KEY, token),
  clear: () => localStorage.removeItem(TOKEN_KEY),
}

// El AuthProvider registra aquí qué hacer cuando la API responde 401 con un token enviado.
let unauthorizedHandler = null
export const setUnauthorizedHandler = (fn) => {
  unauthorizedHandler = fn
}

export class ApiError extends Error {
  constructor(message, status, data) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.data = data
  }
}

const DEFAULT_MESSAGES = {
  400: 'La solicitud no es válida. Revisa los datos e inténtalo de nuevo.',
  401: 'Tu sesión no es válida o venció. Inicia sesión de nuevo.',
  403: 'No tienes permisos para realizar esta acción.',
  404: 'El registro solicitado no existe.',
}

function errorMessage(status, data) {
  const fromApi =
    (typeof data === 'string' && data.trim()) || data?.message || data?.title || data?.error
  return fromApi || DEFAULT_MESSAGES[status] || 'Ocurrió un error en el servidor. Inténtalo más tarde.'
}

async function request(path, { method = 'GET', body, auth = true } = {}) {
  const headers = { Accept: 'application/json' }
  if (body !== undefined) headers['Content-Type'] = 'application/json'

  const token = tokenStorage.get()
  if (auth && token) headers.Authorization = `Bearer ${token}`

  let response
  try {
    response = await fetch(`${BASE_URL}${path}`, {
      method,
      headers,
      body: body !== undefined ? JSON.stringify(body) : undefined,
    })
  } catch {
    throw new ApiError('No se pudo conectar con el servidor. Revisa tu conexión e inténtalo de nuevo.', 0)
  }

  const text = await response.text()
  let data = null
  if (text) {
    try {
      data = JSON.parse(text)
    } catch {
      data = text
    }
  }

  if (!response.ok) {
    if (response.status === 401 && auth && token) unauthorizedHandler?.()
    throw new ApiError(errorMessage(response.status, data), response.status, data)
  }
  return data
}

export const api = {
  get: (path, options) => request(path, { ...options, method: 'GET' }),
  post: (path, body, options) => request(path, { ...options, method: 'POST', body }),
  put: (path, body, options) => request(path, { ...options, method: 'PUT', body }),
}
