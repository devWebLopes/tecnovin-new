import { Table, Input, Button, Space, Tooltip, Typography } from 'antd'
import { SearchOutlined, PlusOutlined } from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'
import type { EmpresaBi, UsuarioBi } from '../types'
import EmpresaVinculoLista from './EmpresaVinculoLista'

const { Text } = Typography

interface UsuarioBiListaProps {
  usuarios: UsuarioBi[]
  loading: boolean
  loadingEmpresas: boolean
  search: string
  expandedIdUsuario: number | null
  empresasUsuario: EmpresaBi[]
  onSearch: (value: string) => void
  onExpand: (idUsuario: number | null) => void
  onRemoverAcesso: (idUsuarioEmpresa: number) => void
  onVincularEmpresa: (idUsuario: number, codigoEmpresa: number) => Promise<boolean>
  onConcederTotal: () => void
}

/**
 * Tabela mestre — usuários com acesso ao B.I.
 * Linhas expandíveis exibem o grid detalhe de empresas vinculadas (RF02).
 * Apenas UMA linha expandida por vez (RF02.8).
 */
export default function UsuarioBiLista({
  usuarios,
  loading,
  loadingEmpresas,
  search,
  expandedIdUsuario,
  empresasUsuario,
  onSearch,
  onExpand,
  onRemoverAcesso,
  onVincularEmpresa,
  onConcederTotal,
}: UsuarioBiListaProps) {
  const columns: ColumnsType<UsuarioBi> = [
    {
      title: 'Usuário',
      dataIndex: 'nome',
      key: 'nome',
      ellipsis: true,
    },
    {
      title: 'Empresas',
      dataIndex: 'empresas',
      key: 'empresas',
      ellipsis: true,
      render: (empresas: string) => <Text>{empresas || '—'}</Text>,
    },
  ]

  return (
    <div>
      <Space style={{ marginBottom: 16, width: '100%', justifyContent: 'space-between' }} align="center">
        <Input
          placeholder="Pesquisar usuário..."
          prefix={<SearchOutlined />}
          allowClear
          value={search}
          onChange={(e) => onSearch(e.target.value)}
          style={{ maxWidth: 320 }}
        />
        <Tooltip title="Concede acesso ao B.I. para todas as empresas de uma vez">
          <Button type="primary" icon={<PlusOutlined />} onClick={onConcederTotal}>
            Conceder Acesso Total
          </Button>
        </Tooltip>
      </Space>

      <Table<UsuarioBi>
        columns={columns}
        dataSource={usuarios}
        rowKey="idUsuario"
        loading={loading}
        size="middle"
        locale={{ emptyText: 'Nenhum usuário com acesso ao B.I. cadastrado.' }}
        expandable={{
          expandedRowKeys: expandedIdUsuario !== null ? [expandedIdUsuario] : [],
          onExpandedRowsChange: (rows) => {
            const key = (rows as readonly number[])[0]
            onExpand(typeof key === 'number' ? key : null)
          },
          expandedRowRender: (record) => (
            <EmpresaVinculoLista
              idUsuario={record.idUsuario}
              loading={loadingEmpresas}
              empresas={expandedIdUsuario === record.idUsuario ? empresasUsuario : []}
              onRemoverAcesso={onRemoverAcesso}
              onVincularEmpresa={onVincularEmpresa}
            />
          ),
        }}
        pagination={false}
        scroll={{ x: 600 }}
      />
    </div>
  )
}