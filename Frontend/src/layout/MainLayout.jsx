import { useRef } from 'react'
import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import { useReminderTimers } from '../hooks/useReminderTimers' // NUEVO
import ReminderTimers from '../components/ReminderTimers' // NUEVO
import logo from '../assets/logo.svg'

const linkClass = ({ isActive }) => (isActive ? 'active font-medium' : '')

export default function MainLayout() {
  const { user, isAdmin, logout } = useAuth()
  const toggleRef = useRef(null)
  const timers = useReminderTimers() // NUEVO
  const closeDrawer = () => {
    if (toggleRef.current) toggleRef.current.checked = false
  }

  return (
    <div className="drawer lg:drawer-open">
      <input id="main-drawer" ref={toggleRef} type="checkbox" className="drawer-toggle" />

      <div className="drawer-content flex min-h-screen flex-col">
        <header className="navbar border-b border-base-300 bg-base-100">
          <div className="flex-none lg:hidden">
            <label htmlFor="main-drawer" className="btn btn-ghost btn-square" aria-label="Abrir menú">
              <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M4 6h16M4 12h16M4 18h16" />
              </svg>
            </label>
          </div>
          <div className="flex-1" />
          <div className="flex items-center gap-3 pr-2">
            <span className="text-sm">{user?.name}</span>
            {isAdmin && <span className="badge badge-outline badge-sm">Admin</span>}
            <button className="btn btn-ghost btn-sm" onClick={logout}>
              Cerrar sesión
            </button>
          </div>
        </header>

        <main className="mx-auto w-full max-w-7xl flex-1 p-4 lg:p-8">
          {/* NUEVO: las páginas pueden pedir actualizar los temporizadores */}
          <Outlet context={{ refreshTimers: timers.refresh }} />
        </main>
      </div>

      <div className="drawer-side z-40">
        <label htmlFor="main-drawer" aria-label="Cerrar menú" className="drawer-overlay" />
        <aside className="flex min-h-full w-64 flex-col border-r border-base-300 bg-base-100 p-4">
          <div className="mb-6 flex items-center gap-3 px-2">
            <img src={logo} alt="" className="h-8 w-8" />
            <span className="font-semibold leading-tight">Recordatorios de facturas</span>
          </div>
          <ul className="menu gap-1 p-0" onClick={closeDrawer}>
            <li>
              <NavLink to="/" end className={linkClass}>
                Panel
              </NavLink>
            </li>
            <li>
              <NavLink to="/invoices" className={linkClass}>
                Facturas
              </NavLink>
            </li>
            <li>
              <NavLink to="/notifications" className={linkClass}>
                Historial de notificaciones
              </NavLink>
            </li>
            {isAdmin && (
              <li>
                <NavLink to="/settings" className={linkClass}>
                  Recordatorios automáticos
                </NavLink>
              </li>
            )}
          </ul>

          {/* NUEVO: temporizadores al pie de la barra lateral */}
          <ReminderTimers {...timers} />
        </aside>
      </div>
    </div>
  )
}
