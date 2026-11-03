import { useEffect, useState } from 'react'
import { useAsync } from './useAsync'
import { getInvoices } from '../services/invoiceService'
import { getReminderSettings } from '../services/reminderSettingsService'
import { canSendReminder } from '../utils/invoiceStates'
import { cycleState } from '../utils/countdown'

// Si una de las dos consultas falla, esa parte queda en null y el widget lo
// indica; el resto de la app no se ve afectada.
const load = () =>
  Promise.all([getReminderSettings().catch(() => null), getInvoices().catch(() => null)])

const EMPTY_CLOCK = { elapsed: 0 }

/**
 * Dos temporizadores en bucle para el procesamiento automático:
 *  - Primer recordatorio:  dura el intervalo configurado (intervalMinutes).
 *  - Segundo recordatorio: dura la mitad del intervalo.
 *
 * Corren mientras la ejecución automática esté habilitada Y existan facturas
 * pendientes (cualquiera que no esté "desactivado"). Si deja de cumplirse
 * alguna, se detienen; al volver a cumplirse, arrancan de cero.
 *
 * Devuelve también `refresh()` para que otras pantallas pidan actualizar la
 * configuración y las facturas (por ejemplo, tras guardar ajustes).
 */
export function useReminderTimers() {
  const { data, reload } = useAsync(load)
  const [clock, setClock] = useState(EMPTY_CLOCK)

  const settings = data ? data[0] : null
  const invoices = data ? data[1] : null

  const minutes = Number(settings?.intervalMinutes)
  const validInterval = Number.isFinite(minutes) && minutes > 0
  const enabled = Boolean(settings?.enabled)
  const pending = invoices ? invoices.filter((inv) => canSendReminder(inv.state)).length : 0

  // Duraciones en segundos.
  const firstPeriod = validInterval ? Math.round(minutes * 60) : 0
  const secondPeriod = validInterval ? Math.max(1, Math.round(minutes * 30)) : 0

  // Condición para que corran. Para que basten "habilitado O pendientes"
  // (en vez de ambas), cambia && por || en la línea de abajo.
  const running = validInterval && enabled && pending > 0

  useEffect(() => {
    if (!running) return undefined

    // Se calcula con la hora real (no contando ticks) para no atrasarse si el
    // navegador frena los timers de una pestaña en segundo plano.
    const startedAt = Date.now()
    let lastCycle = 0

    const id = setInterval(() => {
      const elapsed = Math.floor((Date.now() - startedAt) / 1000)
      setClock({ elapsed })

      // Cada vez que termina una vuelta del temporizador más corto, el backend
      // pudo haber procesado facturas: se vuelve a consultar el estado.
      const cycle = Math.floor(elapsed / secondPeriod)
      if (cycle > lastCycle) {
        lastCycle = cycle
        reload()
      }
    }, 1000)

    // Al detenerse (o cambiar el intervalo) se limpia el reloj: el próximo
    // arranque empieza de cero.
    return () => {
      clearInterval(id)
      setClock(EMPTY_CLOCK)
    }
  }, [running, secondPeriod, reload])

  let status = 'running'
  let reason = ''
  if (!data) {
    status = 'loading'
  } else if (!settings || !invoices) {
    status = 'unavailable'
    reason = !settings
      ? 'No se pudo consultar la configuración.'
      : 'No se pudieron consultar las facturas.'
  } else if (!validInterval) {
    status = 'stopped'
    reason = 'Intervalo de evaluación no configurado.'
  } else if (!enabled) {
    status = 'stopped'
    reason = 'La ejecución automática está deshabilitada.'
  } else if (pending === 0) {
    status = 'stopped'
    reason = 'No hay facturas pendientes.'
  }

  const timers = running
    ? [
        { id: 'primer', label: 'Primer recordatorio', period: firstPeriod, ...cycleState(clock.elapsed, firstPeriod) },
        { id: 'segundo', label: 'Segundo recordatorio', period: secondPeriod, ...cycleState(clock.elapsed, secondPeriod) },
      ]
    : []

  return {
    status,
    reason,
    timers,
    intervalMinutes: validInterval ? minutes : null,
    pending,
    refresh: reload,
  }
}
