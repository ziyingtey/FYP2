const TOKEN = 'bds_token'
const BRANCH = 'bds_branch_id'
const ROLE = 'bds_role'
const NAME = 'bds_staff_name'

export interface SessionPayload {
  accessToken: string
  branchId: number
  role: 'Staff' | 'Manager'
  staffName: string
}

export function saveSession(p: SessionPayload) {
  localStorage.setItem(TOKEN, p.accessToken)
  localStorage.setItem(BRANCH, String(p.branchId))
  localStorage.setItem(ROLE, p.role)
  localStorage.setItem(NAME, p.staffName)
}

export function clearSession() {
  localStorage.removeItem(TOKEN)
  localStorage.removeItem(BRANCH)
  localStorage.removeItem(ROLE)
  localStorage.removeItem(NAME)
}

export function getToken(): string | null {
  return localStorage.getItem(TOKEN)
}

/** Branch for API paths: after login, staff branch; before login, default 1 (demo kiosk). */
export function getApiBranchId(): number {
  const b = localStorage.getItem(BRANCH)
  if (b) {
    const n = parseInt(b, 10)
    if (!Number.isNaN(n) && n > 0) return n
  }
  return 1
}

export function getRole(): 'Staff' | 'Manager' | null {
  const r = localStorage.getItem(ROLE)
  if (r === 'Manager' || r === 'Staff') return r
  return null
}

export function getStaffName(): string | null {
  return localStorage.getItem(NAME)
}

export function isLoggedIn(): boolean {
  return !!getToken() && !!getRole()
}

export function isManager(): boolean {
  return getRole() === 'Manager'
}
