import { Select, Space, Typography } from 'antd'
import type { EmpresaDisponivel } from '../types'

const { Text } = Typography

interface EmpresaSelectProps {
  empresas: EmpresaDisponivel[]
  loading: boolean
  value: number | null
  placeholder?: string
  onChange: (codigoEmpresa: number | null) => void
}

/**
 * Dropdown de empresas disponíveis para vínculo (RF05).
 * Exibe nomeFantasia como label e codigoEmpresa como value.
 */
export default function EmpresaSelect({
  empresas,
  loading,
  value,
  placeholder = 'Selecione uma empresa...',
  onChange,
}: EmpresaSelectProps) {
  return (
    <Space>
      <Select
        style={{ minWidth: 260 }}
        placeholder={placeholder}
        loading={loading}
        value={value ?? undefined}
        allowClear
        showSearch
        optionFilterProp="label"
        options={empresas.map((empresa) => ({
          value: empresa.codigoEmpresa,
          label: empresa.nomeFantasia,
        }))}
        onChange={(val: number | undefined) => onChange(val ?? null)}
      />
      {value !== null && (
        <Text type="secondary" style={{ fontSize: 13 }}>
          {empresas.find((e) => e.codigoEmpresa === value)?.nomeFantasia}
        </Text>
      )}
    </Space>
  )
}