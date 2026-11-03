import { formatClock } from '../utils/countdown'

// Mismos colores que los badges de estado: amarillo = primer, rojo = segundo.
const BAR_COLOR = { primer: 'progress-warning', segundo: 'progress-error' }

export default function ReminderTimers({ status, reason, timers, intervalMinutes, pending }) {
  const running = status === 'running'

  return (
    <section
      aria-label="Temporizadores de recordatorios automáticos"
      className="mt-auto rounded-box border border-base-300 bg-base-200 p-3 text-sm"
    >
      <div className="mb-2 flex items-center justify-between gap-2">
        <h2 className="font-semibold leading-tight">Envío automático</h2>
        {status !== 'loading' && (
          <span className={`badge badge-sm ${running ? 'badge-success' : 'badge-ghost'}`}>
            {running ? 'Activo' : 'Detenido'}
          </span>
        )}
      </div>

      {status === 'loading' && <p className="text-xs opacity-70">Cargando…</p>}

      {(status === 'stopped' || status === 'unavailable') && (
        <p className="text-xs opacity-70">{reason}</p>
      )}

      {running && (
        <>
          <ul className="space-y-3">
            {timers.map((t) => (
              <li key={t.id}>
                <div className="flex items-baseline justify-between gap-2">
                  <span className="text-xs">{t.label}</span>
                  {/* role="timer" evita que el lector de pantalla anuncie cada segundo */}
                  <span role="timer" aria-label={`${t.label}: faltan`} className="font-medium tabular-nums">
                    {formatClock(t.remaining)}
                  </span>
                </div>
                <progress
                  className={`progress ${BAR_COLOR[t.id]} mt-1 w-full`}
                  value={t.elapsedInCycle}
                  max={t.period}
                  aria-hidden="true"
                />
              </li>
            ))}
          </ul>
          <p className="mt-3 text-xs opacity-70">
            Cada {intervalMinutes} min · {pending} {pending === 1 ? 'factura pendiente' : 'facturas pendientes'}
          </p>
        </>
      )}
    </section>
  )
}
