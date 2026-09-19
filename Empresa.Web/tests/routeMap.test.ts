import { describe, expect, it } from 'vitest'
import { ROUTE_MAP, resolverRota } from '@/lib/routeMap'

describe('ROUTE_MAP', () => {
  it('mapeia chaves de controle conhecidas para rotas SPA', () => {
    expect(ROUTE_MAP.dashboard).toBe('/')
    expect(ROUTE_MAP.cadastroUsuario).toBe('/usuarios')
    expect(ROUTE_MAP.cadastroPerfil).toBe('/perfis')
    expect(ROUTE_MAP.cadastroUsuarioBi).toBe('/configuracoes/acesso-bi')
    expect(ROUTE_MAP.alterarSenha).toBe('/alterar-senha')
  })
})

describe('resolverRota', () => {
  it('prioriza a chave de controle do ROUTE_MAP sobre a URL', () => {
    expect(resolverRota('cadastroUsuario', 'interna/Usuarios.aspx')).toBe('/usuarios')
    expect(resolverRota('cadastroPerfil', 'interna/CadastroPerfil.aspx')).toBe('/perfis')
  })

  it('usa a URL legada como fallback em kebab-case', () => {
    expect(resolverRota('', 'interna/CadastroPerfil.aspx')).toBe('/cadastro-perfil')
    expect(resolverRota('chave-desconhecida', 'interna/FinanceiroDre.aspx')).toBe('/financeiro-dre')
  })

  it('remove extensão e normaliza separadores', () => {
    expect(resolverRota('', 'relatorios/Relatorio_Vendas.aspx')).toBe('/relatorio-vendas')
    expect(resolverRota('', 'interna/CadastroBi.aspx')).toBe('/cadastro-bi')
  })

  it('retorna / quando não há chave nem URL', () => {
    expect(resolverRota('', '')).toBe('/')
    expect(resolverRota('', '   ')).toBe('/')
  })
})
