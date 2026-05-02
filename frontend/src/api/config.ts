import client from './client'
import type { ApiResponse, ConfigData, SystemConfig, SaveConfigRequest } from './types'

export const configApi = {
  get() {
    return client.get<ApiResponse<ConfigData[]>>('/config')
  },

  save(payload: SaveConfigRequest) {
    return client.put<ApiResponse<null>>('/config', payload)
  },

  getSystemConfigs() {
    return client.get<ApiResponse<SystemConfig[]>>('/config/system')
  },

  updateSystemConfig(key: string, value: string) {
    return client.put<ApiResponse<null>>(`/config/system/${encodeURIComponent(key)}`, { value })
  },
}
