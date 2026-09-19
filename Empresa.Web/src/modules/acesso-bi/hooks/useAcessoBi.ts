import { useState, useCallback, useEffect, useRef } from 'react'
import { message } from 'antd'
import { acessoBiService } from '../services/acessoBiService'
import type { EmpresaBi, UsuarioBi } from '../types'

/**
 * Hook para gerenciamento do painel Acesso B.I. — listagem master-detail,
 * concessão total, vínculo por empresa e remoção.
 */
export function useAcessoBi() {
  const [usuarios, setUsuarios] = useState<UsuarioBi[]>([])
  const [loading, setLoading] = useState(false)
  const [search, setSearch] = useState('')
  const [expandedIdUsuario, setExpandedIdUsuario] = useState<number | null>(null)
  const [empresasUsuario, setEmpresasUsuario] = useState<EmpresaBi[]>([])
  const [loadingEmpresas, setLoadingEmpresas] = useState(false)
  const [saving, setSaving] = useState(false)
  const searchTimer = useRef<ReturnType<typeof setTimeout> | null>(null)
  const isMounted = useRef(true)

  const carregarUsuarios = useCallback(async (searchValue?: string) => {
    setLoading(true)
    try {
      const data = await acessoBiService.listarUsuarios(searchValue)
      if (isMounted.current) {
        setUsuarios(data || [])
      }
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ||
        (err as Error)?.message ||
        'Erro ao carregar usuários com acesso ao B.I.'
      if (isMounted.current) {
        message.error(msg)
      }
    } finally {
      if (isMounted.current) {
        setLoading(false)
      }
    }
  }, [])

  // Carregar ao montar
  useEffect(() => {
    isMounted.current = true
    carregarUsuarios()
    return () => {
      isMounted.current = false
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const onChangeSearch = useCallback((value: string) => {
    setSearch(value)
    if (searchTimer.current) clearTimeout(searchTimer.current)
    searchTimer.current = setTimeout(() => {
      carregarUsuarios(value)
    }, 300)
  }, [carregarUsuarios])

  const carregarEmpresas = useCallback(async (idUsuario: number) => {
    setLoadingEmpresas(true)
    try {
      const data = await acessoBiService.listarEmpresas(idUsuario)
      if (isMounted.current) {
        setEmpresasUsuario(data || [])
      }
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ||
        'Erro ao carregar empresas do usuário.'
      if (isMounted.current) {
        message.error(msg)
      }
    } finally {
      if (isMounted.current) {
        setLoadingEmpresas(false)
      }
    }
  }, [])

  const expandirUsuario = useCallback(async (idUsuario: number | null) => {
    if (idUsuario === null || idUsuario === expandedIdUsuario) {
      setExpandedIdUsuario(null)
      setEmpresasUsuario([])
      return
    }
    setExpandedIdUsuario(idUsuario)
    await carregarEmpresas(idUsuario)
  }, [carregarEmpresas, expandedIdUsuario])

  const concederAcessoTotal = useCallback(async (idUsuario: number): Promise<boolean> => {
    setSaving(true)
    try {
      await acessoBiService.concederAcessoTotal({ idUsuario })
      message.success('Acesso concedido com sucesso!')
      await carregarUsuarios(search)
      return true
    } catch (err: unknown) {
      const status = (err as { response?: { status?: number } })?.response?.status
      const msg =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message
      if (status === 409) {
        message.warning(msg || 'Usuário já possui acesso ao B.I.')
      } else {
        message.error(msg || 'Erro ao conceder acesso ao B.I.')
      }
      return false
    } finally {
      setSaving(false)
    }
  }, [carregarUsuarios, search])

  const vincularEmpresa = useCallback(async (idUsuario: number, codigoEmpresa: number): Promise<boolean> => {
    setSaving(true)
    try {
      await acessoBiService.vincularEmpresa(idUsuario, { codigoEmpresa })
      message.success('Empresa vinculada com sucesso!')
      await carregarEmpresas(idUsuario)
      await carregarUsuarios(search)
      return true
    } catch (err: unknown) {
      const status = (err as { response?: { status?: number } })?.response?.status
      const msg =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message
      if (status === 409) {
        message.warning(msg || 'Vínculo entre usuário e empresa já existe.')
      } else {
        message.error(msg || 'Erro ao vincular empresa.')
      }
      return false
    } finally {
      setSaving(false)
    }
  }, [carregarEmpresas, carregarUsuarios, search])

  const removerAcesso = useCallback(async (idUsuarioEmpresa: number) => {
    try {
      await acessoBiService.removerAcesso(idUsuarioEmpresa)
      message.success('Vínculo removido com sucesso!')
      if (expandedIdUsuario !== null) {
        await carregarEmpresas(expandedIdUsuario)
      }
      await carregarUsuarios(search)
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ||
        'Erro ao remover vínculo.'
      message.error(msg)
    }
  }, [carregarEmpresas, carregarUsuarios, expandedIdUsuario, search])

  return {
    usuarios,
    loading,
    search,
    expandedIdUsuario,
    empresasUsuario,
    loadingEmpresas,
    saving,
    carregarUsuarios,
    onChangeSearch,
    expandirUsuario,
    concederAcessoTotal,
    vincularEmpresa,
    removerAcesso,
  }
}