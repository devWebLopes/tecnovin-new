import { List, Button, Typography, Spin, Empty } from 'antd'
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons'
import type { Perfil } from '../types'

const { Text } = Typography

interface PerfilListaProps {
  perfis: Perfil[]
  loading: boolean
  perfilSelecionadoId?: number
  onSelect: (id: number) => void
  onDelete: (id: number) => void
  onNovo: () => void
}

/**
 * Lista de perfis no estilo master (painel esquerdo).
 */
export default function PerfilLista({
  perfis,
  loading,
  perfilSelecionadoId,
  onSelect,
  onDelete,
  onNovo,
}: PerfilListaProps) {
  if (loading) {
    return (
      <div style={{ textAlign: 'center', padding: 24 }}>
        <Spin size="default" />
        <p style={{ marginTop: 8, color: '#6b7280' }}>Carregando perfis...</p>
      </div>
    )
  }

  return (
    <div>
      <div style={{ marginBottom: 12, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Text strong style={{ fontSize: 15 }}>Perfis Cadastrados</Text>
        <Button type="primary" size="small" icon={<PlusOutlined />} onClick={onNovo}>
          Novo
        </Button>
      </div>

      {perfis.length === 0 ? (
        <Empty description="Nenhum perfil encontrado" />
      ) : (
        <List
          dataSource={perfis}
          style={{ maxHeight: 480, overflow: 'auto' }}
          renderItem={(perfil) => (
            <List.Item
              key={perfil.idPerfil}
              onClick={() => onSelect(perfil.idPerfil)}
              style={{
                cursor: 'pointer',
                padding: '8px 12px',
                borderRadius: 6,
                marginBottom: 4,
                background: perfilSelecionadoId === perfil.idPerfil ? '#e6f4ff' : '#fafafa',
                border: perfilSelecionadoId === perfil.idPerfil ? '1px solid #91caff' : '1px solid #f0f0f0',
              }}
              actions={[
                <Button
                  key="delete"
                  type="text"
                  danger
                  size="small"
                  icon={<DeleteOutlined />}
                  onClick={(e) => {
                    e.stopPropagation()
                    onDelete(perfil.idPerfil)
                  }}
                />,
              ]}
            >
              <List.Item.Meta
                title={
                  <Text style={{ fontWeight: perfilSelecionadoId === perfil.idPerfil ? 600 : 400 }}>
                    {perfil.idPerfil} - {perfil.descricao}
                  </Text>
                }
              />
            </List.Item>
          )}
        />
      )}
    </div>
  )
}