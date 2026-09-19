import { useEffect, useMemo, useState } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { Menu, Skeleton, Typography } from 'antd'
import type { MenuProps } from 'antd'
import {
  DashboardOutlined,
  ShoppingCartOutlined,
  DollarOutlined,
  ClockCircleOutlined,
  LineChartOutlined,
  AppleOutlined,
  CalendarOutlined,
  SettingOutlined,
  TeamOutlined,
  DatabaseOutlined,
  KeyOutlined,
  FileTextOutlined,
} from '@ant-design/icons'
import type { MenuItem } from '@/types/api'
import { useMenuStore } from '@/store/menuStore'
import { resolverRota } from '@/lib/routeMap'

function resolveIcon(chave: string, url: string, titulo: string) {
  const key = `${chave} ${url} ${titulo}`.toLowerCase()
  if (key.includes('dashboard') || key.includes('home') || key.includes('inicial')) return <DashboardOutlined />
  if (key.includes('compra')) return <ShoppingCartOutlined />
  if (key.includes('financ') || key.includes('dre') || key.includes('fluxo')) return <DollarOutlined />
  if (key.includes('prazo')) return <ClockCircleOutlined />
  if (key.includes('venda') || key.includes('comercial')) return <LineChartOutlined />
  if (key.includes('agric') || key.includes('fruta')) return <AppleOutlined />
  if (key.includes('calendario')) return <CalendarOutlined />
  if (key.includes('usuario') || key.includes('perfil')) return <TeamOutlined />
  if (key.includes('dba') || key.includes('sessao')) return <DatabaseOutlined />
  if (key.includes('bi') || key.includes('acesso')) return <KeyOutlined />
  if (key.includes('config')) return <SettingOutlined />
  return <FileTextOutlined />
}

function buildMenuItems(items: MenuItem[], parentSeen?: Set<string>): MenuProps['items'] {
  const seenKeys = parentSeen ?? new Set<string>()
  const result: MenuProps['items'] = []

  for (const item of items) {
    const rota = resolverRota(item.chaveControle, item.url)
    if (seenKeys.has(rota)) continue
    seenKeys.add(rota)

    const icon = resolveIcon(item.chaveControle, item.url, item.tituloMenu)

    if (item.filhos && item.filhos.length > 0) {
      result.push({
        key: rota,
        icon,
        label: item.tituloMenu,
        children: buildMenuItems(item.filhos, seenKeys),
      })
    } else {
      result.push({ key: rota, icon, label: item.tituloMenu })
    }
  }

  return result
}

/** Coleta folhas (rota → chaveControle) para navegação + telemetria (RF06.2/RF03) */
function collectLeafRoutes(items: MenuItem[], leafKeys: Set<string>, chaveByRoute: Map<string, string>) {
  for (const item of items) {
    const rota = resolverRota(item.chaveControle, item.url)
    if (item.filhos && item.filhos.length > 0) {
      collectLeafRoutes(item.filhos, leafKeys, chaveByRoute)
    } else {
      leafKeys.add(rota)
      chaveByRoute.set(rota, item.chaveControle)
    }
  }
}

// Menu estático de fallback (RF01.8 — usado enquanto a API não responde)
const FALLBACK_MENU: MenuProps['items'] = [
  { key: '/', icon: <DashboardOutlined />, label: 'Dashboard' },
  {
    key: '/compras',
    icon: <ShoppingCartOutlined />,
    label: 'Compras',
    children: [
      { key: '/compras/resumo-anual', label: 'Resumo Anual' },
      { key: '/compras/comite', label: 'Comitê de Compras' },
      { key: '/compras/cfop', label: 'CFOP' },
      { key: '/compras/centro-custo', label: 'Centro de Custo' },
    ],
  },
  {
    key: '/financeiro',
    icon: <DollarOutlined />,
    label: 'Financeiro',
    children: [
      { key: '/financeiro/posicao', label: 'Posição Financeira' },
      { key: '/financeiro/fluxo-caixa', label: 'Fluxo de Caixa' },
      { key: '/financeiro/dre', label: 'DRE' },
    ],
  },
  {
    key: '/prazo-medio',
    icon: <ClockCircleOutlined />,
    label: 'Prazo Médio',
    children: [
      { key: '/prazo-medio/recebimento', label: 'Recebimento' },
      { key: '/prazo-medio/pagamento', label: 'Pagamento' },
    ],
  },
  {
    key: '/vendas',
    icon: <LineChartOutlined />,
    label: 'Vendas',
    children: [
      { key: '/vendas/analise', label: 'Análise' },
      { key: '/vendas/ranking', label: 'Ranking Clientes' },
    ],
  },
  {
    key: '/agricola',
    icon: <AppleOutlined />,
    label: 'Agrícola',
    children: [
      { key: '/agricola/safra-meta', label: 'Cad. Safra/Meta' },
      { key: '/agricola/compras-frutas', label: 'Compras Frutas' },
    ],
  },
  {
    key: '/configuracoes',
    icon: <SettingOutlined />,
    label: 'Configurações',
    children: [
      { key: '/usuarios', label: 'Usuários' },
      { key: '/perfis', label: 'Perfis' },
      { key: '/configuracoes/acesso-bi', label: 'Acesso B.I.', icon: <KeyOutlined /> },
    ],
  },
]

