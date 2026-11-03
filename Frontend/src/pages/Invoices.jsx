import { useCallback, useState } from 'react'
import PageHeader from '../components/PageHeader'
import Loading from '../components/Loading'
import ErrorAlert from '../components/ErrorAlert'
import InvoiceTable from '../components/InvoiceTable'
import { useAsync } from '../hooks/useAsync'
import { getInvoices, getInvoicesByState } from '../services/invoiceService'
import { INVOICE_STATES } from '../utils/invoiceStates'

const PAGE_SIZE = 10

export default function Invoices() {
  const [stateFilter, setStateFilter] = useState('')
  const [query, setQuery] = useState('')
  const [page, setPage] = useState(1)

  // El filtro por estado usa el endpoint /state/{state}; cliente y número se filtran aquí.
  const fetcher = useCallback(
    () => (stateFilter ? getInvoicesByState(stateFilter) : getInvoices()),
    [stateFilter],
  )
  const { data, error, loading, reload } = useAsync(fetcher)

  const q = query.trim().toLowerCase()
  const invoices = data ?? []
  const filtered = q
    ? invoices.filter((i) => `${i.number} ${i.customer}`.toLowerCase().includes(q))
    : invoices

  const pageCount = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE))
  const current = Math.min(page, pageCount)
  const visible = filtered.slice((current - 1) * PAGE_SIZE, current * PAGE_SIZE)

  const clearFilters = () => {
    setStateFilter('')
    setQuery('')
    setPage(1)
  }

  return (
    <>
      <PageHeader
        title="Facturas"
        description="Consulta las facturas y envía recordatorios manualmente."
        actions={
          <button className="btn btn-sm btn-outline" onClick={reload} disabled={loading}>
            Actualizar
          </button>
        }
      />

      <div className="mb-4 flex flex-wrap gap-3">
        <input
          type="search"
          className="input input-bordered input-sm w-full sm:w-72"
          placeholder="Buscar por cliente o número"
          aria-label="Buscar por cliente o número de factura"
          value={query}
          onChange={(e) => {
            setQuery(e.target.value)
            setPage(1)
          }}
        />
        <select
          className="select select-bordered select-sm"
          value={stateFilter}
          onChange={(e) => {
            setStateFilter(e.target.value)
            setPage(1)
          }}
        >
          <option value="">Todos los estados</option>
          {INVOICE_STATES.map((s) => (
            <option key={s.value} value={s.value}>
              {s.label}
            </option>
          ))}
        </select>
      </div>

      {error ? (
        <ErrorAlert error={error} onRetry={reload} />
      ) : !data ? (
        <Loading />
      ) : filtered.length === 0 ? (
        <div className="rounded-box border border-base-300 bg-base-100 p-10 text-center">
          <p className="font-medium">No hay facturas con estos filtros.</p>
          {(stateFilter || query) && (
            <button className="btn btn-sm btn-outline mt-4" onClick={clearFilters}>
              Quitar filtros
            </button>
          )}
        </div>
      ) : (
        <div className={loading ? 'opacity-60 transition-opacity' : ''}>
          <InvoiceTable invoices={visible} onReminderSent={reload} />
          <div className="mt-4 flex items-center justify-between text-sm">
            <span className="opacity-70 tabular-nums">
              {filtered.length} {filtered.length === 1 ? 'factura' : 'facturas'}
            </span>
            {pageCount > 1 && (
              <div className="join">
                <button className="btn btn-sm join-item" disabled={current === 1} onClick={() => setPage(current - 1)}>
                  Anterior
                </button>
                <span className="btn btn-sm join-item pointer-events-none tabular-nums">
                  {current} / {pageCount}
                </span>
                <button className="btn btn-sm join-item" disabled={current === pageCount} onClick={() => setPage(current + 1)}>
                  Siguiente
                </button>
              </div>
            )}
          </div>
        </div>
      )}
    </>
  )
}
