import api from '@/lib/api'
import type { EstabelecimentoGrupo } from '../types'

/**
 * Service de Estabelecimentos — encapsula chamadas à API REST.
 * Backend: GET /api/v1/estabelecimentos/tree
 */
export const estabelecimentoService = {
  async obterTree(): Promise<EstabelecimentoGrupo[]> {
    const { data } = await api.get<EstabelecimentoGrupo[]>('/estabelecimentos/tree')
    return data
  },
}