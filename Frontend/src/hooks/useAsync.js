import { useCallback, useEffect, useState } from 'react'

/**
 * Ejecuta `fetcher` (debe ser una función estable: módulo o useCallback) y expone
 * { data, error, loading, reload }. `data` conserva el último resultado mientras recarga.
 */
export function useAsync(fetcher) {
  const [tick, setTick] = useState(0)
  const [result, setResult] = useState({ fetcher: null, tick: -1, data: null, error: null })

  useEffect(() => {
    let active = true
    fetcher()
      .then((data) => active && setResult({ fetcher, tick, data, error: null }))
      .catch((error) => active && setResult((prev) => ({ fetcher, tick, data: prev.data, error })))
    return () => {
      active = false
    }
  }, [fetcher, tick])

  const reload = useCallback(() => setTick((t) => t + 1), [])
  const loading = result.fetcher !== fetcher || result.tick !== tick

  return { data: result.data, error: loading ? null : result.error, loading, reload }
}
