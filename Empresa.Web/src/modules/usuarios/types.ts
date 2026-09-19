// ─── Tipos para o Módulo de Usuários e Perfis ────────────────────────────
// Alinhados com os DTOs do backend (camelCase via System.Text.Json)

export interface Usuario {
  id: number
  nome: string
  login: string
  idPerfil: number
  descricaoPerfil: string | null
  quantidadeAcesso: number
  atualizaSenha: boolean
  ativo: boolean
  dataHoraUltimoAcesso: string | null
}

export interface UsuarioRequest {
  nome: string
  login: string
  senha?: string
  idPerfil: number
  ativo: boolean
  atualizaSenha: boolean
}

export interface Perfil {
  idPerfil: number
  descricao: string
}

export interface PaginaTree {
  idPagina: number
  url: string
  tituloAba: string
  chaveControle: string
  tituloMenu: string
  idPaginaPai: number | null
  ordem: number
  toolTip: string
  ativo: boolean
  vinculado: boolean
  filhos: PaginaTree[]
}

export interface EstabelecimentoFilho {
  cdEstabelecimento: number
  dsEstabelecimento: string
  vinculado: boolean
}

export interface EstabelecimentoGrupo {
  cdEmpresa: number
  dsEmpresa: string
  estabelecimentos: EstabelecimentoFilho[]
}

export interface VinculoEstabelecimento {
  cdEmpresa: number
  cdEstabelecimento: number
}

export interface EstabelecimentoVinculoRequest {
  vincularEstabelecimentos: VinculoEstabelecimento[]
  desvincularEstabelecimentos: VinculoEstabelecimento[]
}

export interface UsuarioFiltros {
  search: string
  situacao: 'todos' | 'ativos' | 'inativos'
  page: number
  pageSize: number
}