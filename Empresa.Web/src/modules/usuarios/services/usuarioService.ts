import api from '@/lib/api'
import type { Usuario, UsuarioRequest } from '../types'

export interface ListarUsuariosParams {
  search?: string
  ativo?: string
}

/**
 * Service de Usuários — encapsula chamadas à API REST.
 * Alinhado com os endpoints do backend: /api/v1/usuarios
 */
export const usuarioService = {
  /**
   * Listar usuários com filtro opcional (backend retorna array simples).
   */
  async listar(params: ListarUsuariosParams = {}): Promise<Usuario[]> {
    const { data } = await api.get<Usuario[]>('/usuarios', { params })
    return data
  },

  /**
   * Obter detalhes de um usuário por ID.
   */
  async obterPorId(id: number): Promise<Usuario> {
    const { data } = await api.get<Usuario>(`/usuarios/${id}`)
    return data
  },

  /**
   * Criar novo usuário.
   */
  async criar(usuario: UsuarioRequest): Promise<Usuario> {
    const { data } = await api.post<Usuario>('/usuarios', usuario)
    return data
  },

  /**
   * Atualizar usuário existente.
   */
  async atualizar(id: number, usuario: UsuarioRequest): Promise<void> {
    await api.put(`/usuarios/${id}`, usuario)
  },

  /**
   * Excluir (exclusão lógica) um usuário.
   */
  async excluir(id: number): Promise<void> {
    await api.delete(`/usuarios/${id}`)
  },

  /**
   * Obter estabelecimentos do usuário (árvore agrupada por empresa).
   */
  async obterEstabelecimentos(id: number): Promise<import('../types').EstabelecimentoGrupo[]> {
    const { data } = await api.get<import('../types').EstabelecimentoGrupo[]>(`/usuarios/${id}/estabelecimentos`)
    return data
  },

  /**
   * Sincronizar vínculos de estabelecimentos do usuário.
   */
  async sincronizarEstabelecimentos(
    id: number,
    request: import('../types').EstabelecimentoVinculoRequest
  ): Promise<void> {
    await api.put(`/usuarios/${id}/estabelecimentos`, request)
  },
}