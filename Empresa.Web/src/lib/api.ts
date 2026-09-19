import axios, { type InternalAxiosRequestConfig, type AxiosResponse } from 'axios'

/**
 * Instância Axios configurada para o backend GestaoNew.
 *
 * Responsabilidades:
 * - Injeta automaticamente o Bearer token em toda requisição
 * - Tenta refresh automático ao receber 401 (token expirado)
 * - Redireciona para /login em caso de falha no refresh
 */
const api = axios.create({
  baseURL: '/api/v1',
  timeout: 30_000,
  headers: {
    'Content-Type': 'application/json',
  },
})

// ─── Request Interceptor: injeta JWT + idPerfil ───────────────────────────────
api.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const raw = localStorage.getItem('auth-storage')
    if (raw) {
      try {
        const parsed = JSON.parse(raw)
        const token: string | undefined = parsed?.state?.token
        const perfilId: number | undefined = parsed?.state?.usuario?.perfilId

        if (token && config.headers) {
          config.headers['Authorization'] = `Bearer ${token}`
        }

        // Injeta idPerfil como query param em toda requisição (exigido pelo backend)
        if (perfilId != null) {
          config.params = { idPerfil: perfilId, ...config.params }
        }
      } catch {
        // dados corrompidos — ignora
      }
    }
    return config
  },
  (error) => Promise.reject(error),
)

// ─── Response Interceptor: refresh automático em 401 ─────────────────────────
let isRefreshing = false
let pendingRequests: Array<(token: string) => void> = []

api.interceptors.response.use(
  (response: AxiosResponse) => response,
  async (error) => {
    const originalRequest = error.config

    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        // Aguarda o refresh em andamento
        return new Promise((resolve) => {
          pendingRequests.push((token) => {
            originalRequest.headers['Authorization'] = `Bearer ${token}`
            resolve(api(originalRequest))
          })
        })
      }

      originalRequest._retry = true
      isRefreshing = true

      try {
        const raw = localStorage.getItem('auth-storage')
        const parsed = raw ? JSON.parse(raw) : null
        const refreshToken: string | undefined = parsed?.state?.refreshToken

        if (!refreshToken) throw new Error('Sem refresh token')

        const { data } = await axios.post('/api/v1/auth/refresh', { refreshToken })
        const newToken: string = data.token

        // Atualiza o store persisted no localStorage
        if (parsed?.state) {
          parsed.state.token = newToken
          parsed.state.refreshToken = data.refreshToken
          localStorage.setItem('auth-storage', JSON.stringify(parsed))
        }

        pendingRequests.forEach((cb) => cb(newToken))
        pendingRequests = []
        isRefreshing = false

        originalRequest.headers['Authorization'] = `Bearer ${newToken}`
        return api(originalRequest)
      } catch {
        isRefreshing = false
        pendingRequests = []
        localStorage.removeItem('auth-storage')
        window.location.href = '/login'
        return Promise.reject(error)
      }
    }

    return Promise.reject(error)
  },
)

export default api
