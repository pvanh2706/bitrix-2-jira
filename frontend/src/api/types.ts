// Generic API response wrapper matching backend ApiResponse<T>
export interface ApiResponse<T> {
  success: boolean
  data: T | null
  message: string
}

// Admin user (from /api/auth/users)
export interface AdminUser {
  id: number
  username: string
}

// GET /api/deals | GET /api/deals/{id}
export interface DealSummaryDto {
  bitrix_DealID: number
  bitrix_DealLink: string
  bitrix_DateSearch: string
  isSendDataToJira: number
  jira_Link: string
  haveError: number
  errorInfo: string
  dateTimeCreated: string
  lastChangeData: string
}

// POST /api/deals/{id}/process
export interface ProcessDealResult {
  success: boolean
  isWaitingForFiles: boolean
  retryAfterSeconds: number
  jiraKey: string | null
  jiraUrl: string | null
  message: string
}

// GET /api/config  (returns list of ConfigData rows)
export interface ConfigData {
  id: number
  keyConfig: string
  valueConfig: string
  description: string
}

// GET /api/config/system
export interface SystemConfig {
  id: number
  configKey: string
  configValue: string
  description: string
}

// PUT /api/config
export interface SaveConfigRequest {
  quetLaiSau?: number | null
  guiLaiEmailSau?: number | null
  soNgayQuet?: number | null
}

// GET /api/health
export interface HealthStatus {
  status: string
  startedAt: string
  applicationStopping: boolean
}

// Query params for GET /api/deals
export interface DealSearchParams {
  dealId?: number | null
  fromDate?: string | null
  toDate?: string | null
}
