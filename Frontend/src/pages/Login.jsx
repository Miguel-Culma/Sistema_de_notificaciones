import { useState } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import { INVOICE_STATES } from '../utils/invoiceStates'
import logo from '../assets/logo.svg'

export default function Login() {
  const { isAuthenticated, login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const from = location.state?.from?.pathname ?? '/'

  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  if (isAuthenticated) return <Navigate to={from} replace />

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    setSubmitting(true)
    try {
      await login(username.trim(), password)
      navigate(from, { replace: true })
    } catch (err) {
      setError(
        err.status === 401 || err.status === 400
          ? 'Usuario o contraseña incorrectos. Revisa los datos e inténtalo de nuevo.'
          : err.message,
      )
      setSubmitting(false)
    }
  }

  return (
    <div className="grid min-h-screen lg:grid-cols-[1fr_minmax(0,28rem)]">
      <section className="hidden flex-col justify-between bg-neutral p-12 text-neutral-content lg:flex">
        <div className="flex items-center gap-3">
          <img src={logo} alt="" className="h-9 w-9" />
          <span className="text-lg font-semibold">Recordatorios de facturas</span>
        </div>

        <div className="max-w-md">
          <h2 className="text-3xl font-semibold leading-tight">
            Prueba tecnca Monolegal.
          </h2>
          <ol className="mt-8 space-y-4">
            {INVOICE_STATES.map((s, i) => (
              <li key={s.value} className="flex items-center gap-4">
                <span className={`h-3 w-3 rounded-full ${s.bar}`} />
                <span className="text-lg">{s.label}</span>
                {i < INVOICE_STATES.length - 1 && <span className="opacity-40">luego</span>}
              </li>
            ))}
          </ol>
        </div>

        <p className="text-sm opacity-60">Acceso solo para operadores autorizados.</p>
      </section>

      <section className="flex items-center justify-center bg-base-100 p-6">
        <form onSubmit={handleSubmit} className="w-full max-w-sm space-y-5">
          <div>
            <h1 className="text-2xl font-semibold">Iniciar sesión</h1>
            <p className="mt-1 text-sm opacity-70">Entra para gestionar los recordatorios de cobro.</p>
          </div>

          {error && (
            <div role="alert" className="alert alert-error text-sm">
              <span>{error}</span>
            </div>
          )}

          <label className="form-control w-full">
            <span className="label-text mb-1">Usuario</span>
            <input
              className="input input-bordered w-full"
              autoComplete="username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
              autoFocus
            />
          </label>

          <label className="form-control w-full">
            <span className="label-text mb-1">Contraseña</span>
            <input
              type="password"
              className="input input-bordered w-full"
              autoComplete="current-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </label>

          <button type="submit" className="btn btn-primary w-full" disabled={submitting}>
            {submitting && <span className="loading loading-spinner loading-sm" />}
            Iniciar sesión
          </button>
        </form>
      </section>
    </div>
  )
}
