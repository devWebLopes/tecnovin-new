import api from '@/lib/api'
import type {
  ConcederAcessoTotalRequest,
  EmpresaBi,
  EmpresaDisponivel,
  UsuarioBi,
  VincularEmpresaRequest,
} from '../types'

/**
 * Service de Acesso B.I. — encapsula chamadas à API REST.
 * Alinhado com os endpoints do backend: /api/v1/acesso-bi
 */
export const acessoBiService = {
  /**
   * RF02 — Lista usuários com acesso ao B.I. (grid mestre).
   */
  async listarUsuarios(search?: string): Promise<UsuarioBi[]> {
    const { data } = await api.get<UsuarioBi[]>('/acesso-bi/usuarios', {
      params: search ? { search } : {},
    })
    return data
  },

  /**
   * RF03 — Lista empresas vinculadas ao usuário (grid detalhe).
   */
  async listarEmpresas(idUsuario: number): Promise<EmpresaBi[]> {
    const { data } = await api.get<EmpresaBi[]>(`/acesso-bi/usuarios/${idUsuario}/empresas`)
    return data
  },

  /**
   * RF05 — Lista empresas ainda NÃO vinculadas ao usuário (dropdown).
   */
  async listarEmpresasDisponiveis(idUsuario: number): Promise<EmpresaDisponivel[]> {
    const { data } = await api.get<EmpresaDisponivel[]>(
      `/acesso-bi/usuarios/${idUsuario}/empresas/disponiveis`,
    )
    return data
  },

  /**
   * RF04 — Concede acesso total ao B.I. (usuário → todas as empresas).
   */
  async concederAcessoTotal(request: ConcederAcessoTotalRequest): Promise<UsuarioBi[]> {
    const { data } = await api.post<UsuarioBi[]>('/acesso-bi/usuarios', request)
    return data
  },

  /**
   * RF05 — Vincula uma empresa específica ao usuário.
   */
  async vincularEmpresa(idUsuario: number, request: VincularEmpresaRequest): Promise<EmpresaBi> {
    const { data } = await api.post<EmpresaBi>(
      `/acesso-bi/usuarios/${idUsuario}/empresas`,
      request,
    )
    return data
  },

  /**
   * RF06 — Remove um vínculo específico.
   */
  async removerAcesso(idUsuarioEmpresa: number): Promise<void> {
    await api.delete(`/acesso-bi/empresas/${idUsuarioEmpresa}`)
  },
}