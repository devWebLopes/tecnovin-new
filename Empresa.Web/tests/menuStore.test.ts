import { beforeEach, describe, expect, it, vi } from 'vitest'
import { useMenuStore } from '@/store/menuStore'

const getMenuMock = vi.fn()
const registrarAcessoMock = vi.fn()

vi.mock('@/services/menuService', () => ({
  menuService: {
    getMenu: (...args: unknown[]) => getMenuMock(...args),
    registrarAcesso: (...args: unknown[]) => registrarAcessoMock(...args),
  },
}))

describe('menuStore', () => {
  beforeEach(() => {
    getMenuMock.mockReset()
    registrarAcessoMock.mockReset()
    useMenuStore.getState().invalidar()
    useMenuStore.setState({
      menu: [],
      maisAcessados: [],
      usuario: null,
      carregando: false,
      erro: null,
      ultimaCarga: null,
    })
  })

  it('carrega o menu com sucesso e grava ultimaCarga', async () => {
    const payload = {
      menu: [{ idPagina: 1, tituloMenu: 'Início', filhos: [] }],
      maisAcessados: [{ chaveControle: 'usuarios', tituloMenu: 'Usuários', quantidade: 10 }],
      usuario: { nome: 'João', login: 'joao', exibeLinkBi: true },
    }
    getMenuMock.mockResolvedValue(payload)

    await useMenuStore.getState().carregarMenu()

    const state = useMenuStore.getState()
    expect(state.menu).toHaveLength(1)
    expect(state.maisAcessados).toHaveLength(1)
    expect(state.usuario?.exibeLinkBi).toBe(true)
    expect(state.erro).toBeNull()
    expect(state.ultimaCarga).not.toBeNull()
    expect(getMenuMock).toHaveBeenCalledTimes(1)
  })

  it('respeita o TTL de cache (segunda chamada não refaz a requisição)', async () => {
    getMenuMock.mockResolvedValue({ menu: [], maisAcessados: [], usuario: null })

    await useMenuStore.getState().carregarMenu()
    await useMenuStore.getState().carregarMenu()

    expect(getMenuMock).toHaveBeenCalledTimes(1)
  })

  it('force=true ignora o TTL', async () => {
    getMenuMock.mockResolvedValue({ menu: [], maisAcessados: [], usuario: null })

    await useMenuStore.getState().carregarMenu()
    await useMenuStore.getState().carregarMenu(true)

    expect(getMenuMock).toHaveBeenCalledTimes(2)
  })

  it('falha de carregamento define erro e mantém navegação (fallback)', async () => {
    getMenuMock.mockRejectedValue(new Error('API indisponível'))

    await useMenuStore.getState().carregarMenu()

    const state = useMenuStore.getState()
    expect(state.erro).toBe('API indisponível')
    expect(state.menu).toEqual([])
    expect(state.carregando).toBe(false)
  })

  it('registrarAcesso dispara telemetria fire-and-forget', async () => {
    registrarAcessoMock.mockResolvedValue(undefined)

    useMenuStore.getState().registrarAcesso('usuarios')

    expect(registrarAcessoMock).toHaveBeenCalledWith('usuarios')
  })

  it('registrarAcesso deduplica por chave na sessão', async () => {
    registrarAcessoMock.mockResolvedValue(undefined)

    useMenuStore.getState().registrarAcesso('usuarios')
    useMenuStore.getState().registrarAcesso('usuarios')

    expect(registrarAcessoMock).toHaveBeenCalledTimes(1)
  })

  it('registrarAcesso ignora chave vazia', () => {
    useMenuStore.getState().registrarAcesso('')

    expect(registrarAcessoMock).not.toHaveBeenCalled()
  })

  it('invalidar limpa o estado e o cache de deduplicação', async () => {
    getMenuMock.mockResolvedValue({ menu: [], maisAcessados: [], usuario: null })
    registrarAcessoMock.mockResolvedValue(undefined)

    await useMenuStore.getState().carregarMenu()
    useMenuStore.getState().registrarAcesso('usuarios')
    useMenuStore.getState().invalidar()

    expect(useMenuStore.getState().menu).toEqual([])
    expect(useMenuStore.getState().ultimaCarga).toBeNull()

    useMenuStore.getState().registrarAcesso('usuarios')
    expect(registrarAcessoMock).toHaveBeenCalledTimes(2)
  })
})
