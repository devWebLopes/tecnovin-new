import { create } from 'zustand'
import { menuService } from '@/services/menuService'
import type { MaisAcessadoItem, MenuItem, MenuUsuario } from '@/types/api'

/** TTL do cache do menu — espelha o TTL de 10 min do cache backend (RF07.2/D-08) */
const CACHE_TTL_MS = 10 * 60 * 1000

/**
 * RF03.6/D-11 — Deduplicação da telemetria por CHAVE_CONTROLE: registra na primeira
 * abertura da rota na sessão, não a cada re-navegação (semântica "aba nova" do legado RN-07).
 */
const chavesRegistradas = new Set<string>()

interface MenuState {
  menu: MenuItem[]
  maisAcessados: MaisAcessadoItem[]
  usuario: MenuUsuario | null
  carregando: boolean
  erro: string | null
  ultimaCarga: number | null

  /** RF01 — Carrega o shell do menu, respeitando o TTL do cache (force = ignora TTL) */
  carregarMenu: (force?: boolean) => Promise<void>
  /** RF03 — Registra telemetria de acesso (fire-and-forget, deduplicada por chave) */
  registrarAcesso: (chaveControle: string) => void
  invalidar: () => void
}

export const useMenuStore = create<MenuState>()((set, get) => ({
  menu: [],
  maisAcessados: [],
  usuario: null,
  carregando: false,
  erro: null,
  ultimaCarga: null,

  carregarMenu: async (force = false) => {
    const { ultimaCarga, carregando } = get()
    if (carregando) return
    if (!force && ultimaCarga && Date.now() - ultimaCarga < CACHE_TTL_MS) return

    set({ carregando: true, erro: null })
    try {
      const data = await menuService.getMenu()
      set({
        menu: data.menu ?? [],
        maisAcessados: data.maisAcessados ?? [],
        usuario: data.usuario ?? null,
        ultimaCarga: Date.now(),
      })
    } catch (err: unknown) {
      // RF08.3/D-09 — erro de menu não quebra a navegação (fallback estático no SideMenu)
      const msg = (err as Error)?.message || 'Erro ao carregar o menu.'
      set({ erro: msg })
      console.warn('[menuStore] Falha ao carregar menu:', err)
    } finally {
      set({ carregando: false })
    }
  },

  registrarAcesso: (chaveControle: string) => {
    if (!chaveControle || chavesRegistradas.has(chaveControle)) return
    chavesRegistradas.add(chaveControle)

    // RF03.5 — fire-and-forget: falha de telemetria não interrompe a navegação (D-03)
    menuService.registrarAcesso(chaveControle).catch((err: unknown) => {
      console.warn('[menuStore] Falha ao registrar acesso:', err)
    })
  },

  invalidar: () => {
    chavesRegistradas.clear()
    set({ menu: [], maisAcessados: [], usuario: null, ultimaCarga: null })
  },
}))
