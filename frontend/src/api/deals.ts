import client from './client'
import type { ApiResponse, DealSummaryDto, DealSearchParams, ProcessDealResult } from './types'

export const dealsApi = {
  search(params: DealSearchParams) {
    return client.get<ApiResponse<DealSummaryDto[]>>('/deals', { params })
  },

  getById(dealId: number) {
    return client.get<ApiResponse<DealSummaryDto>>(`/deals/${dealId}`)
  },

  isProcessing(dealId: number) {
    return client.get<ApiResponse<boolean>>(`/deals/${dealId}/is-processing`)
  },

  process(dealId: number, signal?: AbortSignal) {
    return client.post<ApiResponse<ProcessDealResult>>(
      `/deals/${dealId}/process`,
      null,
      { signal },
    )
  },
}
