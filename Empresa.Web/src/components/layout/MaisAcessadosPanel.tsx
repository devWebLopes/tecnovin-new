import { useNavigate } from 'react-router-dom'
import { List, Typography } from 'antd'
import { FileTextOutlined } from '@ant-design/icons'
import { useMenuStore } from '@/store/menuStore'
import { resolverRota } from '@/lib/routeMap'

/**
 * RF02 — Painel "Mais Acessados": top 10 do usuário (lista plana, ordenada por quantidade).
 * Clique navega para a rota SPA e registra telemetria (RF02.4).
 */
export function MaisAcessadosPanel() {
  const navigate = useNavigate()
  const maisAcessados = useMenuStore((s) => s.maisAcessados)
  const registrarAcesso = useMenuStore((s) => s.registrarAcesso)

  if (maisAcessados.length === 0) {
    // RF02.5 — empty state
    return (
      <div style={{ padding: '8px 16px 12px' }}>
        <Typography.Text strong style={{ fontSize: 12, color: 'rgba(255,255,255,0.65)' }}>
          Mais Acessados
        </Typography.Text>
        <Typography.Paragraph style={{ fontSize: 12, color: 'rgba(255,255,255,0.45)', marginTop: 8, marginBottom: 0 }}>
          Nenhuma página acessada ainda.
        </Typography.Paragraph>
      </div>
    )
  }

  return (
    <div style={{ padding: '8px 8px 12px 16px' }}>
      <Typography.Text strong style={{ fontSize: 12, color: 'rgba(255,255,255,0.65)' }}>
        Mais Acessados
      </Typography.Text>
      <List
        size="small"
        dataSource={maisAcessados}
        renderItem={(item) => {
          const rota = resolverRota(item.chaveControle, item.url)
          return (
            <List.Item
              style={{ cursor: 'pointer', padding: '4px 0', border: 'none' }}
              onClick={() => {
                registrarAcesso(item.chaveControle)
                navigate(rota)
              }}
            >
              <List.Item.Meta
                avatar={<FileTextOutlined style={{ color: 'rgba(255,255,255,0.65)' }} />}
                title={
                  <span style={{ fontSize: 12, color: 'rgba(255,255,255,0.85)' }}>{item.tituloMenu}</span>
                }
                description={
                  <span style={{ fontSize: 11, color: 'rgba(255,255,255,0.45)' }}>
                    {item.quantidade} acessos
                  </span>
                }
              />
            </List.Item>
          )
        }}
      />
    </div>
  )
}
