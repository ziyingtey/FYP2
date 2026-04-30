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

export async function joinQueue(serviceType: number) {
  const res = await fetch(`/api/branches/${BRANCH_ID}/queue/join`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ serviceType }),
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
