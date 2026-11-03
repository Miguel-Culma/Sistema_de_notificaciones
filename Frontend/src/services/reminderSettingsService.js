import { api } from './api'
import { pick } from './normalize'

export async function getReminderSettings() {
  const raw = await api.get('/api/ReminderSettings')
  return {
    enabled: Boolean(pick(raw, 'enabled', 'isEnabled')),
    intervalMinutes: pick(raw, 'intervalMinutes', 'interval', 'intervalInMinutes'),
  }
}

// Ajusta los nombres de los campos del body si tu API espera otros.
export const updateEnabled = (enabled) => api.put('/api/ReminderSettings/enabled', { enabled })
export const updateInterval = (intervalMinutes) =>
  api.put('/api/ReminderSettings/interval', { intervalMinutes })
