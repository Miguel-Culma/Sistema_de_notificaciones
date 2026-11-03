export default function ErrorAlert({ error, onRetry }) {
  return (
    <div role="alert" className="alert alert-error">
      <span>{error?.message ?? 'Ocurrió un error inesperado.'}</span>
      {onRetry && (
        <button className="btn btn-sm" onClick={onRetry}>
          Reintentar
        </button>
      )}
    </div>
  )
}
