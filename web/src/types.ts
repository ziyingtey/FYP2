export interface BranchDashboard {
  branchId: number
  branchName: string
  maxCapacity: number
  occupancy: number
  occupancyPercent: number
  crowdLevel: string
  queueBookingBlocked: boolean
  bookingBlockReason: string | null
  services: ServiceQueueState[]
  counters: CounterState[]
  recentTickets: TicketSummary[]
}

export interface ServiceQueueState {
  serviceType: string
  displayName: string
  waiting: number
  servingTicketCode: string | null
  servingTicketId: number | null
  servingCounterId: number | null
  estimatedClearingMinutes: number
  estimatedAvgWaitMinutes: number
}

export interface CounterState {
  id: number
  label: string
  serviceType: string
  isOpen: boolean
  staffAvailable: boolean
  currentTicketCode: string | null
  currentTicketId: number | null
}

export interface TicketSummary {
  ticketCode: string
  serviceType: string
  status: string
  createdUtc: string
}

export const BRANCH_ID = 1
