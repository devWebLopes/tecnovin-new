import { useState, useCallback, useEffect, useRef } from 'react'
import { message } from 'antd'
import { usuarioService } from '../services/usuarioService'
import type { Usuario, UsuarioFiltros, UsuarioRequest, VinculoEstabelecimento } from '../types'

const FILTROS_PADRAO: UsuarioFiltros = {
  search: '',
  situacao: 'todos',
  page: 1,
  pageSize: 20,
}

/**
 * Hook para gerenciamento de usuários — listagem, CRUD e filtros.
 * Alinhado com a API REST real do backend.
 */
export function useUsuarios() {
  const [filtros, setFiltros] = useState<UsuarioFiltros>(FILTROS_PADRAO)
  const [usuarios, setUsuarios] = useState<Usuario[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [usuarioSelecionado, setUsuarioSelecionado] = useState<Usuario | null>(null)
  const [loadingDetalhe, setLoadingDetalhe] = useState(false)
  const [saving, setSaving] = useState(false)
  const [estabelecimentosUsuario, setEstabelecimentosUsuario] = useState<VinculoEstabelecimento[]>([])
  const searchTimer = useRef<ReturnType<typeof setTimeout> | null>(null)
  const isMounted = useRef(true)

  const carregarUsuarios = useCallback(async (filtrosOverride?: Partial<UsuarioFiltros>) => {
    setLoading(true)
    setError(null)
    try {
      // Determinar filtros a usar: prioriza override, senão usa state atual
      const search = filtrosOverride?.search !== undefined ? filtrosOverride.search : filtros.search
      const situacao = filtrosOverride?.situacao !== undefined ? filtrosOverride.situacao : filtros.situacao

      const params: { search?: string; ativo?: string } = {}
      if (search) params.search = search
      if (situacao === 'ativos') params.ativo = 'S'
      else if (situacao === 'inativos') params.ativo = 'N'

      console.log('[useUsuarios] Carregando usuários com params:', params)
      const data = await usuarioService.listar(params)
      console.log('[useUsuarios] Usuários recebidos:', data?.length || 0, 'registros')

      if (isMounted.current) {
        setUsuarios(data || [])
      }
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
        || (err as Error)?.message
        || 'Erro ao carregar usuários.'
      console.error('[useUsuarios] Erro:', err)
      if (isMounted.current) {
        setError(msg)
        message.error(msg)
      }
    } finally {
      if (isMounted.current) {
        setLoading(false)
      }
    }
  }, [filtros])

  // Carregar ao montar
  useEffect(() => {
    isMounted.current = true
    carregarUsuarios()
    return () => { isMounted.current = false }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const onChangeSearch = useCallback((value: string) => {
    if (searchTimer.current) clearTimeout(searchTimer.current)
    searchTimer.current = setTimeout(() => {
      const novosFiltros = { ...FILTROS_PADRAO, search: value, page: 1, situacao: filtros.situacao }
      setFiltros(novosFiltros)
      carregarUsuarios({ search: value })
    }, 300)
  }, [filtros, carregarUsuarios])

  const onChangeFiltro = useCallback((partial: Partial<UsuarioFiltros>) => {
    const novosFiltros = { ...filtros, ...partial, page: 1 }
    setFiltros(novosFiltros)
    carregarUsuarios(novosFiltros)
  }, [filtros, carregarUsuarios])

  const onChangePage = useCallback((page: number, pageSize: number) => {
    setFiltros((prev) => ({ ...prev, page, pageSize }))
  }, [])

  // Filtragem local
  const filteredUsuarios = filtros.search
    ? usuarios.filter((u) => {
        const q = filtros.search.toLowerCase()
        return u.nome.toLowerCase().includes(q) || u.login.toLowerCase().includes(q)
      })
    : usuarios

  // Paginação local
  const total = filteredUsuarios.length
  const paginatedUsuarios = filteredUsuarios.slice(
    (filtros.page - 1) * filtros.pageSize,
    filtros.page * filtros.pageSize,
  )

  const selecionarUsuario = useCallback(async (id: number) => {
    setLoadingDetalhe(true)
    setError(null)
    try {
      const data = await usuarioService.obterPorId(id)
      if (isMounted.current) {
        setUsuarioSelecionado(data)
        // Carregar estabelecimentos vinculados
        try {
          const estabs = await usuarioService.obterEstabelecimentos(id)
          const vinculos: VinculoEstabelecimento[] = []
          for (const grupo of estabs) {
            for (const filho of grupo.estabelecimentos) {
              if (filho.vinculado) {
                vinculos.push({
                  cdEmpresa: grupo.cdEmpresa,
                  cdEstabelecimento: filho.cdEstabelecimento,
                })
              }
            }
          }
          setEstabelecimentosUsuario(vinculos)
        } catch (err) {
          console.warn('[useUsuarios] Erro ao carregar estabelecimentos:', err)
          setEstabelecimentosUsuario([])
        }
      }
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message || 'Erro ao carregar detalhes do usuário.'
      if (isMounted.current) {
        setError(msg)
        message.error(msg)
      }
    } finally {
      if (isMounted.current) {
        setLoadingDetalhe(false)
      }
    }
  }, [])

  const limparSelecao = useCallback(() => {
    setUsuarioSelecionado(null)
    setEstabelecimentosUsuario([])
  }, [])

  const salvar = useCallback(async (data: UsuarioRequest, id?: number): Promise<boolean> => {
    setSaving(true)
    try {
      if (id) {
        await usuarioService.atualizar(id, data)
        message.success('Usuário atualizado com sucesso!')
      } else {
        await usuarioService.criar(data)
        message.success('Usuário criado com sucesso!')
      }
      await carregarUsuarios()
      setUsuarioSelecionado(null)
      setEstabelecimentosUsuario([])
      return true
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message || 'Erro ao salvar usuário.'
      message.error(msg)
      return false
    } finally {
      setSaving(false)
    }
  }, [carregarUsuarios])

  const excluir = useCallback(async (id: number) => {
    try {
      await usuarioService.excluir(id)
      message.success('Usuário excluído com sucesso!')
      await carregarUsuarios()
      if (usuarioSelecionado?.id === id) {
        setUsuarioSelecionado(null)
        setEstabelecimentosUsuario([])
      }
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message || 'Erro ao excluir usuário.'
      message.error(msg)
    }
  }, [carregarUsuarios, usuarioSelecionado])

  const criarNovo = useCallback(() => {
    setUsuarioSelecionado({
      id: 0,
      nome: '',
      login: '',
      idPerfil: 0,
      descricaoPerfil: null,
      quantidadeAcesso: 0,
      atualizaSenha: false,
      ativo: true,
      dataHoraUltimoAcesso: null,
    })
    setEstabelecimentosUsuario([])
  }, [])

  const sincronizarEstabelecimentos = useCallback(async (id: number, vinculos: VinculoEstabelecimento[]) => {
    try {
      const atuais = estabelecimentosUsuario
      const atuaisMap = new Set(atuais.map((v) => `${v.cdEmpresa}-${v.cdEstabelecimento}`))
      const novosMap = new Set(vinculos.map((v) => `${v.cdEmpresa}-${v.cdEstabelecimento}`))

      const vincular = vinculos.filter((v) => !atuaisMap.has(`${v.cdEmpresa}-${v.cdEstabelecimento}`))
      const desvincular = atuais.filter((v) => !novosMap.has(`${v.cdEmpresa}-${v.cdEstabelecimento}`))

      if (vincular.length === 0 && desvincular.length === 0) return

      await usuarioService.sincronizarEstabelecimentos(id, {
        vincularEstabelecimentos: vincular,
        desvincularEstabelecimentos: desvincular,
      })
      setEstabelecimentosUsuario(vinculos)
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message || 'Erro ao sincronizar estabelecimentos.'
      message.error(msg)
    }
  }, [estabelecimentosUsuario])

  return {
    usuarios: paginatedUsuarios,
    total,
    loading,
    loadingDetalhe,
    saving,
    error,
    filtros,
    usuarioSelecionado,
    estabelecimentosUsuario,

    carregarUsuarios,
    onChangeSearch,
    onChangeFiltro,
    onChangePage,
    selecionarUsuario,
    limparSelecao,
    salvar,
    excluir,
    criarNovo,
    sincronizarEstabelecimentos,
  }
}