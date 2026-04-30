import { useState } from 'react'
import { joinQueue, simulateGenerate } from './api'

const SERVICES = [
  { label: 'General Banking', value: 0 },
  { label: 'Card Services', value: 1 },
  { label: 'Wealth', value: 2 },
]

export function SimulatorPage() {
  const [count, setCount] = useState(10)
  const [mode, setMode] = useState<'random' | 'manual'>('random')
  const [serviceType, setServiceType] = useState(0)
  const [busy, setBusy] = useState(false)
  const [msg, setMsg] = useState<string | null>(null)
  const [err, setErr] = useState<string | null>(null)

  async function onGenerate() {
    setBusy(true)
    setErr(null)
    setMsg(null)
    try {
      const r =
        mode === 'manual'
          ? await simulateGenerate(count, 'manual', serviceType)
          : await simulateGenerate(count, 'random')
      setMsg(`Generated ${r.generated} customer(s). ${r.message}`)
    } catch (e) {
      setErr(e instanceof Error ? e.message : 'Failed')
    } finally {
      setBusy(false)
    }
  }

  async function oneCustomer(st: number) {
    setBusy(true)
    setErr(null)
    try {
      const r = await joinQueue(st)
      setMsg(
        r.queueBookingBlocked
          ? `Blocked: ${r.message ?? 'capacity'}`
          : `Joined ${r.ticketCode} (${r.serviceType}), position ${r.positionInQueue}`
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
          <h1>Queue request simulator</h1>
          <p className="muted">Generates the same API traffic as customer devices would. Watch the staff dashboard update live.</p>
        </div>
      </header>

      <section className="card">
        <h3>Bulk generation</h3>
        <div className="form-row">
          <label>
            Count
            <input type="number" min={1} max={500} value={count} onChange={(e) => setCount(Number(e.target.value))} />
          </label>
          <label>
            Mode
            <select value={mode} onChange={(e) => setMode(e.target.value as 'random' | 'manual')}>
              <option value="random">Random service mix</option>
              <option value="manual">Single service</option>
            </select>
          </label>
          {mode === 'manual' && (
            <label>
              Service
              <select value={serviceType} onChange={(e) => setServiceType(Number(e.target.value))}>
                {SERVICES.map((s) => (
                  <option key={s.value} value={s.value}>
                    {s.label}
                  </option>
                ))}
              </select>
            </label>
          )}
        </div>
        <button type="button" className="btn primary" disabled={busy} onClick={() => onGenerate()}>
          Generate queue requests
        </button>
      </section>

      <section className="card">
        <h3>Single ticket (manual)</h3>
        <div className="btn-row">
          {SERVICES.map((s) => (
            <button key={s.value} type="button" className="btn outline" disabled={busy} onClick={() => oneCustomer(s.value)}>
              +1 {s.label}
            </button>
          ))}
        </div>
      </section>

      {msg && <div className="alert ok">{msg}</div>}
      {err && <div className="alert error">{err}</div>}
    </div>
  )
}
