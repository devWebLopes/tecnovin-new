import { Tree, Spin, Typography, Empty } from 'antd'
import type { DataNode } from 'antd/es/tree'
import { useEffect, useState, useCallback } from 'react'
import { estabelecimentoService } from '../services/estabelecimentoService'
import type { EstabelecimentoGrupo, VinculoEstabelecimento } from '../types'

const { Text } = Typography

interface EstabelecimentoTreeProps {
  selectedVinculos?: VinculoEstabelecimento[]
  onChange?: (vinculos: VinculoEstabelecimento[]) => void
  disabled?: boolean
}

function toTreeData(grupos: EstabelecimentoGrupo[]): DataNode[] {
  return grupos.map((grupo) => ({
    key: `empresa-${grupo.cdEmpresa}`,
    title: grupo.dsEmpresa,
    selectable: false,
    children: grupo.estabelecimentos.map((filho) => ({
      key: `estab-${grupo.cdEmpresa}-${filho.cdEstabelecimento}`,
      title: filho.dsEstabelecimento,
      isLeaf: true,
    })),
  }))
}

/**
 * Tree de estabelecimentos com checkboxes editáveis.
 * Backend: GET /api/v1/estabelecimentos/tree
 * O vínculo é feito via arrays vincular/desvincular.
 */
export default function EstabelecimentoTree({
  selectedVinculos = [],
  onChange,
  disabled = false,
}: EstabelecimentoTreeProps) {
  const [treeData, setTreeData] = useState<DataNode[]>([])
  const [checkedKeys, setCheckedKeys] = useState<React.Key[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  // Converte vinculos para keys
  const vinculosToKeys = useCallback((vinculos: VinculoEstabelecimento[]): React.Key[] => {
    return vinculos.map((v) => `estab-${v.cdEmpresa}-${v.cdEstabelecimento}`)
  }, [])

  // Carrega a árvore ao montar
  useEffect(() => {
    async function carregar() {
      setLoading(true)
      setError(null)
      try {
        const tree = await estabelecimentoService.obterTree()
        setTreeData(toTreeData(tree))
        setCheckedKeys(vinculosToKeys(selectedVinculos))
      } catch {
        setError('Erro ao carregar árvore de estabelecimentos.')
        setTreeData([])
      } finally {
        setLoading(false)
      }
    }
    carregar()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  // Atualiza checkeds quando selectedVinculos mudar externamente
  useEffect(() => {
    setCheckedKeys(vinculosToKeys(selectedVinculos))
  }, [selectedVinculos, vinculosToKeys])

  const handleCheck = useCallback(
    (checked: React.Key[] | { checked: React.Key[]; halfChecked: React.Key[] }) => {
      const keys = Array.isArray(checked) ? checked : checked.checked
      setCheckedKeys(keys)

      const vinculos: VinculoEstabelecimento[] = []
      for (const key of keys) {
        const parts = String(key).split('-')
        if (parts.length === 3) {
          vinculos.push({
            cdEmpresa: Number(parts[1]),
            cdEstabelecimento: Number(parts[2]),
          })
        }
      }
      onChange?.(vinculos)
    },
    [onChange],
  )

  if (loading) {
    return (
      <div style={{ display: 'flex', alignItems: 'center', gap: 8, padding: 16 }}>
        <Spin size="small" />
        <Text type="secondary">Carregando estabelecimentos...</Text>
      </div>
    )
  }

  if (error) {
    return <Text type="danger">{error}</Text>
  }

  if (treeData.length === 0) {
    return <Empty description="Nenhum estabelecimento encontrado" />
  }

  return (
    <div style={{ maxHeight: 400, overflow: 'auto' }}>
      <Tree
        checkable
        defaultExpandAll
        treeData={treeData}
        checkedKeys={checkedKeys}
        onCheck={handleCheck}
        disabled={disabled}
        style={{ background: 'transparent' }}
      />
    </div>
  )
}