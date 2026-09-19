import { useState } from 'react'
import { Form, Input, Button, Card, Alert, message } from 'antd'
import { LockOutlined, SaveOutlined } from '@ant-design/icons'
import api from '@/lib/api'

interface AlterarSenhaValues {
  senhaAtual: string
  novaSenha: string
  confirmarSenha: string
}

/**
 * Página de Alteração de Senha
 * Integra com PATCH /api/v1/auth/alterar-senha
 */
export default function AlterarSenhaPage() {
  const [form] = Form.useForm<AlterarSenhaValues>()
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function handleSubmit(values: AlterarSenhaValues) {
    setLoading(true)
    setError(null)
    try {
      await api.patch('/auth/alterar-senha', {
        senhaAtual: values.senhaAtual,
        novaSenha: values.novaSenha,
        confirmarSenha: values.confirmarSenha,
      })
      message.success('Senha alterada com sucesso!')
      form.resetFields()
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ||
        'Não foi possível alterar a senha.'
      setError(msg)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div style={{ maxWidth: 480 }}>
      <Card
        title={
          <span>
            <LockOutlined style={{ marginRight: 8, color: '#1a3c5e' }} />
            Alterar Senha
          </span>
        }
        bordered={false}
        style={{ borderRadius: 12, boxShadow: '0 1px 3px rgba(0,0,0,.08)' }}
      >
        {error && (
          <Alert
            message={error}
            type="error"
            showIcon
            closable
            onClose={() => setError(null)}
            style={{ marginBottom: 20 }}
          />
        )}

        <Form form={form} layout="vertical" onFinish={handleSubmit} size="large">
          <Form.Item
            name="senhaAtual"
            label="Senha Atual"
            rules={[{ required: true, message: 'Informe a senha atual' }]}
          >
            <Input.Password prefix={<LockOutlined />} placeholder="Senha atual" />
          </Form.Item>

          <Form.Item
            name="novaSenha"
            label="Nova Senha"
            rules={[
              { required: true, message: 'Informe a nova senha' },
              { min: 6, message: 'Mínimo 6 caracteres' },
            ]}
          >
            <Input.Password prefix={<LockOutlined />} placeholder="Nova senha" />
          </Form.Item>

          <Form.Item
            name="confirmarSenha"
            label="Confirmar Nova Senha"
            dependencies={['novaSenha']}
            rules={[
              { required: true, message: 'Confirme a nova senha' },
              ({ getFieldValue }) => ({
                validator(_, value) {
                  if (!value || getFieldValue('novaSenha') === value) {
                    return Promise.resolve()
                  }
                  return Promise.reject(new Error('As senhas não coincidem'))
                },
              }),
            ]}
          >
            <Input.Password prefix={<LockOutlined />} placeholder="Confirmar nova senha" />
          </Form.Item>

          <Form.Item style={{ marginBottom: 0 }}>
            <Button
              type="primary"
              htmlType="submit"
              loading={loading}
              icon={<SaveOutlined />}
              style={{ fontWeight: 600 }}
            >
              Salvar Nova Senha
            </Button>
          </Form.Item>
        </Form>
      </Card>
    </div>
  )
}
