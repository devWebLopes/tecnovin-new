import type { ThemeConfig } from 'antd'

/**
 * Tema Ant Design customizado — GestaoNew / Tecnovin
 * Cores corporativas: azul-marinho profundo + verde-azulado como accent
 */
export const theme: ThemeConfig = {
  token: {
    colorPrimary: '#1a3c5e',
    colorSuccess: '#10b981',
    colorWarning: '#f59e0b',
    colorError: '#ef4444',
    colorInfo: '#3b82f6',
    colorLink: '#0d9488',
    colorBgBase: '#f0f2f5',
    colorTextBase: '#1f2937',
    borderRadius: 8,
    fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif",
    fontSize: 14,
    lineHeight: 1.6,
    wireframe: false,
  },
  components: {
    Layout: {
      siderBg: '#001529',
      triggerBg: '#002140',
    },
    Menu: {
      darkItemBg: '#001529',
      darkSubMenuItemBg: '#000c17',
      darkItemSelectedBg: '#0d9488',
      darkItemHoverBg: 'rgba(255,255,255,.06)',
      itemHeight: 44,
    },
    Table: {
      headerBg: '#f8fafc',
      headerColor: '#374151',
      borderColor: '#e5e7eb',
      rowHoverBg: '#f0fdf9',
    },
    Card: {
      borderRadiusLG: 12,
    },
    Button: {
      borderRadius: 8,
      primaryShadow: '0 2px 8px rgba(26,60,94,.25)',
    },
    Input: {
      borderRadius: 8,
    },
    Select: {
      borderRadius: 8,
    },
  },
}
