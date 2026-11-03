const dateFmt = new Intl.DateTimeFormat('es-CO', { day: '2-digit', month: 'short', year: 'numeric' })
const dateTimeFmt = new Intl.DateTimeFormat('es-CO', {
  day: '2-digit',
  month: 'short',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
})

const toDate = (value) => {
  if (!value) return null
  const d = new Date(value)
  return Number.isNaN(d.getTime()) ? null : d
}

export const formatDate = (value) => {
  const d = toDate(value)
  return d ? dateFmt.format(d) : '—'
}

export const formatDateTime = (value) => {
  const d = toDate(value)
  return d ? dateTimeFmt.format(d) : '—'
}

export function formatMoney(amount, currency) {
  if (amount === undefined || amount === null || amount === '') return '—'
  const n = Number(amount)
  if (Number.isNaN(n)) return String(amount)
  try {
    return currency
      ? new Intl.NumberFormat('es-CO', { style: 'currency', currency }).format(n)
      : new Intl.NumberFormat('es-CO').format(n)
  } catch {
    return `${new Intl.NumberFormat('es-CO').format(n)} ${currency ?? ''}`.trim()
  }
}
