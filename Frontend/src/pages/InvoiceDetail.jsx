import { useCallback } from 'react'
import { Link, useParams } from 'react-router-dom'
import PageHeader from '../components/PageHeader'
import Loading from '../components/Loading'
import ErrorAlert from '../components/ErrorAlert'
import StatusBadge from '../components/StatusBadge'
import ReminderButton from '../components/ReminderButton'
import { useAsync } from '../hooks/useAsync'
import { getInvoice } from '../services/invoiceService'
import { formatDate, formatDateTime, formatMoney } from '../utils/format'

export default function InvoiceDetail() {
  const { id } = useParams()
  const fetcher = useCallback(() => getInvoice(id), [id])
  const { data: invoice, error, loading, reload } = useAsync(fetcher)
  const back = (
    <Link to="/invoices" className="btn btn-sm btn-ghost mb-4">
      Volver a facturas
    </Link>
  )

  if (error) {
    return (
      <>
        {back}
        <ErrorAlert error={error} onRetry={reload} />
      </>
    )
  }
  if (!invoice) return <Loading />

  const fields = [
    ['Cliente', invoice.customer || '—'],
    ['Valor', formatMoney(invoice.amount, invoice.currency)],
    ['Fecha de emisión', formatDate(invoice.issueDate)],
    ['Fecha de vencimiento', formatDate(invoice.dueDate)],
    ['Última actualización', formatDateTime(invoice.updatedAt)],
  ]

  return (
    <div className={loading ? 'opacity-60 transition-opacity' : ''}>
      {back}
      <PageHeader
        title={`Factura ${invoice.number}`}
        actions={<ReminderButton invoice={invoice} onDone={reload} size="btn-md" />}
      />

      <div className="rounded-box border border-base-300 bg-base-100 p-6">
        <div className="mb-5 flex items-center gap-3">
          <span className="text-sm opacity-70">Estado del recordatorio</span>
          <StatusBadge state={invoice.state} />
        </div>
        <dl className="grid gap-x-8 gap-y-4 sm:grid-cols-2">
          {fields.map(([label, value]) => (
            <div key={label}>
              <dt className="text-sm opacity-70">{label}</dt>
              <dd className="font-medium tabular-nums">{value}</dd>
            </div>
          ))}
        </dl>
      </div>
    </div>
  )
}
