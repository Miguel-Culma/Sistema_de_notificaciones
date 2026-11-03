import { useCallback, useEffect, useMemo, useState } from 'react'
import { AuthContext } from './AuthContext'
import * as authService from '../services/authService'
import { setUnauthorizedHandler, tokenStorage } from '../services/api'
import { decodeJwt, getRoles, getUserName, isExpired } from '../utils/jwt'

function readInitialToken() {
  const token = tokenStorage.get()
  if (!token) return null
  const payload = decodeJwt(token)
  if (!payload || isExpired(payload)) {
    tokenStorage.clear()
    return null
  }
  return token
}

export default function AuthProvider({ children }) {
  const [token, setToken] = useState(readInitialToken)

  // Si la API responde 401 con un token enviado, la sesión ya no sirve.
  useEffect(() => {
    setUnauthorizedHandler(() => {
      tokenStorage.clear()
      setToken(null)
    })
    return () => setUnauthorizedHandler(null)
  }, [])

  const login = useCallback(async (username, password) => {
    const newToken = await authService.login(username, password)
    tokenStorage.set(newToken)
    setToken(newToken)
  }, [])

  const logout = useCallback(async () => {
    try {
      await authService.logout()
    } catch {
      // Aunque falle la invalidación en el servidor, se cierra la sesión local.
    }
    tokenStorage.clear()
    setToken(null)
  }, [])

  const value = useMemo(() => {
    const payload = token ? decodeJwt(token) : null
    const roles = getRoles(payload)
    return {
      token,
      isAuthenticated: Boolean(token),
      user: payload ? { name: getUserName(payload), roles } : null,
      isAdmin: roles.includes('admin'),
      login,
      logout,
    }
  }, [token, login, logout])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
