import { Card, Modal, Spin, Typography, Divider, Row, Col } from 'antd'
import { ExclamationCircleOutlined } from '@ant-design/icons'
import UsuarioLista from './components/UsuarioLista'
import UsuarioForm from './components/UsuarioForm'
import { useUsuarios } from './hooks/useUsuarios'

const { Title } = Typography
const { confirm } = Modal

/**
 * Página principal do módulo de Usuários.
 *
 * Layout master-detail adaptativo alinhado com a API REST real.
 */
export default function UsuarioPage() {
  const {
    usuarios,
    total,
    loading,
    loadingDetalhe,
    saving,
    filtros,
    usuarioSelecionado,
    estabelecimentosUsuario,
    onChangeSearch,
    onChangeFiltro,
    onChangePage,
    selecionarUsuario,
    limparSelecao,
    salvar,
    excluir,
    criarNovo,
    sincronizarEstabelecimentos,
  } = useUsuarios()

  const handleDelete = (id: number) => {
    confirm({
      title: 'Excluir usuário',
      icon: <ExclamationCircleOutlined />,
      content: 'Tem certeza que deseja excluir este usuário? Esta ação é irreversível.',
      okText: 'Sim, excluir',
      okType: 'danger',
      cancelText: 'Cancelar',
      onOk: () => excluir(id),
    })
  }

  const temDetalheAberto = Boolean(usuarioSelecionado || loadingDetalhe)

  return (
    <div>
      <div style={{ marginBottom: 20 }}>
        <Title level={4} style={{ margin: 0 }}>Gestão de Usuários</Title>
        <span style={{ color: '#6b7280', fontSize: 14 }}>
          Cadastro e gerenciamento de usuários, perfis e vínculos com estabelecimentos
        </span>
      </div>

      <Divider style={{ margin: '0 0 20px' }} />

      <Row gutter={[20, 20]}>
        {/* Master: Lista de usuários */}
        <Col xs={24} lg={temDetalheAberto ? 13 : 24} xl={temDetalheAberto ? 14 : 24}>
          <Card styles={{ body: { padding: 16 } }}>
            <UsuarioLista
              usuarios={usuarios}
              total={total}
              loading={loading}
              filtros={filtros}
              usuarioSelecionadoId={usuarioSelecionado?.id}
              onSearch={onChangeSearch}
              onFiltroChange={onChangeFiltro}
              onPageChange={onChangePage}
              onSelect={selecionarUsuario}
              onDelete={handleDelete}
              onNovo={criarNovo}
            />
          </Card>
        </Col>

        {/* Detail: Formulário (exibido ao selecionar ou clicar em Novo) */}
        {temDetalheAberto && (
          <Col xs={24} lg={11} xl={10}>
            {loadingDetalhe ? (
              <Card>
                <div style={{ textAlign: 'center', padding: 40 }}>
                  <Spin size="large" />
                  <p style={{ marginTop: 16, color: '#6b7280' }}>Carregando dados do usuário...</p>
                </div>
              </Card>
            ) : usuarioSelecionado ? (
              <Card styles={{ body: { padding: 16 } }}>
                <UsuarioForm
                  usuario={usuarioSelecionado}
                  saving={saving}
                  estabelecimentosVinculados={estabelecimentosUsuario}
                  onSave={salvar}
                  onSaveVinculos={(vinculos) => {
                    if (usuarioSelecionado.id > 0) {
                      sincronizarEstabelecimentos(usuarioSelecionado.id, vinculos)
                    }
                  }}
                  onCancel={limparSelecao}
                />
              </Card>
            ) : null}
          </Col>
        )}
      </Row>
    </div>
  )
}