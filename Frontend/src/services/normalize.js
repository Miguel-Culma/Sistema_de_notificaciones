// Los nombres exactos de los campos dependen del backend; estos helpers aceptan variantes comunes.
export const pick = (obj, ...keys) => {
  for (const key of keys) {
    if (obj?.[key] !== undefined && obj[key] !== null) return obj[key]
  }
  return undefined
}

export const toList = (data) => (Array.isArray(data) ? data : (data?.items ?? data?.data ?? []))
