import { stateMeta } from '../utils/invoiceStates'

export default function StatusBadge({ state }) {
  const { label, badge } = stateMeta(state)
  return <span className={`badge badge-sm whitespace-nowrap ${badge}`}>{label}</span>
}
