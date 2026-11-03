import { Link } from 'react-router-dom'
import StatusBadge from './StatusBadge'
import ReminderButton from './ReminderButton'
import ChangeStateButton from './ChangeStateButton'
import { useAuth } from '../hooks/useAuth'
import { formatDate, formatMoney } from '../utils/format'

export default function InvoiceTable({ invoices, onReminderSent }) {
  const { isAdmin } = useAuth()

  return (
    <div className="overflow-x-auto rounded-box border border-base-300 bg-base-100">
      <table className="table">
        <thead>
          <tr>
            <th>Factura</th>
            <th>Cliente</th>
            <th className="text-center">Valor</th>
            <th className="text-center">Emisión</th>
            <th className="text-center">Vencimiento</th>
            <th className="text-center">Estado</th>
            <th className="text-center">Actualizada</th>
            <th className="text-center">Acciones</th>
          </tr>
        </thead>
        <tbody>
          {invoices.map((inv) => (
            <tr key={inv.id} className="hover text-center">
              <td className="font-medium tabular-nums">
                <Link to={`/invoices/${inv.id}`} className="link link-hover link-primary">
                  {inv.number}
                </Link>
              </td>
              <td>{inv.customer || '—'}</td>
              <td className="text-center tabular-nums">
                {formatMoney(inv.amount)} {inv.currency}
              </td>
              <td className="whitespace-nowrap">{formatDate(inv.issueDate)}</td>
              <td className="whitespace-nowrap">{formatDate(inv.dueDate)}</td>
              <td>
                <div className="flex  flex-col items-center justify-center gap-2">
                  <StatusBadge state={inv.state} />
                  {isAdmin && <ChangeStateButton invoice={inv} onDone={onReminderSent} />}
                </div>
              </td>
              <td className="whitespace-nowrap text-xs opacity-70">{formatDate(inv.updatedAt)}</td>
              <td>
                <div className="flex flex-col justify-end gap-1">
                  <Link to={`/invoices/${inv.id}`} className="btn btn-ghost btn-sm">
                    Ver detalle
                  </Link>
                  <ReminderButton invoice={inv} onDone={onReminderSent} />
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}