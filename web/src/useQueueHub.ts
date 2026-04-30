import { useEffect, useState } from 'react'
import * as signalR from '@microsoft/signalr'
import type { BranchDashboard } from './types'
import { BRANCH_ID } from './types'
import { fetchDashboard } from './api'

export function useQueueHub() {
  const [data, setData] = useState<BranchDashboard | null>(null)
  const [connState, setConnState] = useState<string>('connecting')
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false
    ;(async () => {
      try {
        const initial = (await fetchDashboard()) as BranchDashboard
        if (!cancelled) setData(initial)
      } catch (e) {
        if (!cancelled) setError(e instanceof Error ? e.message : 'Load failed')
      }
    })()

    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/queue')
      .withAutomaticReconnect()
      .build()

    connection.on('dashboard', (dto: BranchDashboard) => {
      setData(dto)
      setError(null)
    })

    connection.onreconnecting(() => setConnState('reconnecting'))
    connection.onreconnected(() => setConnState('connected'))
    connection.onclose(() => setConnState('disconnected'))

    connection
      .start()
      .then(() => {
        setConnState('connected')
        return connection.invoke('SubscribeBranch', BRANCH_ID)
      })
      .catch((e) => {
        setConnState('error')
        setError(e instanceof Error ? e.message : 'SignalR failed')
      })

    return () => {
      cancelled = true
      connection.stop()
    }
  }, [])

  return { data, connState, error }
}
