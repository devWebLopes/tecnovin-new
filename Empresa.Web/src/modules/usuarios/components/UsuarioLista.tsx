import { Table, Input, Select, Tag, Space, Button, Tooltip, Typography, Row, Col } from 'antd'
import { SearchOutlined, EditOutlined, DeleteOutlined, PlusOutlined } from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'
import type { Usuario, UsuarioFiltros } from '../types'

const { Text } = Typography

interface UsuarioListaProps {
  usuarios: Usuario[]
  total: number
  loading: boolean
  filtros: UsuarioFiltros
  usuarioSelecionadoId?: number | null
  onSearch: (value: string) => void
  onFiltroChange: (partial: Partial<UsuarioFiltros>) => void
  onPageChange: (page: number, pageSize: number) => void
  onSelect: (id: number) => void
  onDelete: (id: number) => void
  onNovo: () => void
}

/**
 * Tabela de listagem de usuários.
 *
 * Colunas alinhadas com o backend: id, nome, login, ativo, idPerfil + descricaoPerfil
 * Backend retorna array simples (sem paginação server-side).
 */
export default function UsuarioLista({
  usuarios,
  total,
  loading,
  filtros,
  usuarioSelecionadoId,
  onSearch,
  onFiltroChange,
  onPageChange,
  onSelect,
  onDelete,
  onNovo,
}: UsuarioListaProps) {
  const columns: ColumnsType<Usuario> = [
    {
      title: 'ID',
      dataIndex: 'id',
      key: 'id',
      width: 75,
      align: 'center',
    },
    {
      title: 'Nome',
      dataIndex: 'nome',
      key: 'nome',
      width: 200,
      ellipsis: { showTitle: true },
      render: (nome: string) => <span style={{ fontWeight: 500 }}>{nome}</span>,
    },
    {
      title: 'Login',
      dataIndex: 'login',
      key: 'login',
      width: 140,
      ellipsis: { showTitle: true },
    },
    {
      title: 'Ativo',
      dataIndex: 'ativo',
      key: 'ativo',
      width: 85,
      align: 'center',
      render: (ativo: boolean) =>
        ativo ? (
          <Tag color="green">Sim</Tag>
        ) : (
          <Tag color="red">Não</Tag>
        ),
    },
    {
      title: 'Perfil',
      dataIndex: 'descricaoPerfil',
      key: 'descricaoPerfil',
      width: 170,
      ellipsis: { showTitle: true },
      render: (nome: string | null) =>
        nome ? (
          <Tag color="blue" style={{ borderRadius: 4, fontWeight: 500, maxWidth: '100%', overflow: 'hidden', textOverflow: 'ellipsis' }}>
            {nome}
          </Tag>
        ) : (
          <Text type="secondary">—</Text>
        ),
    },
    {
      title: 'Ações',
      key: 'acoes',
      width: 100,
      align: 'center',
      fixed: 'right',
      render: (_: unknown, record: Usuario) => (
        <Space size="small">
          <Tooltip title="Editar">
            <Button
              type="link"
              size="small"
              icon={<EditOutlined />}
              onClick={(e) => {
                e.stopPropagation()
                onSelect(record.id)
              }}
            />
          </Tooltip>
          <Tooltip title="Excluir">
            <Button
              type="link"
              size="small"
              danger
              icon={<DeleteOutlined />}
              onClick={(e) => {
                e.stopPropagation()
                onDelete(record.id)
              }}
            />
          </Tooltip>
        </Space>
      ),
    },
  ]

  return (
    <div>
      {/* Barra de filtros */}
      <Row gutter={[12, 12]} style={{ marginBottom: 16 }} align="middle">
        <Col xs={24} sm={10} md={8} lg={9}>
          <Input
            placeholder="Pesquisar por nome ou login..."
            prefix={<SearchOutlined />}
            allowClear
            defaultValue={filtros.search}
            onChange={(e) => onSearch(e.target.value)}
          />
        </Col>
        <Col xs={12} sm={8} md={6} lg={6}>
          <Select
            value={filtros.situacao}
            onChange={(val) => onFiltroChange({ situacao: val as UsuarioFiltros['situacao'] })}
            style={{ width: '100%' }}
            options={[
              { value: 'todos', label: 'Todos' },
              { value: 'ativos', label: 'Ativos' },
              { value: 'inativos', label: 'Inativos' },
            ]}
          />
        </Col>
        <Col xs={12} sm={6} md={4} lg={4} style={{ textAlign: 'right', marginLeft: 'auto' }}>
          <Button type="primary" icon={<PlusOutlined />} onClick={onNovo}>
            Novo
          </Button>
        </Col>
      </Row>

      {/* Tabela */}
      <Table<Usuario>
        columns={columns}
        dataSource={usuarios}
        rowKey="id"
        loading={loading}
        pagination={{
          current: filtros.page,
          pageSize: filtros.pageSize,
          total,
          showSizeChanger: true,
          pageSizeOptions: ['10', '20', '50', '100'],
          showTotal: (totalItems, range) =>
            `${range[0]}-${range[1]} de ${totalItems} usuários`,
        }}
        onChange={(pagination) => {
          if (pagination.current && pagination.pageSize) {
            onPageChange(pagination.current, pagination.pageSize)
          }
        }}
        scroll={{ x: 770 }}
        size="middle"
        locale={{ emptyText: 'Nenhum usuário encontrado' }}
        rowClassName={(record) => (record.id === usuarioSelecionadoId ? 'ant-table-row-selected' : '')}
        onRow={(record) => ({
          style: { cursor: 'pointer' },
          onClick: () => onSelect(record.id),
        })}
      />
    </div>
  )
}