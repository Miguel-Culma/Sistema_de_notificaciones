
import { useRef, useState } from 'react'
import { useToast } from '../hooks/useToast'
import { updateInvoiceState } from '../services/invoiceService'
import { INVOICE_STATES } from '../utils/invoiceStates'

export default function ChangeStateButton({ invoice, onDone }) {
  const dialogRef = useRef(null)
  const { notify } = useToast()
  const [selected, setSelected] = useState(invoice.state)
  const [saving, setSaving] = useState(false)

  const open = () => {
    setSelected(invoice.state)
    dialogRef.current?.showModal()
  }

  const save = async () => {
    setSaving(true)
    try {
      await updateInvoiceState(invoice.id, selected)
      notify('success', `Estado de la factura ${invoice.number} actualizado.`)
      dialogRef.current?.close()
      onDone?.()
    } catch (err) {
      notify('error', err.message)
    } finally {
      setSaving(false)
    }
  }

  return (
    <>
      <button
        className="btn btn-ghost btn-xs"
        onClick={open}
        aria-label={`Cambiar estado de la factura ${invoice.number}`}
      >
        Cambiar
      </button>

      <dialog ref={dialogRef} className="modal">
        <div className="modal-box text-left">

          <h3 className="text-lg font-semibold">
            Cambiar estado del recordatorio
          </h3>

          {/* Advertencia de función experimental */}
          <div
            role="alert"
            className="mt-4 rounded-xl border border-warning/30
                       bg-warning/10 p-4 text-base-content shadow-sm"
          >
            <div className="flex items-center gap-2 mb-3">
              <span className="text-xl text-warning" aria-hidden="true">
                ⚠
              </span>

              <h4 className="font-semibold text-sm">
                Precaución: función de pruebas
              </h4>

              <span className="ml-auto badge badge-warning badge-sm">
                Experimental
              </span>
            </div>

            <p className="text-sm leading-relaxed mb-3">
              Esta acción está destinada exclusivamente a facilitar
              las pruebas del equipo de ingeniería. No forma parte
              del flujo operativo habitual del sistema.
            </p>

            <div className="flex gap-3 rounded-lg bg-warning/15 p-3">
              <span className="text-lg text-warning" aria-hidden="true">
                !
              </span>

              <p className="text-sm leading-relaxed">
                <strong>Antes de continuar:</strong> asegúrese de que
                el envío automático de recordatorios esté desactivado
                para evitar ejecuciones simultáneas o comportamientos
                inesperados.
              </p>
            </div>

            <p className="mt-3 text-xs opacity-70">
              Utilice esta funcionalidad únicamente en entornos de prueba.
            </p>
          </div>

          <p className="mt-4 text-sm">
            Factura <b>{invoice.number}</b>
          </p>

          <label className="form-control mt-3 w-full">
            <span className="label-text mb-1">Nuevo estado</span>

            <select
              className="select select-bordered w-full"
              value={selected}
              onChange={(e) => setSelected(e.target.value)}
            >
              {!INVOICE_STATES.some((s) => s.value === selected) && (
                <option value={selected} disabled>
                  Seleccionar estado
                </option>
              )}

              {INVOICE_STATES.map((s) => (
                <option key={s.value} value={s.value}>
                  {s.label}
                </option>
              ))}
            </select>
          </label>

          <div className="modal-action">
            <form method="dialog">
              <button className="btn btn-ghost" disabled={saving}>
                Cancelar
              </button>
            </form>

            <button
              className="btn btn-primary"
              onClick={save}
              disabled={saving || selected === invoice.state}
            >
              {saving && (
                <span className="loading loading-spinner loading-sm" />
              )}
              Guardar estado
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
