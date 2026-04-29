import client from './client'
import type { ApiResponse, ConfigData, SaveConfigRequest } from './types'

export const configApi = {
  get() {
    return client.get<ApiResponse<ConfigData[]>>('/config')
  },

  save(payload: SaveConfigRequest) {
    return client.put<ApiResponse<null>>('/config', payload)
  },
}
