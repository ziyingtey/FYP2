import { useEffect, useState, type FormEvent } from 'react'
import { fetchBranchDirectory, login } from './api'
import type { BranchSummary } from './types'
import { saveSession } from './session'

export function LoginPage({ onLoggedIn }: { onLoggedIn: () => void }) {
  const [email, setEmail] = useState('aisha.staff@bds.demo')
  const [password, setPassword] = useState('')
  const [err, setErr] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)
  const [branches, setBranches] = useState<BranchSummary[] | null>(null)

  useEffect(() => {
    void fetchBranchDirectory()
      .then(setBranches)
      .catch(() => setBranches([]))
  }, [])

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setBusy(true)
    setErr(null)
    try {
      const r = await login(email.trim(), password)
      saveSession({
        accessToken: r.accessToken,
        branchId: r.branchId,
        role: r.role as 'Staff' | 'Manager',
        staffName: r.staffName,
      })
      onLoggedIn()
    } catch (x) {
      setErr(x instanceof Error ? x.message : 'Login failed')
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="main stack" style={{ maxWidth: 520 }}>
      <header className="hero">
        <div>
          <h1>Branch portal sign-in</h1>
          <p className="muted">Staff operate the queue; branch managers have simulator, policy, and analytics access.</p>
        </div>
      </header>

      <section className="card">
        <h3>Multi-branch demo</h3>
        <p className="muted small" style={{ marginTop: 0 }}>
          Seed data includes three branches. Open <strong>two browser windows</strong>, sign in as a different branch manager in each, and
          run the simulator on one branch while watching the other stay calm — that is your problem vs control narrative.
        </p>
        {branches && branches.length > 0 && (
          <div style={{ overflowX: 'auto', marginBottom: '1rem' }}>
            <table className="table compact">
              <thead>
                <tr>
                  <th>Branch</th>
                  <th>Occ.</th>
                  <th>Crowd</th>
                </tr>
              </thead>
              <tbody>
                {branches.map((b) => (
                  <tr key={b.branchId}>
                    <td>
                      <strong>{b.name}</strong>
                      <div className="muted small">id {b.branchId}</div>
                    </td>
                    <td>
                      {b.occupancy} / {b.maxCapacity} ({b.occupancyPercent}%)
                    </td>
                    <td>
                      {b.crowdLevel}
                      {b.queueBookingBlocked ? ' · full' : ''}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <section className="card">
        <h3>Demo accounts (password pattern)</h3>
        <p className="muted small" style={{ marginTop: 0 }}>
          Floor staff: <code>StaffDemo#1</code> · Managers: <code>ManagerDemo#1</code>
        </p>
        <ul className="muted small" style={{ margin: '0 0 1rem', paddingLeft: '1.2rem' }}>
          <li>
            <strong>Main Branch (KLCC)</strong> — staff: aisha.staff@bds.demo · manager: marcus.manager@bds.demo
          </li>
          <li>
            <strong>Mid Valley Megamall</strong> — staff: preeti.staff@bds.demo · manager: ravi.manager@bds.demo
          </li>
          <li>
            <strong>Gurney Plaza Penang</strong> — staff: hani.staff@bds.demo · manager: najib.manager@bds.demo
          </li>
        </ul>
        <form className="stack" onSubmit={(e) => void onSubmit(e)}>
          {err && <div className="alert error">{err}</div>}
          <label className="stack" style={{ gap: '0.25rem' }}>
            <span className="muted small">Work email</span>
            <input autoComplete="username" value={email} onChange={(e) => setEmail(e.target.value)} required />
          </label>
          <label className="stack" style={{ gap: '0.25rem' }}>
            <span className="muted small">Password</span>
            <input
              type="password"
              autoComplete="current-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </label>
          <button type="submit" className="btn primary" disabled={busy}>
            {busy ? 'Signing in…' : 'Sign in'}
          </button>
        </form>
      </section>
    </div>
  )
}
