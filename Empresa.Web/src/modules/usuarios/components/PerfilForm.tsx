import { Form, Input, Button, Space, Divider, Typography, Spin, Tree, message } from 'antd'
import type { DataNode } from 'antd/es/tree'
import { useEffect, useState, useCallback } from 'react'
import { perfilService } from '../services/perfilService'
import type { PaginaTree as PaginaTreeType } from '../types'

const { Text } = Typography

interface PerfilFormProps {
  perfilId: number
  descricaoAtual: string
  saving: boolean
  onSave: (descricao: string, id?: number) => Promise<boolean>
  onCancel: () => void
}

/**
 * Formulário de edição de perfil com árvore interativa de permissões.
 */
export default function PerfilForm({
  perfilId,
  descricaoAtual,
  saving,
  onSave,
  onCancel,
}: PerfilFormProps) {
  const [form] = Form.useForm()
  const [nomeEditado, setNomeEditado] = useState(false)
  const [paginasTree, setPaginasTree] = useState<DataNode[]>([])
  const [checkedKeys, setCheckedKeys] = useState<React.Key[]>([])
  const [initialCheckedKeys, setInitialCheckedKeys] = useState<Set<string>>(new Set())
  const [loadingPaginas, setLoadingPaginas] = useState(false)
  const [savingPermissoes, setSavingPermissoes] = useState(false)

  // Inicializa nome atual
  useEffect(() => {
    form.setFieldsValue({ descricao: descricaoAtual })
    setNomeEditado(false)
  }, [descricaoAtual, form])

  // Carrega árvore de páginas quando perfilId muda
  useEffect(() => {
    if (perfilId <= 0) {
      setPaginasTree([])
      setCheckedKeys([])
      setInitialCheckedKeys(new Set())
      return
    }
    setLoadingPaginas(true)
    perfilService
      .obterPaginasDoPerfil(perfilId)
      .then((data) => {
        const { tree, keys } = buildTreeData(data)
        setPaginasTree(tree)
        setCheckedKeys(keys)
        setInitialCheckedKeys(new Set(keys.map(String)))
      })
      .catch(() => {
        setPaginasTree([])
        setCheckedKeys([])
        setInitialCheckedKeys(new Set())
        message.error('Erro ao carregar permissões de páginas.')
      })
      .finally(() => setLoadingPaginas(false))
  }, [perfilId])

  const onFinish = async (values: { descricao: string }) => {
    const ok = await onSave(values.descricao.trim(), perfilId)
    if (ok && perfilId <= 0) {
      form.resetFields()
      setNomeEditado(false)
    }
  }

  const handleCheckChange = useCallback(
    (checked: React.Key[] | { checked: React.Key[]; halfChecked: React.Key[] }) => {
      const newChecked = Array.isArray(checked) ? checked : checked.checked
      setCheckedKeys(newChecked)
    },
    [],
  )

  const handleSavePermissoes = useCallback(async () => {
    if (perfilId <= 0) return

    const currentSet = new Set(checkedKeys.map(String))

    // Novos vínculos a adicionar (estão marcados agora, mas não estavam inicialmente)
    const vincularIds = checkedKeys
      .map(String)
      .filter((k) => !initialCheckedKeys.has(k))
      .map(extractId)
      .filter((id) => id > 0)

    // Vínculos a remover (estavam marcados inicialmente, mas não estão mais)
    const desvincularIds = Array.from(initialCheckedKeys)
      .filter((k) => !currentSet.has(k))
      .map(extractId)
      .filter((id) => id > 0)

    if (vincularIds.length === 0 && desvincularIds.length === 0) {
      message.info('Nenhuma alteração de permissão para salvar.')
      return
    }

    setSavingPermissoes(true)
    try {
      await perfilService.salvarPermissoes(perfilId, { vincularIds, desvincularIds })
      message.success('Permissões salvas com sucesso!')

      // Recarregar a árvore para refletir o novo estado do banco
      const data = await perfilService.obterPaginasDoPerfil(perfilId)
      const { tree, keys } = buildTreeData(data)
      setPaginasTree(tree)
      setCheckedKeys(keys)
      setInitialCheckedKeys(new Set(keys.map(String)))
    } catch {
      message.error('Erro ao salvar permissões.')
    } finally {
      setSavingPermissoes(false)
    }
  }, [perfilId, checkedKeys, initialCheckedKeys])

  const podeSalvar = nomeEditado || perfilId <= 0

  return (
    <div>
      <div style={{ marginBottom: 16 }}>
        <Text strong style={{ fontSize: 15 }}>
          {perfilId > 0 ? `Editando Perfil #${perfilId}` : 'Novo Perfil'}
        </Text>
      </div>

      <Form form={form} layout="vertical" onFinish={onFinish}>
        <Form.Item
          name="descricao"
          label="Descrição do Perfil"
          rules={[{ required: true, message: 'Descrição é obrigatória' }]}
        >
          <Input
            placeholder="Ex: Administrador, Gerente, Operador..."
            maxLength={255}
            onChange={() => setNomeEditado(true)}
          />
        </Form.Item>

        <Space style={{ marginBottom: 16 }}>
          <Button type="primary" htmlType="submit" loading={saving} disabled={!podeSalvar}>
            {perfilId > 0 ? 'Atualizar' : 'Criar'}
          </Button>
          <Button onClick={onCancel}>Cancelar</Button>
        </Space>
      </Form>

      {perfilId > 0 && (
        <>
          <Divider style={{ margin: '16px 0' }} />
          <div style={{ marginBottom: 12, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Text strong>Permissões de Páginas</Text>
            <Button
              size="small"
              type="primary"
              ghost
              loading={savingPermissoes}
              onClick={handleSavePermissoes}
            >
              Salvar Permissões
            </Button>
          </div>

          {loadingPaginas ? (
            <div style={{ textAlign: 'center', padding: 16 }}>
              <Spin size="small" />
              <Text type="secondary" style={{ marginLeft: 8 }}>Carregando permissões...</Text>
            </div>
          ) : paginasTree.length === 0 ? (
            <Text type="secondary">Nenhuma página cadastrada para gerenciar permissões.</Text>
          ) : (
            <div style={{ maxHeight: 380, overflow: 'auto', border: '1px solid #f0f0f0', borderRadius: 6, padding: 8 }}>
              <Tree
                checkable
                checkStrictly
                defaultExpandAll
                treeData={paginasTree}
                checkedKeys={checkedKeys}
                onCheck={handleCheckChange}
                style={{ background: 'transparent' }}
              />
            </div>
          )}
        </>
      )}
    </div>
  )
}

// ─── Helpers ──────────────────────────────────────────────────────────────────

function extractId(key: string): number {
  const match = key.match(/^pagina-(\d+)$/)
  return match ? Number(match[1]) : 0
}

function buildTreeData(
  nodes: PaginaTreeType[],
): { tree: DataNode[]; keys: React.Key[] } {
  const keys: React.Key[] = []

  function walk(list: PaginaTreeType[], visited = new Set<number>()): DataNode[] {
    if (!list || !Array.isArray(list)) return []

    return list
      .filter((node) => node && !visited.has(node.idPagina))
      .map((node) => {
        visited.add(node.idPagina)
        const key = `pagina-${node.idPagina}`
        if (node.vinculado) {
          keys.push(key)
        }

        return {
          key,
          title: node.tituloMenu || node.tituloAba || node.chaveControle || `Página ${node.idPagina}`,
          children: node.filhos?.length > 0 ? walk(node.filhos, new Set(visited)) : undefined,
        }
      })
  }

  const tree = walk(nodes)
  return { tree, keys }
}
