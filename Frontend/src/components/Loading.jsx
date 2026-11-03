export default function Loading({ label = 'Cargando…' }) {
  return (
    <div className="flex items-center justify-center gap-3 py-16 text-sm opacity-70" role="status">
      <span className="loading loading-spinner loading-md text-primary" />
      {label}
    </div>
  )
}
