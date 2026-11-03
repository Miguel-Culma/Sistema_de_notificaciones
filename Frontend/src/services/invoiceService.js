import { api } from './api'
import { pick, toList } from './normalize'

function normalizeInvoice(raw) {
  const invoice = raw?.invoice ?? raw ?? {}

  return {
    id: invoice._id,
    number: invoice.invoiceNumber ?? '',
    customer: raw?.clientEmail ?? '',
    amount: invoice.amount,
    currency: invoice.currency,
    issueDate: invoice.createAt,
    dueDate: invoice.dueDate,
    state: String(invoice.status ?? '').toLowerCase(),
    updatedAt: invoice.updatedAt,
  }
}

export const getInvoices = async () => toList(await api.get('/api/Invoices')).map(normalizeInvoice)

export const getInvoice = async (id) => {
  return normalizeInvoice(await api.get(`/api/Invoices/${encodeURIComponent(id)}`))
}
  
export const getInvoicesByState = async (state) =>
  toList(await api.get(`/api/Invoices/state/${encodeURIComponent(state)}`)).map(normalizeInvoice)

// El backend decide el siguiente paso según el estado actual y actualiza el estado si el envío funciona.
export const sendReminder = (id) => api.post(`/api/Invoices/${encodeURIComponent(id)}/reminder`)

// Solo para pruebas: ajusta la ruta y el body a lo que exponga tu API.
export const updateInvoiceState = async (id, state) => {
  return await api.put(`/api/Invoices/${encodeURIComponent(id)}/state`,  state )
}
