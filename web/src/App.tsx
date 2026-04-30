import { useState } from 'react'
import './App.css'
import { DashboardPage } from './DashboardPage'
import { SimulatorPage } from './SimulatorPage'
import { useQueueHub } from './useQueueHub'

export default function App() {
  const [tab, setTab] = useState<'staff' | 'sim'>('staff')
  const { data, connState, error } = useQueueHub()

  return (
    <div className="app-shell">
      <nav className="top-nav">
        <div className="brand">Branch Queue System</div>
        <div className="tabs">
          <button type="button" className={tab === 'staff' ? 'active' : ''} onClick={() => setTab('staff')}>
            Staff dashboard
          </button>
          <button type="button" className={tab === 'sim' ? 'active' : ''} onClick={() => setTab('sim')}>
            Customer simulator
          </button>
        </div>
      </nav>

      <main className="main">
        {error && tab === 'staff' && <div className="alert error">{error}</div>}
        {tab === 'staff' ? <DashboardPage data={data} connState={connState} /> : <SimulatorPage />}
      </main>
    </div>
  )
}
