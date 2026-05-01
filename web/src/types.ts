export interface BranchDashboard {
  branchId: number
  branchName: string
  location: string
  maxCapacity: number
  occupancy: number
  occupancyPercent: number
  crowdLevel: string
  queueBookingBlocked: boolean
  bookingBlockReason: string | null
  crowdMediumStartsAtPercent: number
  crowdHighStartsAtPercent: number
  overcrowdStartsAtPercent: number
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

export interface BranchDetail {
  branchId: number
  name: string
  location: string
  maxCapacity: number
  crowdMediumStartsAtPercent: number
  crowdHighStartsAtPercent: number
  overcrowdStartsAtPercent: number
}

export interface ServiceCatalogItem {
  id: number
  code: string
  displayName: string
  avgServiceTimeMinutes: number
}

export interface StaffListItem {
  id: number
  name: string
  role: string
  email: string | null
  assignedCounterLabel: string | null
}

export interface CrowdLogRow {
  id: number
  totalCustomers: number
  crowdLevel: string
  timestampUtc: string
}

export interface PredictionLogRow {
  id: number
  serviceCode: string
  queueLength: number
  activeCounters: number
  estimatedAvgWaitMinutes: number
  estimatedClearingMinutes: number
  source: string
  timestampUtc: string
}

export interface SimulationRunRow {
  id: number
  requestedCount: number
  generatedCount: number
  mode: string
  scenario: string | null
  fixedServiceCode: string | null
  createdUtc: string
}

export interface BranchSummary {
  branchId: number
  name: string
  location: string
  maxCapacity: number
  occupancy: number
  occupancyPercent: number
  crowdLevel: string
  queueBookingBlocked: boolean
}

export interface TicketHistoryRow {
  id: number
  ticketCode: string
  serviceCode: string
  status: string
  createdUtc: string
  calledUtc: string | null
  completedUtc: string | null
  counterId: number | null
  isSimulated: boolean
}

export interface DailyReport {
  branchId: number
  reportDate: string
  totalCustomersServed: number
  avgWaitMinutes: number
  peakHour: string | null
  busiestServiceCode: string | null
  generatedUtc: string
}
