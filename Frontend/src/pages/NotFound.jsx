import { Link } from 'react-router-dom'

export default function NotFound() {
  return (
    <div className="py-20 text-center">
      <h1 className="text-2xl font-semibold">Esta página no existe</h1>
      <p className="mt-2 text-sm opacity-70">Revisa la dirección o vuelve al panel.</p>
      <Link to="/" className="btn btn-primary btn-sm mt-5">
        Ir al panel
      </Link>
    </div>
  )
}
