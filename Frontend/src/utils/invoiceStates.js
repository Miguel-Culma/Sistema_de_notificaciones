// Estados del proceso de recordatorios según la API.
export const INVOICE_STATES = [
  { value: 'primerrecordatorio', label: 'Primer recordatorio', badge: 'badge-warning', bar: 'bg-warning' },
  { value: 'segundorecordatorio', label: 'Segundo recordatorio', badge: 'badge-error', bar: 'bg-error' },
  { value: 'desactivado', label: 'Desactivado', badge: 'badge-ghost', bar: 'bg-base-300' },
]

export function stateMeta(state) {
  return (
    INVOICE_STATES.find((s) => s.value === state) ?? {
      value: state,
      label: state || 'Sin estado',
      badge: 'badge-outline',
      bar: 'bg-base-300',
    }
  )
}

// Una factura desactivada ya no tiene un siguiente paso en el proceso.
export const canSendReminder = (state) => state !== 'desactivado'
