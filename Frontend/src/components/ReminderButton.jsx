import { useRef, useState } from 'react'
import { useToast } from '../hooks/useToast'
import { sendReminder } from '../services/invoiceService'
import { canSendReminder } from '../utils/invoiceStates'

export default function ReminderButton({ invoice, onDone, size = 'btn-sm' }) {
  const dialogRef = useRef(null)
  const [sending, setSending] = useState(false)
  const { notify } = useToast()

  const allowed = canSendReminder(invoice.state)

  const confirm = async () => {
    dialogRef.current?.close()
    setSending(true)
    try {
      await sendReminder(invoice.id)
      notify('success', `Recordatorio de la factura ${invoice.number} enviado.`)
      onDone?.()
    } catch (err) {
      notify('error', err.message)
    } finally {
      setSending(false)
    }
  }

  return (
    <>
      <button
        className={`btn btn-primary ${size}`}
        disabled={!allowed || sending}
        title={allowed ? undefined : 'Esta factura ya no tiene recordatorios pendientes'}
        onClick={() => dialogRef.current?.showModal()}
      >
        {sending && <span className="loading loading-spinner loading-xs" />}
        Enviar recordatorio
      </button>

      <dialog ref={dialogRef} className="modal">
        <div className="modal-box">
          <h3 className="text-lg font-semibold">¿Enviar recordatorio?</h3>
          <p className="py-3 text-sm">
            Se enviará al cliente el correo que corresponde al estado actual de la factura{' '}
            <b>{invoice.number}</b>. Esta acción no se puede deshacer.
          </p>
          <div className="modal-action">
            <form method="dialog">
              <button className="btn btn-ghost">Cancelar</button>
            </form>
            <button className="btn btn-primary" onClick={confirm}>
              Enviar recordatorio
            </button>
          </div>
        </div>
        <form method="dialog" className="modal-backdrop">
          <button aria-label="Cerrar">cerrar</button>
        </form>
      </dialog>
    </>
  )
}
