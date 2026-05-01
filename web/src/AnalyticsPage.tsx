import { useCallback, useEffect, useState } from 'react'
import {
  fetchCrowdLogs,
  fetchPredictionLogs,
  fetchSimulationRuns,
  fetchTicketHistory,
  getDailyReport,
  refreshDailyReport,
} from './api'
import type {
  CrowdLogRow,
  DailyReport,
  PredictionLogRow,
  SimulationRunRow,
  TicketHistoryRow,
} from './types'

export function AnalyticsPage() {
  const [busy, setBusy] = useState(false)
  const [err, setErr] = useState<string | null>(null)
  const [report, setReport] = useState<DailyReport | null>(null)
  const [crowd, setCrowd] = useState<CrowdLogRow[]>([])
  const [pred, setPred] = useState<PredictionLogRow[]>([])
  const [runs, setRuns] = useState<SimulationRunRow[]>([])
  const [tickets, setTickets] = useState<TicketHistoryRow[]>([])

  const load = useCallback(async () => {
    setErr(null)
    setBusy(true)
    try {
      const [c, p, r, t, rep] = await Promise.all([
        fetchCrowdLogs(30),
        fetchPredictionLogs(40),
        fetchSimulationRuns(15),
        fetchTicketHistory(40),
        getDailyReport(),
      ])
      setCrowd(c)
      setPred(p)
      setRuns(r)
      setTickets(t)
      setReport(rep)
    } catch (e) {
      setErr(e instanceof Error ? e.message : 'Load failed')
    } finally {
      setBusy(false)
    }
  }, [])

  useEffect(() => {
    void load()
  }, [load])

  async function onRefreshReport() {
    setBusy(true)
    setErr(null)
    try {
      const rep = await refreshDailyReport()
      setReport(rep)
    } catch (e) {
      setErr(e instanceof Error ? e.message : 'Report refresh failed')
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="stack">
      <header className="hero">
        <div>
          <h1>Reporting &amp; analytics</h1>
          <p className="muted">Daily summary, footfall logs, prediction history, simulator runs, and ticket audit.</p>
        </div>
        <button type="button" className="btn primary" disabled={busy} onClick={() => void onRefreshReport()}>
          Refresh daily report
        </button>
      </header>

      {err && <div className="alert error">{err}</div>}

      <section className="card">
        <h3>Daily report (UTC day)</h3>
        {!report ? (
          <p className="muted">No report row yet — click &quot;Refresh daily report&quot; to compute from completed tickets.</p>
        ) : (
          <div className="big-metrics">
            <div>
              <span className="muted">Date</span>
              <div className="num small">{report.reportDate}</div>
            </div>
            <div>
              <span className="muted">Served</span>
              <div className="num">{report.totalCustomersServed}</div>
            </div>
            <div>
              <span className="muted">Avg wait</span>
              <div className="num">{report.avgWaitMinutes} min</div>
            </div>
            <div>
              <span className="muted">Peak hour</span>
              <div className="num small">{report.peakHour ?? '—'}</div>
            </div>
            <div>
              <span className="muted">Busiest service</span>
              <div className="num small">{report.busiestServiceCode ?? '—'}</div>
            </div>
          </div>
        )}
      </section>

      <section className="grid2">
        <div className="card">
          <h3>Crowd log</h3>
          <table className="table compact">
            <thead>
              <tr>
                <th>Time (UTC)</th>
                <th>People</th>
                <th>Level</th>
              </tr>
            </thead>
            <tbody>
              {crowd.map((x) => (
                <tr key={x.id}>
                  <td>{new Date(x.timestampUtc).toLocaleString()}</td>
                  <td>{x.totalCustomers}</td>
                  <td>{x.crowdLevel}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        <div className="card">
          <h3>Simulation runs</h3>
          <table className="table compact">
            <thead>
              <tr>
                <th>Time</th>
                <th>Gen / req</th>
                <th>Mode</th>
                <th>Scenario</th>
              </tr>
            </thead>
            <tbody>
              {runs.map((x) => (
                <tr key={x.id}>
                  <td>{new Date(x.createdUtc).toLocaleString()}</td>
                  <td>
                    {x.generatedCount}/{x.requestedCount}
                  </td>
                  <td>
                    {x.mode}
                    {x.fixedServiceCode ? ` · ${x.fixedServiceCode}` : ''}
                  </td>
                  <td>{x.scenario ?? '—'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>

      <section className="card">
        <h3>Prediction log (recent)</h3>
        <table className="table compact">
          <thead>
            <tr>
              <th>Time</th>
              <th>Svc</th>
              <th>QLen</th>
              <th>Est. avg</th>
              <th>Est. clear</th>
            </tr>
          </thead>
          <tbody>
            {pred.map((x) => (
              <tr key={x.id}>
                <td>{new Date(x.timestampUtc).toLocaleTimeString()}</td>
                <td>{x.serviceCode}</td>
                <td>{x.queueLength}</td>
                <td>{x.estimatedAvgWaitMinutes}</td>
                <td>{x.estimatedClearingMinutes}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>

      <section className="card">
        <h3>Ticket history</h3>
        <table className="table compact">
          <thead>
            <tr>
              <th>Code</th>
              <th>Service</th>
              <th>Status</th>
              <th>Sim?</th>
            </tr>
          </thead>
          <tbody>
            {tickets.map((x) => (
              <tr key={x.id}>
                <td>{x.ticketCode}</td>
                <td>{x.serviceCode}</td>
                <td>{x.status}</td>
                <td>{x.isSimulated ? 'yes' : 'no'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </div>
  )
}
