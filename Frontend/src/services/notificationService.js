import { api } from './api'
import { pick, toList } from './normalize'

function normalizeNotification(raw, index) {
  const result = pick(raw, 'result', 'status')
  const success = pick(raw, 'success', 'isSuccess', 'sent')
  return {
    id: pick(raw, 'id') ?? index,
    invoice: pick(raw, 'invoiceNumber') ?? pick(raw, 'invoiceId'),
    recipient: pick(raw, 'recipient', 'recipientEmail', 'email', 'to'),
    ok: typeof success === 'boolean' ? success : !/error|fail|fall/i.test(String(result ?? '')),
    result: result !== undefined ? String(result) : undefined,
    message: pick(raw, 'message', 'detail', 'error'),
    createdAt: pick(raw, 'createAt', 'sentAt', 'date', 'timestamp'),
  }
}

export const getNotifications = async () =>
  toList(await api.get('/api/Notifications'))
    .map(normalizeNotification)
    .sort((a, b) => new Date(b.createdAt ?? 0) - new Date(a.createdAt ?? 0))
