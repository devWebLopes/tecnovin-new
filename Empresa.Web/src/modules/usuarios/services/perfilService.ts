import api from '@/lib/api'
import type { Perfil, PaginaTree } from '../types'

export interface PerfilPaginasRequest {
  vincularIds: number[]
  desvincularIds: number[]
}

/**
 * Service de Perfis — encapsula chamadas à API REST.
 * Backend:
 *   GET    /api/v1/perfis               → listar todos (com search opcional)
 *   GET    /api/v1/perfis/lista-simples  → dropdown (apenas idPerfil + descricao)
 *   GET    /api/v1/perfis/{id}           → buscar por ID
 *   POST   /api/v1/perfis               → criar
 *   PUT    /api/v1/perfis/{id}           → atualizar
 *   DELETE /api/v1/perfis/{id}           → excluir
 *   GET    /api/v1/perfis/{id}/paginas   → árvore de páginas com permissões
 *   PUT    /api/v1/perfis/{id}/paginas   → salvar permissões
 */
export const perfilService = {
  async listar(search?: string): Promise<Perfil[]> {
    const params = search ? { search } : {}
    const { data } = await api.get<Perfil[]>('/perfis', { params })
    return data
  },

  async listarSimples(): Promise<Perfil[]> {
    const { data } = await api.get<Perfil[]>('/perfis/lista-simples')
    return data
  },

  async obterPorId(id: number): Promise<Perfil> {
    const { data } = await api.get<Perfil>(`/perfis/${id}`)
    return data
  },

  async criar(descricao: string): Promise<Perfil> {
    const { data } = await api.post<Perfil>('/perfis', { descricao })
    return data
  },

  async atualizar(id: number, descricao: string): Promise<void> {
    await api.put(`/perfis/${id}`, { descricao })
  },

  async excluir(id: number): Promise<void> {
    await api.delete(`/perfis/${id}`)
  },

  async obterPaginasDoPerfil(perfilId: number): Promise<PaginaTree[]> {
    const { data } = await api.get<PaginaTree[]>(`/perfis/${perfilId}/paginas`)
    return data
  },

  async salvarPermissoes(perfilId: number, request: PerfilPaginasRequest): Promise<void> {
    await api.put(`/perfis/${perfilId}/paginas`, request)
  },
}
