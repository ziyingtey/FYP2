import { useCallback, useEffect, useRef, useState } from 'react'
import { joinQueue, simulateGenerate } from './api'

const SERVICES = [
  { label: 'General Banking', value: 0 },
  { label: 'Card Services', value: 1 },
  { label: 'Wealth', value: 2 },
]

const SCENARIOS = [
  { value: '', label: 'Default (base count only)' },
  { value: 'morning', label: 'Morning — low traffic' },
  { value: 'midday', label: 'Midday — moderate' },
  { value: 'peak', label: 'Peak / lunch — high volume' },
  { value: 'evening', label: 'Evening — tapering' },
  { value: 'stress', label: 'Stress / overcrowding' },
]

export function SimulatorPage() {
  const [count, setCount] = useState(10)
  const [mode, setMode] = useState<'random' | 'manual'>('random')
  const [serviceType, setServiceType] = useState(0)
  const [scenario, setScenario] = useState('')
  const [pulseOn, setPulseOn] = useState(false)
  const [pulseIntervalSec, setPulseIntervalSec] = useState(10)
  const [busy, setBusy] = useState(false)
  const [msg, setMsg] = useState<string | null>(null)
  const [err, setErr] = useState<string | null>(null)

  const argsRef = useRef({ count, mode, serviceType, scenario })
  argsRef.current = { count, mode, serviceType, scenario }

  const runningRef = useRef(false)

  const runGenerate = useCallback(async () => {
    if (runningRef.current) return
    runningRef.current = true
    setBusy(true)
    setErr(null)
    const a = argsRef.current
    try {
      const r =
        a.mode === 'manual'
          ? await simulateGenerate(a.count, 'manual', { serviceType: a.serviceType, scenario: a.scenario || undefined })
          : await simulateGenerate(a.count, 'random', { scenario: a.scenario || undefined })
      setMsg(`${r.message} (${r.generated}/${r.requested} tickets)`)
    } catch (e) {
      setErr(e instanceof Error ? e.message : 'Failed')
    } finally {
      runningRef.current = false
      setBusy(false)
    }
  }, [])

  useEffect(() => {
    if (!pulseOn) return
    const ms = Math.max(3000, pulseIntervalSec * 1000)
    const id = window.setInterval(() => void runGenerate(), ms)
    return () => clearInterval(id)
  }, [pulseOn, pulseIntervalSec, runGenerate])

  async function oneCustomer(st: number) {
    setBusy(true)
    setErr(null)
    try {
      const r = await joinQueue(st, true)
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
          <p className="muted">
            Mimics customer arrivals: time-of-day profiles scale batch size and service mix. For a <strong>multi-branch</strong> demo,
            open two browser windows, sign in as each branch&apos;s manager, and run different scenarios side by side.
          </p>
        </div>
      </header>

      <section className="card">
        <h3>Time profile &amp; bulk generation</h3>
        <p className="muted small" style={{ marginTop: 0 }}>
          Profiles adjust the <em>effective</em> batch size (e.g. peak increases count; morning reduces it) and bias random services toward
          realistic mixes.
        </p>
        <div className="form-row">
          <label>
            Base count
            <input type="number" min={1} max={500} value={count} onChange={(e) => setCount(Number(e.target.value))} />
          </label>
          <label>
            Time profile
            <select value={scenario} onChange={(e) => setScenario(e.target.value)}>
              {SCENARIOS.map((s) => (
                <option key={s.value || 'default'} value={s.value}>
                  {s.label}
                </option>
              ))}
            </select>
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
        <div className="btn-row" style={{ flexWrap: 'wrap', marginBottom: '0.75rem' }}>
          {SCENARIOS.filter((s) => s.value).map((s) => (
            <button key={s.value} type="button" className="btn outline" disabled={busy} onClick={() => setScenario(s.value)}>
              {s.label.split(' —')[0]}
            </button>
          ))}
        </div>
        <button type="button" className="btn primary" disabled={busy} onClick={() => void runGenerate()}>
          Generate one batch
        </button>
      </section>

      <section className="card">
        <h3>Continuous pulse</h3>
        <p className="muted small" style={{ marginTop: 0 }}>
          Fires the same bulk generation on a timer (minimum every 3s) so queues ebb and flow like real arrivals.
        </p>
        <div className="form-row">
          <label>
            Interval (seconds)
            <input
              type="number"
              min={3}
              max={120}
              value={pulseIntervalSec}
              onChange={(e) => setPulseIntervalSec(Number(e.target.value))}
            />
          </label>
        </div>
        <div className="btn-row">
          <button type="button" className="btn primary" disabled={pulseOn} onClick={() => setPulseOn(true)}>
            Start pulse
          </button>
          <button type="button" className="btn outline" onClick={() => setPulseOn(false)}>
            Stop pulse
          </button>
        </div>
        {pulseOn && <div className="alert ok">Pulse running every {pulseIntervalSec}s (uses current profile &amp; count).</div>}
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
