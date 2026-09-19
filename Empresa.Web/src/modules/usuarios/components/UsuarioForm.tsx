import { useState, useEffect, useCallback } from 'react'
import {
  Tabs,
  Form,
  Input,
  Switch,
  Button,
  Space,
  Row,
  Col,
  Typography,
  Progress,
  Divider,
} from 'antd'
import { SaveOutlined, CloseOutlined } from '@ant-design/icons'
import PerfilSelect from './PerfilSelect'
import PaginaTree from './PaginaTree'
import EstabelecimentoTree from './EstabelecimentoTree'
import type { Usuario, UsuarioRequest, VinculoEstabelecimento } from '../types'

const { Title } = Typography
interface UsuarioFormProps {
  usuario: Usuario | null
  saving: boolean
  estabelecimentosVinculados: VinculoEstabelecimento[]
  onSave: (data: UsuarioRequest, id?: number) => Promise<boolean>
  onSaveVinculos: (vinculos: VinculoEstabelecimento[]) => void
  onCancel: () => void
}

function verificarSenha(senha: string) {
  return {
    minLength: senha.length >= 8,
    hasUpper: /[A-Z]/.test(senha),
    hasNumber: /[0-9]/.test(senha),
    hasSpecial: /[!@#$%^&*()_+\-=[\]{};':"\\|,.<>/?]/.test(senha),
  }
}

function calcularForcaSenha(req: {
  minLength: boolean
  hasUpper: boolean
  hasNumber: boolean
  hasSpecial: boolean
}): number {
  const count = [req.minLength, req.hasUpper, req.hasNumber, req.hasSpecial].filter(Boolean).length
  return (count / 4) * 100
}

function forcaColor(percent: number): string {
  if (percent <= 25) return '#ff4d4f'
  if (percent <= 50) return '#faad14'
  if (percent <= 75) return '#1890ff'
  return '#52c41a'
}

/**
 * Formulário de cadastro/edição de usuário com 3 abas.
 * Alinhado com o backend: login (não email), idPerfil, ativo, atualizaSenha
 */
export default function UsuarioForm({
  usuario,
  saving,
  estabelecimentosVinculados,
  onSave,
  onSaveVinculos,
  onCancel,
}: UsuarioFormProps) {
  const isEditing = usuario && usuario.id > 0
  const [form] = Form.useForm()
  const [activeTab, setActiveTab] = useState('dados')
  const [senha, setSenha] = useState('')
  const [senhaReqs, setSenhaReqs] = useState({
    minLength: false,
    hasUpper: false,
    hasNumber: false,
    hasSpecial: false,
  })
  const [idPerfil, setIdPerfil] = useState<number>(0)
  const [vinculos, setVinculos] = useState<VinculoEstabelecimento[]>([])
  const [errors, setErrors] = useState<Record<string, string>>({})

  useEffect(() => {
    if (usuario) {
      form.setFieldsValue({
        nome: usuario.nome,
        login: usuario.login,
        ativo: usuario.ativo,
        atualizaSenha: usuario.atualizaSenha,
      })
      setIdPerfil(usuario.idPerfil)
      setVinculos(estabelecimentosVinculados || [])
      setSenha('')
      setSenhaReqs({ minLength: false, hasUpper: false, hasNumber: false, hasSpecial: false })
      setErrors({})
    }
  }, [usuario, estabelecimentosVinculados, form])

  const handleSenhaChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value
    setSenha(value)
    setSenhaReqs(verificarSenha(value))
  }, [])

  // Salvar vínculos ao trocar de aba para estabelecimentos
  const handleTabChange = useCallback((key: string) => {
    setActiveTab(key)
    if (key === 'estabelecimentos' && isEditing && usuario) {
      onSaveVinculos(vinculos)
    }
  }, [isEditing, usuario, vinculos, onSaveVinculos])

  const validar = useCallback((): UsuarioRequest | null => {
    const values = form.getFieldsValue()
    const errs: Record<string, string> = {}

    if (!values.nome?.trim()) errs.nome = 'Nome é obrigatório.'
    if (!values.login?.trim()) errs.login = 'Login é obrigatório.'

    if (!isEditing && (!senha || !senha.trim())) {
      errs.senha = 'Senha é obrigatória.'
    } else if (senha?.trim()) {
      const req = verificarSenha(senha)
      if (!req.minLength) errs.senha = 'Mínimo de 8 caracteres.'
      else if (!req.hasUpper) errs.senha = 'Deve conter 1 letra maiúscula.'
      else if (!req.hasNumber) errs.senha = 'Deve conter 1 número.'
      else if (!req.hasSpecial) errs.senha = 'Deve conter 1 caractere especial.'
    }

    setErrors(errs)
    if (Object.keys(errs).length > 0) return null

    return {
      nome: values.nome.trim(),
      login: values.login.trim(),
      senha: senha?.trim() || undefined,
      idPerfil: idPerfil,
      ativo: values.ativo !== false,
      atualizaSenha: values.atualizaSenha === true,
    }
  }, [form, senha, isEditing, idPerfil])

  const handleSubmit = useCallback(async () => {
    const data = validar()
    if (!data) return
    const success = await onSave(data, isEditing ? usuario?.id : undefined)
    if (success && usuario?.id) {
      onSaveVinculos(vinculos)
    }
  }, [validar, onSave, isEditing, usuario, vinculos, onSaveVinculos])

  const forcaPercent = calcularForcaSenha(senhaReqs)

  const tabItems = [
    {
      key: 'dados',
      label: 'Dados Pessoais',
      children: (
        <Form
          form={form}
          layout="vertical"
          initialValues={{ nome: '', login: '', ativo: true, atualizaSenha: false }}
        >
          <Row gutter={24}>
            <Col xs={24} md={12}>
              <Form.Item
                label="Nome"
                name="nome"
                validateStatus={errors.nome ? 'error' : undefined}
                help={errors.nome}
              >
                <Input placeholder="Nome completo" maxLength={100} />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item
                label="Login"
                name="login"
                validateStatus={errors.login ? 'error' : undefined}
                help={errors.login}
              >
                <Input placeholder="login" maxLength={50} />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={24}>
            <Col xs={24} md={12}>
              <Form.Item
                label={isEditing ? 'Nova senha (deixe em branco para manter)' : 'Senha'}
                validateStatus={errors.senha ? 'error' : undefined}
                help={errors.senha}
              >
                <Input.Password
                  placeholder="Senha"
                  onChange={handleSenhaChange}
                />
              </Form.Item>
              {senha && (
                <div style={{ marginBottom: 16 }}>
                  <Progress
                    percent={forcaPercent}
                    strokeColor={forcaColor(forcaPercent)}
                    showInfo={false}
                    size="small"
                  />
                  <span style={{ fontSize: 12, color: forcaColor(forcaPercent) }}>
                    {forcaPercent <= 25 ? 'Fraca' : forcaPercent <= 50 ? 'Média' : forcaPercent <= 75 ? 'Boa' : 'Forte'}
                  </span>
                  <ul style={{ fontSize: 12, margin: '4px 0 0', paddingLeft: 16, color: '#666' }}>
                    <li style={{ color: senhaReqs.minLength ? '#52c41a' : '#ff4d4f' }}>Mínimo 8 caracteres</li>
                    <li style={{ color: senhaReqs.hasUpper ? '#52c41a' : '#ff4d4f' }}>1 letra maiúscula</li>
                    <li style={{ color: senhaReqs.hasNumber ? '#52c41a' : '#ff4d4f' }}>1 número</li>
                    <li style={{ color: senhaReqs.hasSpecial ? '#52c41a' : '#ff4d4f' }}>1 caractere especial</li>
                  </ul>
                </div>
              )}
            </Col>
            <Col xs={24} md={12}>
              <Form.Item label="Ativo" name="ativo" valuePropName="checked">
                <Switch checkedChildren="Sim" unCheckedChildren="Não" />
              </Form.Item>
              <Form.Item label="Exigir alteração de senha no próximo login" name="atualizaSenha" valuePropName="checked">
                <Switch />
              </Form.Item>
            </Col>
          </Row>
        </Form>
      ),
    },
    {
      key: 'perfil',
      label: 'Perfil e Permissões',
      children: (
        <>
          <div style={{ marginBottom: 16 }}>
            <label style={{ display: 'block', marginBottom: 4, fontWeight: 500 }}>
              Perfil de Acesso
            </label>
            <PerfilSelect
              value={idPerfil}
              onChange={(val) => setIdPerfil(val ?? 0)}
              placeholder="Selecione o perfil do usuário"
              allowClear={false}
            />
          </div>

          <Divider />

          <div>
            <label style={{ display: 'block', marginBottom: 8, fontWeight: 500 }}>
              Permissões do Perfil (somente leitura)
            </label>
            <PaginaTree perfilId={idPerfil} />
          </div>
        </>
      ),
    },
    {
      key: 'estabelecimentos',
      label: 'Estabelecimentos',
      children: (
        <>
          <label style={{ display: 'block', marginBottom: 8, fontWeight: 500 }}>
            Selecione os estabelecimentos que o usuário poderá acessar
          </label>
          <EstabelecimentoTree
            selectedVinculos={vinculos}
            onChange={(v) => setVinculos(v)}
          />
        </>
      ),
    },
  ]

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <Title level={5} style={{ margin: 0 }}>
          {isEditing ? `Editando: ${usuario?.nome}` : 'Novo Usuário'}
        </Title>
        <Button
          type="text"
          icon={<CloseOutlined />}
          onClick={onCancel}
          disabled={saving}
          title="Fechar painel"
        />
      </div>

      <Tabs activeKey={activeTab} onChange={handleTabChange} items={tabItems} />

      <Divider />

      <Space style={{ width: '100%', justifyContent: 'flex-end' }}>
        <Button icon={<CloseOutlined />} onClick={onCancel} disabled={saving}>
          Cancelar
        </Button>
        <Button
          type="primary"
          icon={<SaveOutlined />}
          loading={saving}
          onClick={handleSubmit}
        >
          Salvar
        </Button>
      </Space>
    </div>
  )
}