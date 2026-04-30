import { useState } from 'react'
import './App.css'
import { AnalyticsPage } from './AnalyticsPage'
import { CustomerPage } from './CustomerPage'
import { DashboardPage } from './DashboardPage'
import { ManagerPage } from './ManagerPage'
import { SimulatorPage } from './SimulatorPage'
import { useQueueHub } from './useQueueHub'

type Tab = 'staff' | 'customer' | 'sim' | 'manager' | 'analytics'

export default function App() {
  const [tab, setTab] = useState<Tab>('staff')
  const { data, connState, error } = useQueueHub()

  return (
    <div className="app-shell">
      <nav className="top-nav">
        <div className="brand">Branch Queue System</div>
        <div className="tabs">
          <button type="button" className={tab === 'staff' ? 'active' : ''} onClick={() => setTab('staff')}>
            Staff
          </button>
          <button type="button" className={tab === 'customer' ? 'active' : ''} onClick={() => setTab('customer')}>
            Customer
          </button>
          <button type="button" className={tab === 'sim' ? 'active' : ''} onClick={() => setTab('sim')}>
            Simulator
          </button>
          <button type="button" className={tab === 'manager' ? 'active' : ''} onClick={() => setTab('manager')}>
            Manager
          </button>
          <button type="button" className={tab === 'analytics' ? 'active' : ''} onClick={() => setTab('analytics')}>
            Analytics
          </button>
        </div>
      </nav>

      <main className="main">
        {error && tab === 'staff' && <div className="alert error">{error}</div>}
        {tab === 'staff' && <DashboardPage data={data} connState={connState} />}
        {tab === 'customer' && <CustomerPage />}
        {tab === 'sim' && <SimulatorPage />}
        {tab === 'manager' && <ManagerPage />}
        {tab === 'analytics' && <AnalyticsPage />}
      </main>
    </div>
  )
}
