import { useState } from 'react'
import { joinQueue } from './api'

const SERVICES = [
  { label: 'General Banking', value: 0 },
  { label: 'Card Services', value: 1 },
  { label: 'Wealth', value: 2 },
]

/** Public customer flow — tickets are not marked simulated (unlike stress-test simulator). */
export function CustomerPage() {
  const [busy, setBusy] = useState(false)
  const [msg, setMsg] = useState<string | null>(null)
  const [err, setErr] = useState<string | null>(null)

  async function takeTicket(serviceType: number) {
    setBusy(true)
    setErr(null)
    setMsg(null)
    try {
      const r = await joinQueue(serviceType, false)
      setMsg(
        r.queueBookingBlocked
          ? `Cannot join: ${r.message ?? 'branch at capacity'}`
          : `Your ticket is ${r.ticketCode} (${r.serviceType}). You are #${r.positionInQueue} in line for this service.`
      )
    } catch (e) {
      setErr(e instanceof Error ? e.message : 'Failed')
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="stack">
      <header className="hero">
        <div>
          <h1>Get a queue ticket</h1>
          <p className="muted">Choose your service. Same API as a kiosk or mobile app would use.</p>
        </div>
      </header>

      {err && <div className="alert error">{err}</div>}
      {msg && <div className="alert">{msg}</div>}

      <section className="card">
        <h3>Select service</h3>
        <div className="btn-row">
          {SERVICES.map((s) => (
            <button key={s.value} type="button" className="btn primary outline" disabled={busy} onClick={() => takeTicket(s.value)}>
              {s.label}
            </button>
          ))}
        </div>
      </section>
    </div>
  )
}
