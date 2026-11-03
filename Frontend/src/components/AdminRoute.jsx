import { Navigate, Outlet, useOutletContext } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'

export default function AdminRoute() {
  const { isAdmin } = useAuth()
  // Reenvía el contexto que MainLayout le pasa a sus rutas hijas (refreshTimers).
  const context = useOutletContext()
  return isAdmin ? <Outlet context={context} /> : <Navigate to="/" replace />
}