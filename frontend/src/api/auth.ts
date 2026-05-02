import axios from 'axios'
import client from './client'
import type { AdminUser } from './types'

// Auth calls bypass the ApiKey header — use a plain axios instance
const authClient = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
  timeout: 10_000,
})

export const authApi = {
  login(username: string, password: string) {
    return authClient.post<{ success: boolean; role: string; username: string }>('/auth/login', {
      username,
      password,
    })
  },

  // Admin user management — all require X-Api-Key (sent by `client`)
  listUsers() {
    return client.get<AdminUser[]>('/auth/users')
  },
  createUser(username: string, password: string) {
    return client.post<AdminUser>('/auth/users', { username, password })
  },
  updateUser(id: number, payload: { newUsername?: string; newPassword?: string }) {
    return client.put<AdminUser>(`/auth/users/${id}`, payload)
  },
  deleteUser(id: number) {
    return client.delete(`/auth/users/${id}`)
  },
}
