import { Button, Result } from 'antd'
import { useNavigate } from 'react-router-dom'

/**
 * RF04.7 — Tela "Sem Permissão" (RN-12): exibida quando o usuário acessa uma rota
 * sem o item correspondente no menu (dados já filtrados pelo backend).
 */
export default function SemPermissaoPage() {
  const navigate = useNavigate()

  return (
    <Result
      status="403"
      title="Sem permissão de acesso"
      subTitle="Você não possui permissão para acessar esta página. Caso precise, solicite ao administrador o vínculo da página ao seu perfil."
      extra={
        <Button type="primary" onClick={() => navigate('/')}>
          Voltar ao Dashboard
        </Button>
      }
    />
  )
}
