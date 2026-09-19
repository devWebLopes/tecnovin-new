import { useEffect } from 'react'
import { Card, Modal, Spin, Typography, Divider } from 'antd'
import { ExclamationCircleOutlined } from '@ant-design/icons'
import PerfilLista from './components/PerfilLista'
import PerfilForm from './components/PerfilForm'
import { usePerfilAdmin } from './hooks/usePerfilAdmin'

const { Title } = Typography
const { confirm } = Modal

/**
 * Página principal do módulo de Perfis.
 *
 * Layout master-detail: lista à esquerda, formulário + permissões à direita.
 * Backend: API REST /api/v1/perfis/* (CRUD + permissões de páginas)
 */
export default function PerfilPage() {
  const {
    perfis,
    loading,
    perfilSelecionado,
    loadingSelecionado,
    saving,
    carregarPerfis,
    selecionarPerfil,
    limparSelecao,
    criarNovo,
    salvar,
    excluir,
  } = usePerfilAdmin()

  useEffect(() => {
    carregarPerfis()
  }, [carregarPerfis])

  const handleDelete = (id: number) => {
    confirm({
      title: 'Excluir perfil',
      icon: <ExclamationCircleOutlined />,
      content: 'Tem certeza que deseja excluir este perfil? Esta ação é irreversível.',
      okText: 'Sim, excluir',
      okType: 'danger',
      cancelText: 'Cancelar',
      onOk: () => excluir(id),
    })
  }

  return (
    <div>
      <div style={{ marginBottom: 24 }}>
        <Title level={4} style={{ margin: 0 }}>Gestão de Perfis</Title>
        <span style={{ color: '#6b7280', fontSize: 14 }}>
          Cadastro e gerenciamento de perfis de acesso e permissões de páginas
        </span>
      </div>

      <Divider style={{ margin: '0 0 24px' }} />

      <div style={{ display: 'flex', gap: 24, flexWrap: 'wrap' }}>
        {/* Master: Lista de perfis */}
        <div style={{ flex: 1, minWidth: 280, maxWidth: 380 }}>
          <Card styles={{ body: { padding: 16 } }}>
            <PerfilLista
              perfis={perfis}
              loading={loading}
              perfilSelecionadoId={perfilSelecionado?.idPerfil}
              onSelect={selecionarPerfil}
              onDelete={handleDelete}
              onNovo={criarNovo}
            />
          </Card>
        </div>

        {/* Detail: Formulário + Permissões */}
        <div style={{ flex: 2, minWidth: 420 }}>
          {loadingSelecionado ? (
            <Card>
              <div style={{ textAlign: 'center', padding: 40 }}>
                <Spin size="large" />
                <p style={{ marginTop: 16, color: '#6b7280' }}>Carregando dados do perfil...</p>
              </div>
            </Card>
          ) : perfilSelecionado ? (
            <Card styles={{ body: { padding: 16 } }}>
              <PerfilForm
                perfilId={perfilSelecionado.idPerfil}
                descricaoAtual={perfilSelecionado.descricao}
                saving={saving}
                onSave={salvar}
                onCancel={limparSelecao}
              />
            </Card>
          ) : (
            <Card>
              <div style={{ textAlign: 'center', padding: 40, color: '#9ca3af' }}>
                <p style={{ marginBottom: 8 }}>
                  Selecione um perfil na lista para editar
                </p>
                <p style={{ fontSize: 13 }}>
                  ou clique em "Novo" para cadastrar
                </p>
              </div>
            </Card>
          )}
        </div>
      </div>
    </div>
  )
}