import type {
  BranchDetail,
  BranchSummary,
  CrowdLogRow,
  DailyReport,
  PredictionLogRow,
  ServiceCatalogItem,
  SimulationRunRow,
  StaffListItem,
  TicketHistoryRow,
} from './types'
import { getApiBranchId, getToken } from './session'

function authHeaders(): HeadersInit {
  const t = getToken()
  return t ? { Authorization: `Bearer ${t}` } : {}
}

async function json<T>(res: Response): Promise<T> {
  if (!res.ok) {
    const t = await res.text()
    throw new Error(t || res.statusText)
  }
  return res.json() as Promise<T>
}

export interface LoginResponse {
  accessToken: string
  expiresAtUtc: string
  staffId: number
  staffName: string
  role: string
  branchId: number
  branchName: string
}

export async function login(email: string, password: string) {
  const res = await fetch('/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  })
  return json<LoginResponse>(res)
}

export async function fetchDashboard() {
  const id = getApiBranchId()
  const res = await fetch(`/api/branches/${id}/dashboard`, { headers: { ...authHeaders() } })
  return json(res)
}

export interface JoinQueueResult {
  ticketCode: string
  serviceType: string
  positionInQueue: number
  queueBookingBlocked: boolean
  message?: string | null
}

/** Public join (no JWT) — uses current branch id from session if set, else branch 1. */
export async function joinQueue(serviceType: number, isSimulated = false) {
  const id = getApiBranchId()
  const res = await fetch(`/api/branches/${id}/queue/join`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ serviceType, isSimulated }),
  })
  return json<JoinQueueResult>(res)
}

export async function callNext(counterId: number) {
  const id = getApiBranchId()
  const res = await fetch(`/api/branches/${id}/queue/call-next`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify({ counterId }),
  })
  if (!res.ok) throw new Error(await res.text())
}

export async function completeTicket(ticketId: number) {
  const id = getApiBranchId()
  const res = await fetch(`/api/branches/${id}/queue/complete`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify({ ticketId }),
  })
  if (!res.ok) throw new Error(await res.text())
}

export async function skipTicket(ticketId: number) {
  const id = getApiBranchId()
  const res = await fetch(`/api/branches/${id}/queue/skip`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify({ ticketId }),
  })
  if (!res.ok) throw new Error(await res.text())
}

export async function recallTicket(ticketId: number) {
  const id = getApiBranchId()
  const res = await fetch(`/api/branches/${id}/queue/recall`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify({ ticketId }),
  })
  if (!res.ok) throw new Error(await res.text())
}

export async function resetQueue() {
  const id = getApiBranchId()
  const res = await fetch(`/api/branches/${id}/queue/reset`, { method: 'POST', headers: { ...authHeaders() } })
  if (!res.ok) throw new Error(await res.text())
}

export async function patchCounter(
  counterId: number,
  body: { isOpen?: boolean; staffAvailable?: boolean; serviceType?: number }
) {
  const res = await fetch(`/api/counters/${counterId}`, {
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify(body),
  })
  if (!res.ok) throw new Error(await res.text())
}

export interface SimulateGenerateResponse {
  generated: number
  requested: number
  scenario?: string | null
  message: string
}

export async function simulateGenerate(
  count: number,
  mode: 'random' | 'manual',
  opts?: { serviceType?: number; scenario?: string | null }
) {
  const id = getApiBranchId()
  const body: Record<string, unknown> = { count, mode }
  if (mode === 'manual' && opts?.serviceType !== undefined) body.serviceType = opts.serviceType
  if (opts?.scenario) body.scenario = opts.scenario
  const res = await fetch(`/api/branches/${id}/simulator/generate`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify(body),
  })
  return json<SimulateGenerateResponse>(res)
}

/** Live occupancy per branch — use on login or manager overview for multi-branch demos. */
export async function fetchBranchDirectory() {
  const res = await fetch('/api/branches')
  return json<BranchSummary[]>(res)
}

export async function fetchBranchInfo() {
  const id = getApiBranchId()
  const res = await fetch(`/api/branches/${id}/info`, { headers: { ...authHeaders() } })
  return json<BranchDetail>(res)
}

export async function patchBranchSettings(body: {
  name?: string
  location?: string
  maxCapacity?: number
  crowdMediumStartsAtPercent?: number
  crowdHighStartsAtPercent?: number
  overcrowdStartsAtPercent?: number
}) {
  const id = getApiBranchId()
  const res = await fetch(`/api/branches/${id}/settings`, {
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify(body),
  })
  return json<BranchDetail>(res)
}

export async function fetchServiceCatalog() {
  const res = await fetch('/api/catalog/services')
  return json<ServiceCatalogItem[]>(res)
}

export async function fetchStaffList() {
  const id = getApiBranchId()
  const res = await fetch(`/api/branches/${id}/staff`, { headers: { ...authHeaders() } })
  return json<StaffListItem[]>(res)
}

export async function fetchCrowdLogs(take = 40) {
  const id = getApiBranchId()
  const res = await fetch(`/api/branches/${id}/insights/crowd-logs?take=${take}`, { headers: { ...authHeaders() } })
  return json<CrowdLogRow[]>(res)
}

export async function fetchPredictionLogs(take = 60) {
  const id = getApiBranchId()
  const res = await fetch(`/api/branches/${id}/insights/prediction-logs?take=${take}`, { headers: { ...authHeaders() } })
  return json<PredictionLogRow[]>(res)
}

export async function fetchSimulationRuns(take = 20) {
  const id = getApiBranchId()
  const res = await fetch(`/api/branches/${id}/insights/simulation-runs?take=${take}`, { headers: { ...authHeaders() } })
  return json<SimulationRunRow[]>(res)
}

export async function fetchTicketHistory(take = 50) {
  const id = getApiBranchId()
  const res = await fetch(`/api/branches/${id}/insights/ticket-history?take=${take}`, { headers: { ...authHeaders() } })
  return json<TicketHistoryRow[]>(res)
}

export async function getDailyReport(date?: string) {
  const id = getApiBranchId()
  const q = date ? `?date=${encodeURIComponent(date)}` : ''
  const res = await fetch(`/api/branches/${id}/reports/daily${q}`, { headers: { ...authHeaders() } })
  if (res.status === 404) return null
  return json<DailyReport>(res)
}

export async function refreshDailyReport(date?: string) {
  const id = getApiBranchId()
  const q = date ? `?date=${encodeURIComponent(date)}` : ''
  const res = await fetch(`/api/branches/${id}/reports/daily/refresh${q}`, { method: 'POST', headers: { ...authHeaders() } })
  return json<DailyReport>(res)
}
