import { useState } from 'react'
import './App.css'
import { AnalyticsPage } from './AnalyticsPage'
import { CustomerPage } from './CustomerPage'
import { DashboardPage } from './DashboardPage'
import { LoginPage } from './LoginPage'
import { ManagerPage } from './ManagerPage'
import { SimulatorPage } from './SimulatorPage'
import { clearSession, getStaffName, isLoggedIn, isManager } from './session'
import { useQueueHub } from './useQueueHub'

type Tab = 'staff' | 'customer' | 'sim' | 'manager' | 'analytics'

function BankPortal({ onLogout }: { onLogout: () => void }) {
  const [tab, setTab] = useState<Tab>('staff')
  const { data, connState, error } = useQueueHub()
  const manager = isManager()
  const name = getStaffName()

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
          {manager && (
            <button type="button" className={tab === 'sim' ? 'active' : ''} onClick={() => setTab('sim')}>
              Simulator
            </button>
          )}
          {manager && (
            <button type="button" className={tab === 'manager' ? 'active' : ''} onClick={() => setTab('manager')}>
              Manager
            </button>
          )}
          {manager && (
            <button type="button" className={tab === 'analytics' ? 'active' : ''} onClick={() => setTab('analytics')}>
              Analytics
            </button>
          )}
        </div>
        <div className="nav-user">
          <span className="muted small">{name}</span>
          <span className="tag">{manager ? 'Manager' : 'Staff'}</span>
          <button type="button" className="btn small outline" onClick={onLogout}>
            Sign out
          </button>
        </div>
      </nav>

      <main className="main">
        {error && tab === 'staff' && <div className="alert error">{error}</div>}
        {tab === 'staff' && <DashboardPage data={data} connState={connState} />}
        {tab === 'customer' && <CustomerPage />}
        {manager && tab === 'sim' && <SimulatorPage />}
        {manager && tab === 'manager' && <ManagerPage />}
        {manager && tab === 'analytics' && <AnalyticsPage />}
      </main>
    </div>
  )
}

export default function App() {
  const [loggedIn, setLoggedIn] = useState(() => isLoggedIn())

  if (!loggedIn) {
    return (
      <div className="app-shell">
        <LoginPage onLoggedIn={() => setLoggedIn(true)} />
      </div>
    )
  }

  return (
    <BankPortal
      onLogout={() => {
        clearSession()
        setLoggedIn(false)
      }}
    />
  )
}
