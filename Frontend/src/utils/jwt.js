const ROLE_CLAIMS = [
  'role',
  'roles',
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role',
]
const NAME_CLAIMS = [
  'unique_name',
  'name',
  'sub',
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name',
]

export function decodeJwt(token) {
  try {
    const part = token.split('.')[1]
    const base64 = part.replace(/-/g, '+').replace(/_/g, '/')
    const json = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + c.charCodeAt(0).toString(16).padStart(2, '0'))
        .join(''),
    )
    return JSON.parse(json)
  } catch {
    return null
  }
}

export function getRoles(payload) {
  if (!payload) return []
  return ROLE_CLAIMS.flatMap((claim) => [].concat(payload[claim] ?? [])).map((r) =>
    String(r).toLowerCase(),
  )
}

export function getUserName(payload) {
  const claim = NAME_CLAIMS.find((c) => payload?.[c])
  return claim ? payload[claim] : 'Operador'
}

export function isExpired(payload) {
  return typeof payload?.exp === 'number' && payload.exp * 1000 <= Date.now()
}
