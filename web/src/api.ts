import type {
  BranchDetail,
  CrowdLogRow,
  DailyReport,
  PredictionLogRow,
  ServiceCatalogItem,
  SimulationRunRow,
  StaffListItem,
  TicketHistoryRow,
} from './types'
import { BRANCH_ID } from './types'

async function json<T>(res: Response): Promise<T> {
  if (!res.ok) {
    const t = await res.text()
    throw new Error(t || res.statusText)
  }
  return res.json() as Promise<T>
}

export async function fetchDashboard() {
  const res = await fetch(`/api/branches/${BRANCH_ID}/dashboard`)
  return json(res)
}

export interface JoinQueueResult {
  ticketCode: string
  serviceType: string
  positionInQueue: number
  queueBookingBlocked: boolean
  message?: string | null
}

export async function joinQueue(serviceType: number, isSimulated = false) {
  const res = await fetch(`/api/branches/${BRANCH_ID}/queue/join`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ serviceType, isSimulated }),
  })
  return json<JoinQueueResult>(res)
}

export async function callNext(counterId: number) {
  const res = await fetch(`/api/branches/${BRANCH_ID}/queue/call-next`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ counterId }),
  })
  if (!res.ok) throw new Error(await res.text())
}

export async function completeTicket(ticketId: number) {
  const res = await fetch(`/api/branches/${BRANCH_ID}/queue/complete`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ ticketId }),
  })
  if (!res.ok) throw new Error(await res.text())
}

export async function skipTicket(ticketId: number) {
  const res = await fetch(`/api/branches/${BRANCH_ID}/queue/skip`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ ticketId }),
  })
  if (!res.ok) throw new Error(await res.text())
}

export async function recallTicket(ticketId: number) {
  const res = await fetch(`/api/branches/${BRANCH_ID}/queue/recall`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ ticketId }),
  })
  if (!res.ok) throw new Error(await res.text())
}

export async function resetQueue() {
  const res = await fetch(`/api/branches/${BRANCH_ID}/queue/reset`, { method: 'POST' })
  if (!res.ok) throw new Error(await res.text())
}

export async function patchCounter(
  counterId: number,
  body: { isOpen?: boolean; staffAvailable?: boolean; serviceType?: number }
) {
  const res = await fetch(`/api/counters/${counterId}`, {
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
  if (!res.ok) throw new Error(await res.text())
}

export async function simulateGenerate(count: number, mode: 'random' | 'manual', serviceType?: number) {
  const res = await fetch(`/api/branches/${BRANCH_ID}/simulator/generate`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ count, mode, serviceType }),
  })
  return json<{ generated: number; message: string }>(res)
}

export async function fetchBranchInfo() {
  const res = await fetch(`/api/branches/${BRANCH_ID}/info`)
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
  const res = await fetch(`/api/branches/${BRANCH_ID}/settings`, {
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
  return json<BranchDetail>(res)
}

export async function fetchServiceCatalog() {
  const res = await fetch('/api/catalog/services')
  return json<ServiceCatalogItem[]>(res)
}

export async function fetchStaffList() {
  const res = await fetch(`/api/branches/${BRANCH_ID}/staff`)
  return json<StaffListItem[]>(res)
}

export async function fetchCrowdLogs(take = 40) {
  const res = await fetch(`/api/branches/${BRANCH_ID}/insights/crowd-logs?take=${take}`)
  return json<CrowdLogRow[]>(res)
}

export async function fetchPredictionLogs(take = 60) {
  const res = await fetch(`/api/branches/${BRANCH_ID}/insights/prediction-logs?take=${take}`)
  return json<PredictionLogRow[]>(res)
}

export async function fetchSimulationRuns(take = 20) {
  const res = await fetch(`/api/branches/${BRANCH_ID}/insights/simulation-runs?take=${take}`)
  return json<SimulationRunRow[]>(res)
}

export async function fetchTicketHistory(take = 50) {
  const res = await fetch(`/api/branches/${BRANCH_ID}/insights/ticket-history?take=${take}`)
  return json<TicketHistoryRow[]>(res)
}

export async function getDailyReport(date?: string) {
  const q = date ? `?date=${encodeURIComponent(date)}` : ''
  const res = await fetch(`/api/branches/${BRANCH_ID}/reports/daily${q}`)
  if (res.status === 404) return null
  return json<DailyReport>(res)
}

export async function refreshDailyReport(date?: string) {
  const q = date ? `?date=${encodeURIComponent(date)}` : ''
  const res = await fetch(`/api/branches/${BRANCH_ID}/reports/daily/refresh${q}`, { method: 'POST' })
  return json<DailyReport>(res)
}
