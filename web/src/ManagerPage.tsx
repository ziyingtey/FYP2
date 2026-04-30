import { useEffect, useState } from 'react'
import { fetchBranchInfo, fetchStaffList, patchBranchSettings } from './api'
import type { BranchDetail, StaffListItem } from './types'

export function ManagerPage() {
  const [loading, setLoading] = useState(true)
  const [err, setErr] = useState<string | null>(null)
  const [saved, setSaved] = useState<string | null>(null)
  const [staff, setStaff] = useState<StaffListItem[]>([])
  const [form, setForm] = useState<BranchDetail | null>(null)

  useEffect(() => {
    let cancelled = false
    ;(async () => {
      try {
        const [b, s] = await Promise.all([fetchBranchInfo(), fetchStaffList()])
        if (!cancelled) {
          setForm(b)
          setStaff(s)
        }
      } catch (e) {
        if (!cancelled) setErr(e instanceof Error ? e.message : 'Load failed')
      } finally {
        if (!cancelled) setLoading(false)
      }
    })()
    return () => {
      cancelled = true
    }
  }, [])

  async function save() {
    if (!form) return
    setErr(null)
    setSaved(null)
    try {
      const b = await patchBranchSettings({
        name: form.name,
        location: form.location,
        maxCapacity: form.maxCapacity,
        crowdMediumStartsAtPercent: form.crowdMediumStartsAtPercent,
        crowdHighStartsAtPercent: form.crowdHighStartsAtPercent,
        overcrowdStartsAtPercent: form.overcrowdStartsAtPercent,
      })
      setForm(b)
      setSaved('Saved. Dashboard will refresh for all connected staff.')
    } catch (e) {
      setErr(e instanceof Error ? e.message : 'Save failed')
    }
  }

  if (loading || !form) {
    return (
      <div className="panel">
        <p className="muted">{loading ? 'Loading branch settings…' : 'No data'}</p>
        {err && <div className="alert error">{err}</div>}
      </div>
    )
  }

  return (
    <div className="stack">
      <header className="hero">
        <div>
          <h1>Branch manager</h1>
          <p className="muted">Capacity, location, and crowd band thresholds (used for Low / Medium / High / Overcrowded).</p>
        </div>
      </header>

      {err && <div className="alert error">{err}</div>}
      {saved && <div className="alert">{saved}</div>}

      <section className="card">
        <h3>Branch profile</h3>
        <div className="form-row">
          <label>
            Name
            <input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
          </label>
          <label>
            Location
            <input value={form.location} onChange={(e) => setForm({ ...form, location: e.target.value })} />
          </label>
          <label>
            Max capacity
            <input
              type="number"
              min={1}
              value={form.maxCapacity}
              onChange={(e) => setForm({ ...form, maxCapacity: Number(e.target.value) || 1 })}
            />
          </label>
        </div>
      </section>

      <section className="card">
        <h3>Crowd thresholds (% of capacity)</h3>
        <p className="muted small">Rule: medium &lt; high &lt; overcrowd, each between 0 and 100.</p>
        <div className="form-row">
          <label>
            Medium from
            <input
              type="number"
              min={0}
              max={99}
              value={form.crowdMediumStartsAtPercent}
              onChange={(e) => setForm({ ...form, crowdMediumStartsAtPercent: Number(e.target.value) })}
            />
          </label>
          <label>
            High from
            <input
              type="number"
              min={1}
              max={99}
              value={form.crowdHighStartsAtPercent}
              onChange={(e) => setForm({ ...form, crowdHighStartsAtPercent: Number(e.target.value) })}
            />
          </label>
          <label>
            Overcrowded from
            <input
              type="number"
              min={1}
              max={100}
              value={form.overcrowdStartsAtPercent}
              onChange={(e) => setForm({ ...form, overcrowdStartsAtPercent: Number(e.target.value) })}
            />
          </label>
        </div>
        <button type="button" className="btn primary" onClick={() => void save()}>
          Save settings
        </button>
      </section>

      <section className="card">
        <h3>Staff &amp; counters</h3>
        <table className="table compact">
          <thead>
            <tr>
              <th>Name</th>
              <th>Role</th>
              <th>Counter</th>
            </tr>
          </thead>
          <tbody>
            {staff.map((s) => (
              <tr key={s.id}>
                <td>{s.name}</td>
                <td>{s.role}</td>
                <td>{s.assignedCounterLabel ?? '—'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </div>
  )
}
