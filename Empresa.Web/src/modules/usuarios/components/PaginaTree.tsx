import { Tree, Spin, Typography, Empty } from 'antd'
import type { DataNode } from 'antd/es/tree'
import { useEffect, useState } from 'react'
import { paginaService } from '../services/paginaService'
import type { PaginaTree as PaginaTreeType } from '../types'

const { Text } = Typography

interface PaginaTreeProps {
  perfilId?: number
}

function toTreeData(nodes: PaginaTreeType[]): DataNode[] {
  return nodes.map((node) => ({
    key: `pagina-${node.idPagina}`,
    title: node.tituloMenu || node.tituloAba || node.chaveControle,
    disabled: true,
    children: node.filhos?.length > 0 ? toTreeData(node.filhos) : undefined,
  }))
}

function getVinculadasKeys(nodes: PaginaTreeType[]): React.Key[] {
  const keys: React.Key[] = []
  for (const node of nodes) {
    if (node.vinculado) keys.push(`pagina-${node.idPagina}`)
    if (node.filhos?.length > 0) {
      keys.push(...getVinculadasKeys(node.filhos))
    }
  }
  return keys
}

/**
 * Tree de páginas com checkboxes — exibe as permissões do perfil selecionado.
 * Backend: GET /api/v1/perfis/{perfilId}/paginas
 */
export default function PaginaTree({ perfilId }: PaginaTreeProps) {
  const [treeData, setTreeData] = useState<DataNode[]>([])
  const [checkedKeys, setCheckedKeys] = useState<React.Key[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    async function carregar() {
      setLoading(true)
      setError(null)
      try {
        if (!perfilId || perfilId <= 0) {
          setTreeData([])
          setCheckedKeys([])
          return
        }

        // O endpoint GET /api/v1/perfis/{perfilId}/paginas retorna a árvore completa
        // com flag vinculado indicando as páginas que o perfil tem acesso
        const paginasPerfil = await paginaService.obterPaginasPorPerfil(perfilId)
        setTreeData(toTreeData(paginasPerfil))
        setCheckedKeys(getVinculadasKeys(paginasPerfil))
      } catch {
        setError('Erro ao carregar permissões do perfil.')
        setTreeData([])
        setCheckedKeys([])
      } finally {
        setLoading(false)
      }
    }
    carregar()
  }, [perfilId])

  if (loading) {
    return (
      <div style={{ display: 'flex', alignItems: 'center', gap: 8, padding: 16 }}>
        <Spin size="small" />
        <Text type="secondary">Carregando permissões...</Text>
      </div>
    )
  }

  if (error) {
    return <Text type="danger">{error}</Text>
  }

  if (!perfilId || perfilId <= 0) {
    return <Text type="secondary">Selecione um perfil para visualizar as permissões</Text>
  }

  if (treeData.length === 0) {
    return <Empty description="Nenhuma página encontrada para este perfil" />
  }

  return (
    <div style={{ maxHeight: 400, overflow: 'auto' }}>
      <Tree
        checkable
        defaultExpandAll
        treeData={treeData}
        checkedKeys={checkedKeys}
        disabled
        style={{ background: 'transparent' }}
      />
    </div>
  )
}