import PageHeader from '../components/PageHeader'
import Loading from '../components/Loading'
import ErrorAlert from '../components/ErrorAlert'
import { useAsync } from '../hooks/useAsync'
import { getNotifications } from '../services/notificationService'
import { formatDate, formatDateTime } from '../utils/format'

export default function Notifications() {
  const { data, error, loading, reload } = useAsync(getNotifications)
  return (
    <>
      <PageHeader
        title="Historial de notificaciones"
        description="Correos procesados por los recordatorios manuales y automáticos."
        actions={
          <button className="btn btn-sm btn-outline" onClick={reload} disabled={loading}>
            Actualizar
          </button>
        }
      />

      {error ? (
        <ErrorAlert error={error} onRetry={reload} />
      ) : !data ? (
        <Loading />
      ) : data.length === 0 ? (
        <div className="rounded-box border border-base-300 bg-base-100 p-10 text-center">
          <p className="font-medium">Todavía no hay notificaciones registradas.</p>
          <p className="mt-1 text-sm opacity-70">Aparecerán aquí cuando se envíe un recordatorio.</p>
        </div>
      ) : (
        <div className={`overflow-x-auto rounded-box border border-base-300 bg-base-100 ${loading ? 'opacity-60' : ''}`}>
          <table className="table">
            <thead>
              <tr>
                <th className="text-center">Fecha</th>
                <th className="text-center">Factura</th>
                <th className="text-center">Destinatario</th>
                <th className="text-center">Resultado</th>
                <th className="text-center">Mensaje</th>
              </tr>
            </thead>
            <tbody>
              {data.map((n) => (
                <tr className="text-center" key={n.id}>
                  <td className="whitespace-nowrap">{formatDate(n.createdAt)}</td>
                  <td className="tabular-nums">{n.invoice ?? '—'}</td>
                  <td>{n.recipient ?? '—'}</td>
                  <td>
                    <span className={`badge badge-sm ${n.ok ? 'badge-success' : 'badge-error'}`}>
                      {n.ok ? 'Enviado' : 'error'}
                    </span>
                  </td>
                  <td className="max-w-md text-sm opacity-80">{n.message ?? n.result ?? '—'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </>
  )
}
