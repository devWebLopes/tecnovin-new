import api from '@/lib/api'
import type { PaginaTree } from '../types'

/**
 * Service de Páginas — encapsula chamadas à API REST.
 * Backend: GET /api/v1/perfis/{perfilId}/paginas (árvore com vínculos)
 */
export const paginaService = {
  async obterTree(): Promise<PaginaTree[]> {
    await api.get<PaginaTree[]>('/perfis/lista-simples')
    return []
  },

  async obterPaginasPorPerfil(perfilId: number): Promise<PaginaTree[]> {
    const { data } = await api.get<PaginaTree[]>(`/perfis/${perfilId}/paginas`)
    return data
  },
}