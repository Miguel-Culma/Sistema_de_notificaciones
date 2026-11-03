import { useCallback, useMemo, useRef, useState } from 'react'
import { ToastContext } from './ToastContext'

const TYPE_CLASS = { success: 'alert-success', error: 'alert-error', info: 'alert-info' }

export default function ToastProvider({ children }) {
  const [toasts, setToasts] = useState([])
  const nextId = useRef(0)

  const notify = useCallback((type, message) => {
    const id = nextId.current++
    setToasts((prev) => [...prev, { id, type, message }])
    setTimeout(() => setToasts((prev) => prev.filter((t) => t.id !== id)), 5000)
  }, [])

  const value = useMemo(() => ({ notify }), [notify])

  return (
    <ToastContext.Provider value={value}>
      {children}
      <div className="toast toast-end toast-bottom z-50" aria-live="polite">
        {toasts.map((t) => (
          <div key={t.id} role="status" className={`alert ${TYPE_CLASS[t.type] ?? ''} max-w-sm text-sm`}>
            <span>{t.message}</span>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  )
}