interface SideMenuProps {
  collapsed: boolean
}

/**
 * Menu lateral dinâmico — consome o shell do menu (menuStore ← GET /api/v1/menu).
 * Folha navega (RN-06) + registra telemetria (RF03); grupo expande/colapsa.
 * Fallback estático enquanto a API não responde (RF01.8).
 */
function collectLeafKeysFromItems(items: MenuProps['items'], set: Set<string>) {
  if (!items) return
  for (const item of items) {
    if (!item) continue
    if ('children' in item && item.children && item.children.length > 0) {
      collectLeafKeysFromItems(item.children, set)
    } else if ('key' in item && item.key) {
      set.add(String(item.key))
    }
  }
}

export function SideMenu({ collapsed }: SideMenuProps) {
  const navigate = useNavigate()
  const location = useLocation()
  const menu = useMenuStore((s) => s.menu)
  const carregando = useMenuStore((s) => s.carregando)
  const erro = useMenuStore((s) => s.erro)
  const carregarMenu = useMenuStore((s) => s.carregarMenu)
  const registrarAcesso = useMenuStore((s) => s.registrarAcesso)

  const [userOpenKeys, setUserOpenKeys] = useState<string[]>([])

  useEffect(() => {
    carregarMenu()
  }, [carregarMenu])

  const items = useMemo<MenuProps['items']>(() => {
    if (menu.length > 0) return buildMenuItems(menu)
    return FALLBACK_MENU
  }, [menu])

  const { leafKeys, chaveByRoute } = useMemo(() => {
    const leafKeys = new Set<string>()
    const chaveByRoute = new Map<string, string>()
    if (menu.length > 0) {
      collectLeafRoutes(menu, leafKeys, chaveByRoute)
    } else {
      collectLeafKeysFromItems(FALLBACK_MENU, leafKeys)
    }
    return { leafKeys, chaveByRoute }
  }, [menu])

  const normalizedPath = location.pathname.replace(/\/+$/, '') || '/'

  const pathOpenKeys = useMemo(() => {
    return normalizedPath
      .split('/')
      .filter(Boolean)
      .reduce<string[]>((acc, _, i, arr) => {
        acc.push('/' + arr.slice(0, i + 1).join('/'))
        return acc
      }, [])
  }, [normalizedPath])

  const openKeys = useMemo(() => {
    const merged = new Set([...pathOpenKeys, ...userOpenKeys])
    return Array.from(merged)
  }, [pathOpenKeys, userOpenKeys])

  function handleSelect({ key }: { key: string }) {
    if (!leafKeys.has(key)) return
    const chave = chaveByRoute.get(key)
    if (chave) registrarAcesso(chave)
    navigate(key)
  }

  if (carregando && menu.length === 0) {
    return (
      <div style={{ padding: '12px 16px' }}>
        <Skeleton active paragraph={{ rows: 8 }} title={false} />
      </div>
    )
  }

  return (
    <>
      {erro && (
        <div style={{ padding: '6px 16px 0' }}>
          <Typography.Text
            style={{ fontSize: 12, color: 'rgba(255,255,255,0.45)' }}
          >
            Menu offline — usando fallback estático
          </Typography.Text>
        </div>
      )}
      <Menu
        theme="dark"
        mode="inline"
        inlineCollapsed={collapsed}
        selectedKeys={[normalizedPath]}
        openKeys={openKeys}
        onOpenChange={setUserOpenKeys}
        items={items}
        onClick={handleSelect}
        style={{ border: 'none', paddingTop: erro ? 0 : 8 }}
      />
    </>
  )
}
