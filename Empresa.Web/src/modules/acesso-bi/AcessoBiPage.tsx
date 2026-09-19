import { useState } from 'react'
import { Card, Typography, Divider } from 'antd'
import { DownOutlined, RightOutlined } from '@ant-design/icons'
import UsuarioBiLista from './components/UsuarioBiLista'
import UsuarioBiForm from './components/UsuarioBiForm'
import { useAcessoBi } from './hooks/useAcessoBi'

const { Title } = Typography

/**
 * Página principal do painel "Acesso B.I." (RF02–RF06).
 *
 * Título: "Usuários com acesso ao B.I. por empresas"
 * Cabeçalho colapsável funcional (correção do defeito D1 do legado —
 * o legado referenciava o id `dadosDados` inexistente em vez de `dadosFiltro`).
 */
export default function AcessoBiPage() {
  const {
    usuarios,
    loading,
    search,
    expandedIdUsuario,
    empresasUsuario,
    loadingEmpresas,
    saving,
    onChangeSearch,
    expandirUsuario,
    concederAcessoTotal,
    vincularEmpresa,
    removerAcesso,
  } = useAcessoBi()

  const [colapsado, setColapsado] = useState(false)
  const [modalAberto, setModalAberto] = useState(false)

  return (
    <div>
      <div style={{ marginBottom: 24 }}>
        <Title level={4} style={{ margin: 0 }}>Usuários com acesso ao B.I. por empresas</Title>
        <span style={{ color: '#6b7280', fontSize: 14 }}>
          Gerencia quais usuários podem acessar o módulo de B.I. e para quais empresas
        </span>
      </div>

      <Divider style={{ margin: '0 0 24px' }} />

      {/* Cabeçalho colapsável — correção D1 */}
      <div style={{ border: '1px solid #d9d9d9', borderRadius: 6, background: '#fff' }}>
        <div
          style={{
            background: '#0B74A3',
            color: '#fff',
            padding: '8px 16px',
            borderRadius: '6px 6px 0 0',
            cursor: 'pointer',
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            fontWeight: 'bold',
            fontSize: 14,
          }}
          onClick={() => setColapsado((v) => !v)}
        >
          <span>Usuários com acesso ao B.I. por empresas</span>
          <span>{colapsado ? <RightOutlined /> : <DownOutlined />}</span>
        </div>

        {!colapsado && (
          <div style={{ padding: 16, background: '#F2F2F2' /* D5 corrigido */ }}>
            <Card styles={{ body: { padding: 16 } }}>
              <UsuarioBiLista
                usuarios={usuarios}
                loading={loading}
                loadingEmpresas={loadingEmpresas}
                search={search}
                expandedIdUsuario={expandedIdUsuario}
                empresasUsuario={empresasUsuario}
                onSearch={onChangeSearch}
                onExpand={expandirUsuario}
                onRemoverAcesso={removerAcesso}
                onVincularEmpresa={vincularEmpresa}
                onConcederTotal={() => setModalAberto(true)}
              />
            </Card>
          </div>
        )}
      </div>

      {/* Modal de concessão total — RF04 */}
      <UsuarioBiForm
        open={modalAberto}
        saving={saving}
        usuariosComAcesso={usuarios}
        onCancel={() => setModalAberto(false)}
        onSubmit={async (idUsuario) => {
          const ok = await concederAcessoTotal(idUsuario)
          if (ok) setModalAberto(false)
          return ok
        }}
      />
    </div>
  )
}