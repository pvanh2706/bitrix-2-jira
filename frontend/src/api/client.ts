import axios from 'axios'

const client = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json',
    'X-Api-Key': import.meta.env.VITE_API_KEY ?? '',
  },
  timeout: 60_000,
})

client.interceptors.response.use(
  (res) => res,
  (error) => {
    const msg =
      error.response?.data?.message ||
      error.response?.statusText ||
      error.message ||
      'Unknown error'
    return Promise.reject(new Error(msg))
  },
)

export default client
