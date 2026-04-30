import { useState } from 'react'
import type { BranchDashboard, CounterState } from './types'
import {
  callNext,
  completeTicket,
  patchCounter,
  recallTicket,
  resetQueue,
  skipTicket,
} from './api'

function crowdColor(level: string) {
  const u = level.toLowerCase()
  if (u.includes('over')) return 'var(--crowd-over)'
  if (u === 'high') return 'var(--crowd-high)'
  if (u === 'medium') return 'var(--crowd-med)'
  return 'var(--crowd-low)'
}

export function DashboardPage({
  data,
  connState,
}: {
  data: BranchDashboard | null
  connState: string
}) {
  const [busy, setBusy] = useState<string | null>(null)
  const [err, setErr] = useState<string | null>(null)

  async function run(label: string, fn: () => Promise<void>) {
    setBusy(label)
    setErr(null)
    try {
      await fn()
    } catch (e) {
      setErr(e instanceof Error ? e.message : 'Action failed')
    } finally {
      setBusy(null)
    }
  }

  if (!data) {
    return (
      <div className="panel">
        <p className="muted">Loading dashboard… ({connState})</p>
      </div>
    )
  }

  return (
    <div className="stack">
      <header className="hero">
        <div>
          <h1>{data.branchName}</h1>
          <p className="muted">
            {data.location ? `${data.location} · ` : ''}
            Staff dashboard · SignalR: <strong>{connState}</strong>
          </p>
          <p className="muted small">
            Crowd bands: Medium ≥{data.crowdMediumStartsAtPercent}% · High ≥{data.crowdHighStartsAtPercent}% · Overcrowded ≥
            {data.overcrowdStartsAtPercent}%
          </p>
        </div>
        <div className="crowd-pill" style={{ borderColor: crowdColor(data.crowdLevel) }}>
          <span>Crowd</span>
          <strong style={{ color: crowdColor(data.crowdLevel) }}>{data.crowdLevel}</strong>
        </div>
      </header>

      {data.queueBookingBlocked && (
        <div className="alert">
          <strong>Capacity control:</strong> {data.bookingBlockReason ?? 'Branch full — new queue tickets blocked.'}
        </div>
      )}
      {err && <div className="alert error">{err}</div>}

      <section className="grid2">
        <div className="card">
          <h3>Branch occupancy</h3>
          <div className="big-metrics">
            <div>
              <span className="muted">Current</span>
              <div className="num">{data.occupancy}</div>
            </div>
            <div>
              <span className="muted">Capacity</span>
              <div className="num">{data.maxCapacity}</div>
            </div>
            <div>
              <span className="muted">Load</span>
              <div className="num">{data.occupancyPercent}%</div>
            </div>
          </div>
          <div className="bar">
            <div className="bar-fill" style={{ width: `${Math.min(100, data.occupancyPercent)}%` }} />
          </div>
        </div>

        <div className="card">
          <h3>Queue performance (ML-assisted)</h3>
          <table className="table">
            <thead>
              <tr>
                <th>Service</th>
                <th>Waiting</th>
                <th>Serving</th>
                <th>Est. avg wait</th>
                <th>Est. clearing</th>
              </tr>
            </thead>
            <tbody>
              {data.services.map((s) => (
                <tr key={s.serviceType}>
                  <td>{s.displayName}</td>
                  <td>{s.waiting}</td>
                  <td>{s.servingTicketCode ?? '—'}</td>
                  <td>{s.estimatedAvgWaitMinutes} min</td>
                  <td>{s.estimatedClearingMinutes} min</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>

      <section className="card">
        <div className="row-between">
          <h3>Counters & queue actions</h3>
          <button
            type="button"
            className="btn danger outline"
            disabled={!!busy}
            onClick={() => run('reset', resetQueue)}
          >
            Reset queue (daily)
          </button>
        </div>
        <div className="counter-grid">
          {data.counters.map((c) => (
            <CounterCard key={c.id} c={c} busy={busy} onRun={run} />
          ))}
        </div>
      </section>

      <section className="card">
        <h3>Recent tickets</h3>
        <table className="table compact">
          <thead>
            <tr>
              <th>Ticket</th>
              <th>Service</th>
              <th>Status</th>
              <th>Time (UTC)</th>
            </tr>
          </thead>
          <tbody>
            {data.recentTickets.map((t) => (
              <tr key={`${t.ticketCode}-${t.createdUtc}`}>
                <td>{t.ticketCode}</td>
                <td>{t.serviceType}</td>
                <td>{t.status}</td>
                <td>{new Date(t.createdUtc).toLocaleTimeString()}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </div>
  )
}

function CounterCard({
  c,
  busy,
  onRun,
}: {
  c: CounterState
  busy: string | null
  onRun: (label: string, fn: () => Promise<void>) => void
}) {
  const ticketId = c.currentTicketId
  const canServe = c.isOpen && c.staffAvailable

  return (
    <div className={`counter-card ${canServe ? '' : 'inactive'}`}>
      <div className="row-between">
        <strong>{c.label}</strong>
        <span className="tag">{c.serviceType}</span>
      </div>
      <div className="muted small">
        Counter open: {c.isOpen ? 'yes' : 'no'} · Staff: {c.staffAvailable ? 'available' : 'break'}
      </div>
      <div className="now-serving">
        Now serving: <strong>{c.currentTicketCode ?? '—'}</strong>
      </div>
      <div className="btn-row">
        <button type="button" className="btn small" disabled={!!busy} onClick={() => onRun(`open-${c.id}`, () => patchCounter(c.id, { isOpen: true }))}>
          Open
        </button>
        <button type="button" className="btn small outline" disabled={!!busy} onClick={() => onRun(`close-${c.id}`, () => patchCounter(c.id, { isOpen: false }))}>
          Close
        </button>
        <button
          type="button"
          className="btn small outline"
          disabled={!!busy}
          onClick={() => onRun(`break-${c.id}`, () => patchCounter(c.id, { staffAvailable: !c.staffAvailable }))}
        >
          Toggle lunch
        </button>
      </div>
      <div className="btn-row">
        <button type="button" className="btn primary" disabled={!!busy || !canServe} onClick={() => onRun(`next-${c.id}`, () => callNext(c.id))}>
          Call next
        </button>
        <button
          type="button"
          className="btn outline"
          disabled={!!busy || ticketId == null}
          onClick={() => onRun(`done-${c.id}`, () => completeTicket(ticketId!))}
        >
          Complete
        </button>
        <button type="button" className="btn outline" disabled={!!busy || ticketId == null} onClick={() => onRun(`skip-${c.id}`, () => skipTicket(ticketId!))}>
          Skip
        </button>
        <button type="button" className="btn outline" disabled={!!busy || ticketId == null} onClick={() => onRun(`recall-${c.id}`, () => recallTicket(ticketId!))}>
          Recall
        </button>
      </div>
    </div>
  )
}
