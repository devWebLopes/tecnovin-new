import { useState } from 'react'
import { Layout, Button, Dropdown, Avatar, Space, Typography, Divider } from 'antd'
import type { MenuProps } from 'antd'
import {
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  UserOutlined,
  LogoutOutlined,
  LockOutlined,
  BellOutlined,
  BarChartOutlined,
} from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import { SideMenu } from './SideMenu'
import { MaisAcessadosPanel } from './MaisAcessadosPanel'
import { useAuthStore } from '@/store/authStore'
import { useMenuStore } from '@/store/menuStore'

const { Header, Sider, Content } = Layout
const { Text } = Typography

const SIDER_WIDTH = 240
const SIDER_COLLAPSED_WIDTH = 64
const COLLAPSED_STORAGE_KEY = 'menu-collapsed'

/**
 * URL externa do B.I. (RN-09) — configurável via VITE_BI_URL.
 * Legado: CarregaBi.aspx montava a URL a partir do appSetting "caminhoBI".
 */
const BI_URL = import.meta.env.VITE_BI_URL as string | undefined

interface AppLayoutProps {
  children: React.ReactNode
}

/**
 * Layout principal da aplicação.
 * Sidebar fixa (menu + mais acessados) + Header sticky + Content área.
 * RF05 — link condicional do B.I. (exibeLinkBi); RF06.7 — collapsed persistido.
 */
export default function AppLayout({ children }: AppLayoutProps) {
  const [collapsed, setCollapsed] = useState(() => localStorage.getItem(COLLAPSED_STORAGE_KEY) === '1')
  const navigate = useNavigate()
  const { usuario, logout } = useAuthStore()
  const exibeLinkBi = useMenuStore((s) => s.usuario?.exibeLinkBi ?? false)

  function handleCollapse(value: boolean) {
    setCollapsed(value)
    localStorage.setItem(COLLAPSED_STORAGE_KEY, value ? '1' : '0')
  }

  function abrirBi() {
    if (BI_URL) {
      window.open(BI_URL, '_blank', 'noopener,noreferrer')
    } else {
      navigate('/')
      console.warn('[AppLayout] VITE_BI_URL não configurada — atalho do B.I. inativo.')
    }
  }

  const userMenuItems: MenuProps['items'] = [
    {
      key: 'usuario-info',
      disabled: true,
      label: <Text style={{ fontSize: 12, color: '#6b7280' }}>{usuario?.login ?? ''}</Text>,
    },
    { type: 'divider' },
    {
      key: 'alterar-senha',
      icon: <LockOutlined />,
      label: 'Alterar Senha',
      onClick: () => navigate('/alterar-senha'),
    },
    { type: 'divider' },
    {
      key: 'sair',
      icon: <LogoutOutlined />,
      label: 'Sair',
      danger: true,
      onClick: () => {
        logout()
        navigate('/login', { replace: true })
      },
    },
  ]

  const siderWidth = collapsed ? SIDER_COLLAPSED_WIDTH : SIDER_WIDTH

  return (
    <Layout style={{ minHeight: '100vh' }}>
      {/* ─── Sidebar ─────────────────────────────────────────────────────── */}
      <Sider
        className="app-sider"
        collapsible
        collapsed={collapsed}
        onCollapse={handleCollapse}
        width={SIDER_WIDTH}
        collapsedWidth={SIDER_COLLAPSED_WIDTH}
        trigger={null}
      >
        {/* Logo */}
        <div className="sider-logo">
          {!collapsed ? (
            <div style={{ textAlign: 'center' }}>
              <div className="sider-logo-text">
                Gestao<span style={{ color: '#0d9488' }}>New</span>
              </div>
              <span className="sider-logo-sub">Tecnovin</span>
            </div>
          ) : (
            <div style={{ color: '#0d9488', fontSize: 20, fontWeight: 800 }}>G</div>
          )}
        </div>

        {/* Menu dinâmico */}
        <SideMenu collapsed={collapsed} />

        {/* RF02.6 — Mais Acessados (some quando colapsado) */}
        {!collapsed && (
          <>
            <Divider style={{ margin: '8px 0', borderColor: 'rgba(255,255,255,0.12)' }} />
            <MaisAcessadosPanel />
          </>
        )}
      </Sider>

      {/* ─── Main área ───────────────────────────────────────────────────── */}
      <Layout style={{ marginLeft: siderWidth, transition: 'margin-left .2s' }}>
        {/* Header */}
        <Header className="app-header">
          {/* Toggle collapse */}
          <Button
            type="text"
            icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
            onClick={() => handleCollapse(!collapsed)}
            style={{ fontSize: 18, width: 40, height: 40 }}
          />

          {/* Ações do header */}
          <Space size={8}>
            {/* RF05 — Link condicional do B.I. (RN-09) */}
            {exibeLinkBi && (
              <Button
                type="text"
                icon={<BarChartOutlined style={{ fontSize: 18 }} />}
                onClick={abrirBi}
                title="Acesso ao B.I."
              >
                <span style={{ fontSize: 13 }}>Acesso ao B.I.</span>
              </Button>
            )}

            {/* Sino de notificações (placeholder) */}
            <Button type="text" icon={<BellOutlined />} style={{ fontSize: 18 }} />

            {/* Dropdown do usuário */}
            <Dropdown menu={{ items: userMenuItems }} trigger={['click']} placement="bottomRight">
              <Space style={{ cursor: 'pointer', padding: '0 8px' }}>
                <Avatar
                  size={34}
                  icon={<UserOutlined />}
                  style={{ background: '#1a3c5e', cursor: 'pointer' }}
                />
                <Text strong style={{ fontSize: 13, color: '#374151' }}>
                  {usuario?.nome?.split(' ')[0] ?? 'Usuário'}
                </Text>
              </Space>
            </Dropdown>
          </Space>
        </Header>

        {/* Content */}
        <Content className="app-content">
          {children}
        </Content>
      </Layout>
    </Layout>
  )
}
