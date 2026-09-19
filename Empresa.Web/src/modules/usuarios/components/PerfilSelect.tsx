import { Select, Spin, Typography } from 'antd'
import { usePerfis } from '../hooks/usePerfis'

const { Text } = Typography

interface PerfilSelectProps {
  value?: number
  onChange?: (value: number | null) => void
  placeholder?: string
  allowClear?: boolean
}

/**
 * Dropdown de seleção de perfil.
 * Backend: GET /api/v1/perfis → retorna { idPerfil, descricao }
 */
export default function PerfilSelect({
  value,
  onChange,
  placeholder = 'Selecione um perfil',
  allowClear = true,
}: PerfilSelectProps) {
  const { perfis, loading } = usePerfis()

  if (loading) {
    return (
      <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
        <Spin size="small" />
        <Text type="secondary">Carregando perfis...</Text>
      </div>
    )
  }

  return (
    <Select
      value={value ?? undefined}
      onChange={(val) => onChange?.(val ?? null)}
      placeholder={placeholder}
      allowClear={allowClear}
      style={{ width: '100%' }}
      showSearch
      optionFilterProp="label"
      options={perfis.map((p) => ({
        value: p.idPerfil,
        label: `${p.idPerfil} - ${p.descricao}`,
      }))}
    />
  )
}