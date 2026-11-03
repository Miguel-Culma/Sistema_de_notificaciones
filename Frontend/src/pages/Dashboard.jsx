import { Link } from 'react-router-dom'
import PageHeader from '../components/PageHeader'
import Loading from '../components/Loading'
import ErrorAlert from '../components/ErrorAlert'
import { useAsync } from '../hooks/useAsync'
import { useAuth } from '../hooks/useAuth'
import { getInvoices } from '../services/invoiceService'
import { getReminderSettings } from '../services/reminderSettingsService'
import { INVOICE_STATES } from '../utils/invoiceStates'

// La configuración es opcional en el panel: si falla, el resto sigue funcionando.
const load = () => Promise.all([getInvoices(), getReminderSettings().catch(() => null)])

export default function Dashboard() {
  const { data, error, loading, reload } = useAsync(load)
  const { isAdmin } = useAuth()

  if (error) return <ErrorAlert error={error} onRetry={reload} />
  if (!data) return <Loading />

  const [invoices, settings] = data
  const total = invoices.length
  const counts = Object.fromEntries(
    INVOICE_STATES.map((s) => [s.value, invoices.filter((i) => i.state === s.value).length]),
  )

  return (
    <div className={loading ? 'opacity-60 transition-opacity' : ''}>
      <PageHeader
        title="Panel"
        description="Estado de las facturas dentro del proceso de recordatorios."
        actions={
          <button className="btn btn-sm btn-outline" onClick={reload} disabled={loading}>
            Actualizar
          </button>
        }
      />

      <div className="stats stats-vertical w-full border border-base-300 bg-base-100 sm:stats-horizontal">
        <div className="stat">
          <div className="stat-title">Total de facturas</div>
          <div className="stat-value tabular-nums">{total}</div>
        </div>
        {INVOICE_STATES.map((s) => (
          <div key={s.value} className="stat">
            <div className="stat-title">{s.label}</div>
            <div className="stat-value tabular-nums">{counts[s.value]}</div>
          </div>
        ))}
      </div>

      <section className="mt-6 rounded-box border border-base-300 bg-base-100 p-5">
        <h2 className="font-semibold">Distribución por estado</h2>
        {total === 0 ? (
          <p className="mt-2 text-sm opacity-70">Aún no hay facturas registradas.</p>
        ) : (
          <>
            <div className="mt-4 flex h-4 overflow-hidden rounded-full bg-base-200" role="img"
              aria-label="Distribución de facturas por estado">
              {INVOICE_STATES.map((s) => (
                <div key={s.value} className={s.bar} style={{ width: `${(counts[s.value] / total) * 100}%` }} />
              ))}
            </div>
            <ul className="mt-3 flex flex-wrap gap-x-6 gap-y-1 text-sm">
              {INVOICE_STATES.map((s) => (
                <li key={s.value} className="flex items-center gap-2">
                  <span className={`h-2.5 w-2.5 rounded-full ${s.bar}`} />
                  {s.label}
                  <span className="tabular-nums opacity-60">
                    {Math.round((counts[s.value] / total) * 100)}%
                  </span>
                </li>
              ))}
            </ul>
          </>
        )}
        <Link to="/invoices" className="btn btn-sm btn-primary mt-5">
          Ver facturas
        </Link>
      </section>

      <section className="mt-6 rounded-box border border-base-300 bg-base-100 p-5">
        <h2 className="font-semibold">Procesamiento automático</h2>
        {settings ? (
          <div className="mt-3 flex flex-wrap items-center gap-x-8 gap-y-3 text-sm">
            <div className="flex items-center gap-2">
              <span className={`badge ${settings.enabled ? 'badge-success' : 'badge-ghost'}`}>
                {settings.enabled ? 'Habilitado' : 'Deshabilitado'}
              </span>
            </div>
            <div>
              Se evalúa cada{' '}
              <b className="tabular-nums">{settings.intervalMinutes ?? '—'}</b> minutos
            </div>
            {isAdmin && (
              <Link to="/settings" className="link link-primary">
                Cambiar configuración
              </Link>
            )}
          </div>
        ) : (
          <p className="mt-2 text-sm opacity-70">No se pudo consultar la configuración.</p>
        )}
      </section>
    </div>
  )
}
