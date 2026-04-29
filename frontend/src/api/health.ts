import client from './client'
import type { ApiResponse, HealthStatus } from './types'

export const healthApi = {
  get() {
    return client.get<ApiResponse<HealthStatus>>('/health')
  },
}
