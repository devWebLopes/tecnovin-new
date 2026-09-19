import { Card, Row, Col, Statistic, Typography, Space } from 'antd'
import {
  ShoppingCartOutlined,
  DollarOutlined,
  LineChartOutlined,
  ClockCircleOutlined,
} from '@ant-design/icons'

const { Title, Text } = Typography

/**
 * Dashboard principal — ponto de entrada após login.
 * Exibe cards de acesso rápido aos módulos.
 */
export default function DashboardPage() {
  const cards = [
    {
      title: 'Compras',
      icon: <ShoppingCartOutlined style={{ fontSize: 32, color: '#1a3c5e' }} />,
      description: 'Resumo anual, comitê e análise de preços',
      color: '#eff6ff',
    },
    {
      title: 'Financeiro',
      icon: <DollarOutlined style={{ fontSize: 32, color: '#0d9488' }} />,
      description: 'Posição financeira, DRE e fluxo de caixa',
      color: '#f0fdf9',
    },
    {
      title: 'Vendas',
      icon: <LineChartOutlined style={{ fontSize: 32, color: '#7c3aed' }} />,
      description: 'Análise de vendas e ranking de clientes',
      color: '#faf5ff',
    },
    {
      title: 'Prazo Médio',
      icon: <ClockCircleOutlined style={{ fontSize: 32, color: '#d97706' }} />,
      description: 'Recebimento e pagamento com drill-down',
      color: '#fffbeb',
    },
  ]

  return (
    <Space direction="vertical" size={24} style={{ width: '100%' }}>
      {/* Boas-vindas */}
      <div>
        <Title level={4} style={{ marginBottom: 4, color: '#1f2937' }}>
          Bem-vindo ao GestaoNew
        </Title>
        <Text type="secondary">Selecione um módulo no menu lateral ou nos atalhos abaixo.</Text>
      </div>

      {/* Cards de módulos */}
      <Row gutter={[16, 16]}>
        {cards.map((card) => (
          <Col key={card.title} xs={24} sm={12} lg={6}>
            <Card
              hoverable
              style={{
                borderRadius: 12,
                background: card.color,
                border: '1px solid #e5e7eb',
                cursor: 'pointer',
              }}
            >
              <Space direction="vertical" size={8}>
                {card.icon}
                <Statistic
                  title={<Text strong style={{ fontSize: 14, color: '#374151' }}>{card.title}</Text>}
                  value={' '}
                  suffix={null}
                  style={{ marginBottom: 0 }}
                />
                <Text type="secondary" style={{ fontSize: 12 }}>
                  {card.description}
                </Text>
              </Space>
            </Card>
          </Col>
        ))}
      </Row>
    </Space>
  )
}
