import { Table, Button, Space, Popconfirm, Typography } from 'antd'
import { DeleteOutlined, LinkOutlined } from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'
import { useState, useEffect } from 'react'
import { acessoBiService } from '../services/acessoBiService'
import type { EmpresaBi, EmpresaDisponivel } from '../types'

const { Text } = Typography

interface EmpresaVinculoListaProps {
  idUsuario: number
  loading: boolean
  empresas: EmpresaBi[]
  onRemoverAcesso: (idUsuarioEmpresa: number) => void
  onVincularEmpresa: (idUsuario: number, codigoEmpresa: number) => Promise<boolean>
}

/**
 * Grid detalhe — empresas vinculadas ao usuário expandido (RF03).
 * Exibe botão "Vincular Empresa" (dropdown de empresas não vinculadas — RF05)
 * e botão "Remover" com Popconfirm (RF06).
 */
export default function EmpresaVinculoLista({
  idUsuario,
  loading,
  empresas,
  onRemoverAcesso,
  onVincularEmpresa,
}: EmpresaVinculoListaProps) {
  const [disponiveis, setDisponiveis] = useState<EmpresaDisponivel[]>([])
  const [loadingDisponiveis, setLoadingDisponiveis] = useState(false)
  const [codigoSelecionado, setCodigoSelecionado] = useState<number | null>(null)

  // Carrega empresas disponíveis (não vinculadas) para o dropdown
  useEffect(() => {
    let ativo = true
    setLoadingDisponiveis(true)
    acessoBiService
      .listarEmpresasDisponiveis(idUsuario)
      .then((data) => {
        if (ativo) setDisponiveis(data || [])
      })
      .catch(() => {
        if (ativo) setDisponiveis([])
      })
      .finally(() => {
        if (ativo) setLoadingDisponiveis(false)
      })
    return () => {
      ativo = false
    }
  }, [idUsuario, empresas])

  const columns: ColumnsType<EmpresaBi> = [
    {
      title: 'Empresa',
      dataIndex: 'nomeFantasia',
      key: 'nomeFantasia',
      ellipsis: true,
    },
    {
      title: 'Ações',
      key: 'acoes',
      width: 90,
      render: (_, record) => (
        <Popconfirm
          title="Deseja realmente excluir o acesso deste usuário ao BI da empresa selecionada?"
          okText="Sim, excluir"
          okType="danger"
          cancelText="Cancelar"
          onConfirm={() => onRemoverAcesso(record.idUsuarioEmpresa)}
        >
          <Button type="link" danger size="small" icon={<DeleteOutlined />}>
            Remover
          </Button>
        </Popconfirm>
      ),
    },
  ]

  const handleVincular = async () => {
    if (codigoSelecionado === null) return
    const ok = await onVincularEmpresa(idUsuario, codigoSelecionado)
    if (ok) setCodigoSelecionado(null)
  }

  return (
    <div>
      <Space style={{ marginBottom: 12 }} align="center">
        <Button
          type="default"
          size="small"
          icon={<LinkOutlined />}
          loading={loadingDisponiveis}
          disabled={disponiveis.length === 0}
          onClick={handleVincular}
        >
          Vincular Empresa
        </Button>
        {codigoSelecionado !== null && (
          <Text type="secondary" style={{ fontSize: 13 }}>
            {disponiveis.find((d) => d.codigoEmpresa === codigoSelecionado)?.nomeFantasia}
          </Text>
        )}
      </Space>

      <Table<EmpresaBi>
        columns={columns}
        dataSource={empresas}
        rowKey="idUsuarioEmpresa"
        loading={loading}
        size="small"
        pagination={false}
        locale={{ emptyText: 'Nenhuma empresa vinculada.' }}
      />
    </div>
  )
}