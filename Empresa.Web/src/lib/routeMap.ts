/**
 * RF06.3 — Mapeia CHAVE_CONTROLE → rota SPA.
 *
 * As URLs do banco são `.aspx` legadas (D-10); o mapa traduz a chave de controle
 * (a "moeda" de autorização RN-12) para a rota do React Router.
 *
 * Regra: todo novo item de menu DEVE ser registrado aqui (obrigatório no code review).
 */
export const ROUTE_MAP: Record<string, string> = {
  dashboard: '/',
  cadastroUsuario: '/usuarios',
  cadastroPerfil: '/perfis',
  cadastroUsuarioBi: '/configuracoes/acesso-bi',
  alterarSenha: '/alterar-senha',

  // Placeholders para módulos futuros
  compras: '/compras',
  comprasResumoAnual: '/compras/resumo-anual',
  comprasComite: '/compras/comite',
  comprasCfop: '/compras/cfop',
  comprasCentroCusto: '/compras/centro-custo',
  financeiro: '/financeiro',
  financeiroPosicao: '/financeiro/posicao',
  financeiroFluxoCaixa: '/financeiro/fluxo-caixa',
  financeiroDre: '/financeiro/dre',
  prazoMedio: '/prazo-medio',
  prazoMedioRecebimento: '/prazo-medio/recebimento',
  prazoMedioPagamento: '/prazo-medio/pagamento',
  vendas: '/vendas',
  vendasAnalise: '/vendas/analise',
  vendasRanking: '/vendas/ranking',
  agricola: '/agricola',
  cadastroSafraMeta: '/agricola/safra-meta',
  comprasFrutasPorEmpresas: '/agricola/compras-frutas',
  calendario: '/calendario',
  agrupamentoDre: '/configuracoes/agrupamento-dre',
  dba: '/admin/dba',
  estabelecimentos: '/estabelecimentos',
}

/**
 * Resolve a rota SPA de um item de menu.
 * 1. Usa o ROUTE_MAP (chave de controle);
 * 2. Fallback por convenção: converte a URL legada (ex.: "interna/CadastroPerfil.aspx")
 *    em rota kebab-case (ex.: "/cadastro-perfil");
 * 3. Último recurso: "/" (dashboard).
 */
export function resolverRota(chaveControle: string, url: string): string {
  if (chaveControle && ROUTE_MAP[chaveControle]) return ROUTE_MAP[chaveControle]

  const nome = url?.split('/').pop()?.replace(/\.aspx$/i, '') ?? ''
  if (!nome) return '/'

  const kebab = nome
    .replace(/([a-z0-9])([A-Z])/g, '$1-$2')
    .replace(/[^a-zA-Z0-9]+/g, '-')
    .toLowerCase()
    .replace(/^-+|-+$/g, '')

  return `/${kebab}`
}
