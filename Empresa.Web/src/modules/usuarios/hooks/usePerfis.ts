import { useState, useEffect, useCallback } from 'react'
import { perfilService } from '../services/perfilService'
import type { Perfil } from '../types'

/**
 * Hook para carregar lista de perfis para dropdowns.
 * Faz chamada para: GET /api/v1/perfis (ou /api/v1/perfis/lista-simples)
 */
export function usePerfis() {
  const [perfis, setPerfis] = useState<Perfil[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const carregar = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      console.log('[usePerfis] Carregando perfis...')
      const data = await perfilService.listarSimples()
      console.log('[usePerfis] Perfis recebidos:', data?.length || 0, data)
      setPerfis(data || [])
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
        || (err as Error)?.message
        || 'Erro ao carregar perfis.'
      console.error('[usePerfis] Erro:', err)
      setError(msg)
      setPerfis([])
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    carregar()
  }, [carregar])

  return { perfis, loading, error, recarregar: carregar }
}