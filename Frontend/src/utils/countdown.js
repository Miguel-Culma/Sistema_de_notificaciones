// Matemática de los temporizadores en bucle. Son funciones puras: no usan reloj
// ni React, así es fácil probarlas.

// Dado el tiempo transcurrido desde que arrancó el proceso y la duración de un
// ciclo (ambos en segundos), calcula en qué punto del ciclo actual estamos.
//   elapsed = 0   → recién arrancó, falta el ciclo completo
//   elapsed = 60  con period 60 → el ciclo terminó y empieza otro (bucle)
export function cycleState(elapsed, period) {
  const elapsedInCycle = elapsed % period
  return {
    elapsedInCycle,
    remaining: period - elapsedInCycle,
    cycles: Math.floor(elapsed / period), // cuántas vueltas completas lleva
  }
}

// 75 → "01:15"   |   3725 → "1:02:05"
export function formatClock(totalSeconds) {
  const s = Math.max(0, Math.floor(totalSeconds))
  const h = Math.floor(s / 3600)
  const m = Math.floor((s % 3600) / 60)
  const sec = s % 60
  const mm = String(m).padStart(2, '0')
  const ss = String(sec).padStart(2, '0')
  return h > 0 ? `${h}:${mm}:${ss}` : `${mm}:${ss}`
}
