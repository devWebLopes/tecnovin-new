import api from '@/lib/api'
import type { MenuResponse } from '@/types/api'

/**
 * Service do menu — encapsula chamadas ao módulo Menu (GET /api/v1/menu).
 * Alinhado com os endpoints do backend: /api/v1/menu.
 */
export const menuService = {
  /**
   * RF01 — Retorna o shell do menu: árvore hierárquica + mais acessados + usuário/B.I.
   */
  async getMenu(): Promise<MenuResponse> {
    const { data } = await api.get<MenuResponse>('/menu')
    return data
  },

  /**
   * RF03 — Registra a telemetria de acesso a uma página (fire-and-forget).
   * O chamador deve tratar a falha de forma silenciosa (RF03.5/D-03).
   */
  async registrarAcesso(chaveControle: string): Promise<void> {
    await api.post('/menu/acessos', { chaveControle })
  },
}
