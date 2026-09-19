import { useState, useCallback } from 'react'
import { message } from 'antd'
import type { AxiosRequestConfig } from 'axios'
import api from '@/lib/api'

interface UseApiState<T> {
  data: T | null
  loading: boolean
  error: string | null
}

/**
 * Hook genérico para consumo da API REST.
 *
 * Gerencia automaticamente: estado de loading, dados e erros.
 * Exibe notificações de erro via Ant Design message.
 *
 * @example
 * const { data, loading, execute } = useApi<Usuario[]>()
 * useEffect(() => { execute({ method: 'GET', url: '/usuarios' }) }, [])
 */
export function useApi<T = unknown>() {
  const [state, setState] = useState<UseApiState<T>>({
    data: null,
    loading: false,
    error: null,
  })

  const execute = useCallback(
    async (config: AxiosRequestConfig, silent = false): Promise<T | null> => {
      setState((s) => ({ ...s, loading: true, error: null }))
      try {
        const response = await api.request<T>(config)
        setState({ data: response.data, loading: false, error: null })
        return response.data
      } catch (err: unknown) {
        const msg =
          (err as { response?: { data?: { message?: string } } })?.response?.data?.message ||
          'Erro ao processar a requisição.'
        setState((s) => ({ ...s, loading: false, error: msg }))
        if (!silent) message.error(msg)
        return null
      }
    },
    [],
  )

  const reset = useCallback(() => {
    setState({ data: null, loading: false, error: null })
  }, [])

  return { ...state, execute, reset }
}
