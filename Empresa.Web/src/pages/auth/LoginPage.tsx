import { useState } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { Form, Input, Button, Alert } from 'antd'
import { UserOutlined, LockOutlined, LoginOutlined } from '@ant-design/icons'
import { useAuthStore } from '@/store/authStore'

interface LoginFormValues {
  login: string
  senha: string
}

/**
 * Página de Login — GestaoNew
 *
 * Integra com POST /api/v1/auth/login via authStore.
 * Redireciona para a URL original após autenticação bem-sucedida.
 */
export default function LoginPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const login = useAuthStore((s) => s.login)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const from = (location.state as { from?: Location })?.from?.pathname || '/'

  async function handleSubmit(values: LoginFormValues) {
    setLoading(true)
    setError(null)
    try {
      await login(values.login, values.senha)
      navigate(from, { replace: true })
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ||
        'Usuário ou senha inválidos.'
      setError(msg)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="login-wrapper">
      <div className="login-card">
        {/* Logo */}
        <div className="login-logo">
          <div className="login-logo-name">
            Gestao<span className="login-logo-accent">New</span>
          </div>
          <div className="login-logo-subtitle">Sistema de Gestão Empresarial · Tecnovin</div>
        </div>

        {/* Erro de autenticação */}
        {error && (
          <Alert
            message={error}
            type="error"
            showIcon
            closable
            onClose={() => setError(null)}
            style={{ marginBottom: 24 }}
          />
        )}

        {/* Formulário */}
        <Form
          name="login"
          onFinish={handleSubmit}
          layout="vertical"
          size="large"
          autoComplete="off"
        >
          <Form.Item
            name="login"
            label="Usuário"
            rules={[{ required: true, message: 'Informe seu usuário' }]}
          >
            <Input
              prefix={<UserOutlined style={{ color: '#9ca3af' }} />}
              placeholder="seu.login"
              autoFocus
            />
          </Form.Item>

          <Form.Item
            name="senha"
            label="Senha"
            rules={[{ required: true, message: 'Informe sua senha' }]}
          >
            <Input.Password
              prefix={<LockOutlined style={{ color: '#9ca3af' }} />}
              placeholder="••••••••"
            />
          </Form.Item>

          <Form.Item style={{ marginBottom: 0, marginTop: 8 }}>
            <Button
              type="primary"
              htmlType="submit"
              loading={loading}
              icon={<LoginOutlined />}
              block
              style={{ height: 46, fontWeight: 600, fontSize: 15 }}
            >
              Entrar
            </Button>
          </Form.Item>
        </Form>

        <div style={{ textAlign: 'center', marginTop: 20, fontSize: 12, color: '#9ca3af' }}>
          © 2026 Tecnovin — Todos os direitos reservados
        </div>
      </div>
    </div>
  )
}
