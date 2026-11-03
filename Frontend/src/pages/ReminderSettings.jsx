import { useState } from 'react'
import { useOutletContext } from 'react-router-dom' // NUEVO
import PageHeader from '../components/PageHeader'
import Loading from '../components/Loading'
import ErrorAlert from '../components/ErrorAlert'
import { useAsync } from '../hooks/useAsync'
import { useToast } from '../hooks/useToast'
import { getReminderSettings, updateEnabled, updateInterval } from '../services/reminderSettingsService'

function SettingsForm({ initial, onSaved }) {
  const { notify } = useToast()
  const [enabled, setEnabled] = useState(initial.enabled)
  const [minutes, setMinutes] = useState(String(initial.intervalMinutes ?? ''))
  const [minutesError, setMinutesError] = useState('')
  const [busy, setBusy] = useState(null)

  const handleToggle = async (e) => {
    const next = e.target.checked
    setEnabled(next)
    setBusy('enabled')
    try {
      await updateEnabled(next)
      notify('success', next ? 'Procesamiento automático habilitado.' : 'Procesamiento automático deshabilitado.')
      onSaved()
    } catch (err) {
      setEnabled(!next)
      notify('error', err.message)
    } finally {
      setBusy(null)
    }
  }

  const handleInterval = async (e) => {
    e.preventDefault()
    const value = Number(minutes)
    if (!Number.isInteger(value) || value < 1) {
      setMinutesError('Ingresa un número entero de minutos mayor o igual a 1.')
      return
    }
    setMinutesError('')
    setBusy('interval')
    try {
      await updateInterval(value)
      notify('success', `Intervalo actualizado a ${value} minutos.`)
      onSaved()
    } catch (err) {
      notify('error', err.message)
    } finally {
      setBusy(null)
    }
  }

  return (
    <div className="max-w-xl space-y-6">
      <section className="rounded-box border border-base-300 bg-base-100 p-5">
        <label className="flex cursor-pointer items-center justify-between gap-4">
          <span>
            <span className="block font-semibold">Ejecución automática</span>
            <span className="block text-sm opacity-70">
              Evalúa las facturas cada cierto tiempo y envía los recordatorios que correspondan.
            </span>
          </span>
          <input
            type="checkbox"
            className="toggle toggle-primary"
            checked={enabled}
            disabled={busy === 'enabled'}
            onChange={handleToggle}
          />
        </label>
      </section>

      <form onSubmit={handleInterval} className="rounded-box border border-base-300 bg-base-100 p-5">
        <label className="form-control w-full">
          <span className="font-semibold">Intervalo de evaluación</span>
          <span className="mb-2 text-sm opacity-70">Cada cuántos minutos se revisan las facturas.</span>
          <div className="flex gap-3">
            <input
              type="number"
              min="1"
              step="1"
              className={`input input-bordered w-32 tabular-nums ${minutesError ? 'input-error' : ''}`}
              value={minutes}
              onChange={(e) => setMinutes(e.target.value)}
            />
            <button className="btn btn-primary" disabled={busy === 'interval'}>
              {busy === 'interval' && <span className="loading loading-spinner loading-sm" />}
              Guardar intervalo
            </button>
          </div>
          {minutesError && <span className="mt-2 text-sm text-error">{minutesError}</span>}
        </label>
      </form>
    </div>
  )
}

export default function ReminderSettings() {
  const { data, error, reload } = useAsync(getReminderSettings)
  // NUEVO: función que expone MainLayout para refrescar los temporizadores
  const refreshTimers = useOutletContext()?.refreshTimers

  return (
    <>
      <PageHeader
        title="Recordatorios automáticos"
        description="Controla si los recordatorios se envían solos y con qué frecuencia se evalúan."
      />
      {error ? (
        <ErrorAlert error={error} onRetry={reload} />
      ) : !data ? (
        <Loading />
      ) : (
        // La key reinicia el formulario con los valores que devuelve la API tras cada cambio.
        <SettingsForm
          key={`${data.enabled}-${data.intervalMinutes}`}
          initial={data}
          onSaved={() => {
            reload()
            refreshTimers?.() // NUEVO: al guardar, los temporizadores se actualizan al instante
          }}
        />
      )}
    </>
  )
}
