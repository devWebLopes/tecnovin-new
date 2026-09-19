/** DTOs espelhando os contratos do backend Empresa.Api */

export interface LoginRequest {
  login: string
  senha: string
}

export interface LoginResponse {
  token: string
  refreshToken: string
  expiresAt: string
  usuario: UsuarioInfo
}

export interface UsuarioInfo {
  id: number
  nome: string
  login: string
  email: string
  perfilId: number
  perfilNome: string
  estabelecimentoId: number
}

export interface PaginaMenuItem {
  id: number
  nome: string
  url: string
  icone: string
  ordem: number
  paiId: number | null
  filhos: PaginaMenuItem[]
}

/** Nó da árvore do menu — espelha MenuItemResponse do backend (RF06.1 — corrige G-03) */
export interface MenuItem {
  idPagina: number
  chaveControle: string
  tituloMenu: string
  tituloAba: string
  url: string
  tooltip: string
  ordem: number
  filhos: MenuItem[]
}

/** Item do painel "Mais Acessados" — espelha MaisAcessadoItemResponse (RF02) */
export interface MaisAcessadoItem {
  idPagina: number
  chaveControle: string
  tituloMenu: string
  tituloAba: string
  url: string
  quantidade: number
}

/** Barra do usuário do menu — espelha MenuUsuarioResponse (RF05) */
export interface MenuUsuario {
  login: string
  nome: string
  exibeLinkBi: boolean
}

/** Shell completo do menu — espelha MenuResponse do GET /api/v1/menu */
export interface MenuResponse {
  menu: MenuItem[]
  maisAcessados: MaisAcessadoItem[]
  usuario: MenuUsuario
}

export interface ApiError {
  status: number
  message: string
  detail?: string
}

export interface PagedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
}

export interface AlterarSenhaRequest {
  senhaAtual: string
  novaSenha: string
  confirmarSenha: string
}
