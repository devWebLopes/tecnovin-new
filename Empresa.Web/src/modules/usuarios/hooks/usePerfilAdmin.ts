import { useState, useCallback, useRef } from 'react'
import { message } from 'antd'
import { perfilService } from '../services/perfilService'
import type { Perfil, PaginaTree } from '../types'

/**
 * Hook para gerenciamento de perfis — listagem, CRUD e permissões.
 * Alinhado com a API REST real do backend.
 */
export function usePerfilAdmin() {
  const [perfis, setPerfis] = useState<Perfil[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [perfilSelecionado, setPerfilSelecionado] = useState<Perfil | null>(null)
  const [loadingSelecionado, setLoadingSelecionado] = useState(false)
  const [saving, setSaving] = useState(false)
  const [paginasPerfil, setPaginasPerfil] = useState<PaginaTree[]>([])
  const [loadingPaginas, setLoadingPaginas] = useState(false)
  const isMounted = useRef(true)

  const carregarPerfis = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await perfilService.listar()
      if (isMounted.current) {
        setPerfis(data || [])
      }
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
        || (err as Error)?.message
        || 'Erro ao carregar perfis.'
      if (isMounted.current) {
        setError(msg)
        message.error(msg)
      }
    } finally {
      if (isMounted.current) {
        setLoading(false)
      }
    }
  }, [])

  const selecionarPerfil = useCallback(async (id: number) => {
    setLoadingSelecionado(true)
    setError(null)
    try {
      const perfil = await perfilService.obterPorId(id)
      if (isMounted.current) {
        setPerfilSelecionado(perfil)
      }
      // Carregar também as páginas/permissões
      try {
        const paginas = await perfilService.obterPaginasDoPerfil(id)
        if (isMounted.current) {
          setPaginasPerfil(paginas)
        }
      } catch {
        if (isMounted.current) {
          setPaginasPerfil([])
        }
      }
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
        || 'Erro ao carregar detalhes do perfil.'
      if (isMounted.current) {
        setError(msg)
        message.error(msg)
      }
    } finally {
      if (isMounted.current) {
        setLoadingSelecionado(false)
      }
    }
  }, [])

  const limparSelecao = useCallback(() => {
    setPerfilSelecionado(null)
    setPaginasPerfil([])
  }, [])

  const criarNovo = useCallback(() => {
    setPerfilSelecionado({ idPerfil: 0, descricao: '' })
    setPaginasPerfil([])
  }, [])

  const salvar = useCallback(async (descricao: string, id?: number): Promise<boolean> => {
    setSaving(true)
    try {
      if (id && id > 0) {
        await perfilService.atualizar(id, descricao)
        message.success('Perfil atualizado com sucesso!')
        // Atualizar perfil selecionado
        if (isMounted.current) {
          setPerfilSelecionado((prev) => prev ? { ...prev, descricao } : null)
        }
      } else {
        const novo = await perfilService.criar(descricao)
        message.success('Perfil criado com sucesso!')
        if (isMounted.current) {
          setPerfilSelecionado(novo)
        }
      }
      await carregarPerfis()
      return true
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
        || 'Erro ao salvar perfil.'
      message.error(msg)
      return false
    } finally {
      if (isMounted.current) {
        setSaving(false)
      }
    }
  }, [carregarPerfis])

  const excluir = useCallback(async (id: number) => {
    try {
      await perfilService.excluir(id)
      message.success('Perfil excluído com sucesso!')
      await carregarPerfis()
      if (perfilSelecionado?.idPerfil === id) {
        setPerfilSelecionado(null)
        setPaginasPerfil([])
      }
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
        || 'Erro ao excluir perfil.'
      message.error(msg)
    }
  }, [carregarPerfis, perfilSelecionado])

  const salvarPermissoes = useCallback(async (vincularIds: number[], desvincularIds: number[]) => {
    if (!perfilSelecionado || perfilSelecionado.idPerfil <= 0) return
    setLoadingPaginas(true)
    try {
      await perfilService.salvarPermissoes(perfilSelecionado.idPerfil, {
        vincularIds,
        desvincularIds,
      })
      message.success('Permissões atualizadas com sucesso!')
      // Recarregar páginas do perfil
      const paginas = await perfilService.obterPaginasDoPerfil(perfilSelecionado.idPerfil)
      if (isMounted.current) {
        setPaginasPerfil(paginas)
      }
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
        || 'Erro ao salvar permissões.'
      message.error(msg)
    } finally {
      if (isMounted.current) {
        setLoadingPaginas(false)
      }
    }
  }, [perfilSelecionado])

  return {
    perfis,
    loading,
    error,
    perfilSelecionado,
    loadingSelecionado,
    saving,
    paginasPerfil,
    loadingPaginas,

    carregarPerfis,
    selecionarPerfil,
    limparSelecao,
    criarNovo,
    salvar,
    excluir,
    salvarPermissoes,
  }
}