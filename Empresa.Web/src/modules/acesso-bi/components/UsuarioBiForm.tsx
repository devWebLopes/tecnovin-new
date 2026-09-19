import { Modal, Select, Space, Typography, Spin } from 'antd'
import { useState, useEffect } from 'react'
import api from '@/lib/api'
import type { UsuarioBi } from '../types'

const { Text } = Typography

interface UsuarioBiFormProps {
  open: boolean
  saving: boolean
  usuariosComAcesso: UsuarioBi[]
  onCancel: () => void
  onSubmit: (idUsuario: number) => Promise<boolean>
}

/**
 * Modal de concessão total — usuário → todas as empresas (RF04).
 * O dropdown lista apenas usuários que AINDA NÃO possuem acesso ao B.I.
 * (correção do defeito D6 do legado).
 */
export default function UsuarioBiForm({
  open,
  saving,
  usuariosComAcesso,
  onCancel,
  onSubmit,
}: UsuarioBiFormProps) {
  const [usuarios, setUsuarios] = useState<Array<{ idUsuario: number; nome: string }>>([])
  const [loadingUsuarios, setLoadingUsuarios] = useState(false)
  const [idSelecionado, setIdSelecionado] = useState<number | null>(null)

  const idsComAcesso = new Set(usuariosComAcesso.map((u) => u.idUsuario))

  useEffect(() => {
    if (!open) return
    let ativo = true
    setLoadingUsuarios(true)
    setIdSelecionado(null)
    api
      .get<Array<{ id: number; nome: string; ativo: boolean }>>('/usuarios')
      .then(({ data }) => {
        if (!ativo) return
        // Filtra: apenas usuários ativos que NÃO possuem acesso ao B.I. (D6)
        const semAcesso = (data || [])
          .filter((u) => u.ativo && !idsComAcesso.has(u.id))
          .map((u) => ({ idUsuario: u.id, nome: u.nome }))
        setUsuarios(semAcesso)
      })
      .catch(() => {
        if (ativo) setUsuarios([])
      })
      .finally(() => {
        if (ativo) setLoadingUsuarios(false)
      })
    return () => {
      ativo = false
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open])

  const handleOk = async () => {
    if (idSelecionado === null) return
    const ok = await onSubmit(idSelecionado)
    if (ok) setIdSelecionado(null)
  }

  return (
    <Modal
      title="Conceder Acesso ao B.I."
      open={open}
      onOk={handleOk}
      onCancel={onCancel}
      okText="Conceder"
      cancelText="Cancelar"
      okButtonProps={{ disabled: idSelecionado === null, loading: saving }}
      destroyOnHidden
    >
      <Space direction="vertical" style={{ width: '100%' }}>
        <Text type="secondary">
          Concede acesso ao B.I. para <strong>todas</strong> as empresas de uma vez.
        </Text>
        {loadingUsuarios ? (
          <div style={{ textAlign: 'center', padding: 16 }}>
            <Spin size="small" />
          </div>
        ) : (
          <Select
            style={{ width: '100%' }}
            placeholder="Selecione um usuário sem acesso..."
            value={idSelecionado ?? undefined}
            allowClear
            showSearch
            optionFilterProp="label"
            options={usuarios.map((u) => ({ value: u.idUsuario, label: u.nome }))}
            onChange={(val: number | undefined) => setIdSelecionado(val ?? null)}
            notFoundContent={
              usuarios.length === 0 ? 'Todos os usuários já possuem acesso.' : undefined
            }
          />
        )}
      </Space>
    </Modal>
  )
}