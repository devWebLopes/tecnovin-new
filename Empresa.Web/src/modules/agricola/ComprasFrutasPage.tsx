import React, { useEffect, useMemo, useState } from 'react';
import { DatePicker, Button, Select, Space, Spin, Alert, Empty, Typography } from 'antd';
import { ReloadOutlined, FileExcelOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { useComprasFrutas } from '@/modules/agricola/hooks/useComprasFrutas';
import { transformarGridPlana } from '@/modules/agricola/utils/comprasFrutasTransform';
import { exportarCsv } from '@/modules/agricola/utils/exportarCsv';
import { TotaisPanel } from '@/modules/agricola/components/TotaisPanel';
import { EmpresaSection } from '@/modules/agricola/components/EmpresaSection';
import { DetalhamentoModal } from '@/modules/agricola/components/DetalhamentoModal';
import { NotaFiscalModal } from '@/modules/agricola/components/NotaFiscalModal';
import type { ColunaClicada, DetalhamentoParams, NotasFiscaisParams } from '@/modules/agricola/types';

const { Title, Text } = Typography;

export const ComprasFrutasPage: React.FC = () => {
  const { data, loading, error, loadData, refreshInterval, setRefreshInterval, stopAutoRefresh } =
    useComprasFrutas();
  const [selectedDate, setSelectedDate] = useState(dayjs());
  const [detalhamentoOpen, setDetalhamentoOpen] = useState(false);
  const [detalhamentoParams, setDetalhamentoParams] = useState<DetalhamentoParams | null>(null);
  const [nfModalOpen, setNfModalOpen] = useState(false);
  const [nfParams, setNfParams] = useState<NotasFiscaisParams | null>(null);

  useEffect(() => {
    loadData(selectedDate.format('YYYY-MM-DD'));
    return () => stopAutoRefresh();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const painelData = useMemo(() => transformarGridPlana(data), [data]);

  const handleDateChange = (date: dayjs.Dayjs | null) => {
    if (date) {
      setSelectedDate(date);
      loadData(date.format('YYYY-MM-DD'));
    }
  };

  const handleReload = () => {
    loadData(selectedDate.format('YYYY-MM-DD'));
  };

  const handleCellClick = (empresa: string, linha: string, uf: string, colunaClicada: ColunaClicada) => {
    const baseParams = {
      data: selectedDate.format('YYYY-MM-DD'),
      empresa,
      linha,
      uf,
      colunaClicada,
    };

    // Se é linha de Total Empresa ou Total Geral (linha == '79' ou '99')
    if (linha === '79' || linha === '99') {
      setNfParams(baseParams);
      setNfModalOpen(true);
    } else {
      setDetalhamentoParams(baseParams);
      setDetalhamentoOpen(true);
    }
  };

  const handleGlobalExportCsv = () => {
    if (!data) return;
    exportarCsv(data.colunas, data.linhas, 'compras_frutas_consolidado.csv');
  };

  return (
    <div style={{ padding: '16px 24px' }}>
      {/* Cabeçalho */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <Title level={4} style={{ margin: 0 }}>
          🍇 Relatório de Compra de Frutas (v2)
        </Title>
        {data && (
          <Text type="secondary" style={{ fontSize: 12 }}>
            Última atualização: {dayjs(data.ultimaAtualizacao).format('DD/MM/YYYY HH:mm:ss')}
          </Text>
        )}
      </div>

      {/* Barra de filtros */}
      <Space wrap style={{ marginBottom: 16 }}>
        <DatePicker
          value={selectedDate}
          onChange={handleDateChange}
          format="DD/MM/YYYY"
          allowClear={false}
        />
        <Button icon={<ReloadOutlined />} onClick={handleReload} loading={loading}>
          Recarregar
        </Button>
        <Select
          value={refreshInterval}
          onChange={setRefreshInterval}
          style={{ width: 150 }}
          options={[
            { value: 0, label: 'Manual' },
            { value: 1800000, label: '30 Minutos' },
            { value: 3600000, label: '01 Hora' },
          ]}
        />
        {data && (
          <Button
            icon={<FileExcelOutlined />}
            onClick={handleGlobalExportCsv}
            style={{ color: '#52c41a', borderColor: '#52c41a' }}
          >
            Exportar CSV Global
          </Button>
        )}
      </Space>

      {/* Estados: erro / loading / vazio / dados */}
      {error && (
        <Alert
          type="error"
          message="Erro ao carregar dados"
          description={error}
          showIcon
          style={{ marginBottom: 16 }}
          action={
            <Button size="small" onClick={handleReload}>
              Tentar novamente
            </Button>
          }
        />
      )}

      <Spin spinning={loading} tip="Carregando dados do painel...">
        {!loading && !error && (!data || painelData.empresas.length === 0) && (
          <Empty description="Nenhum dado disponível para a data selecionada." />
        )}

        {data && painelData.empresas.length > 0 && (
          <>
            {/* Seção 1 — Totais Consolidados (Sticky) */}
            <TotaisPanel
              empresas={painelData.empresas}
              totalGeral={painelData.totalGeral}
              colunasDatas={painelData.colunasDatas}
              onCellClick={handleCellClick}
            />

            {/* Seção 2 — Seções por Empresa */}
            {painelData.empresas.map((emp) => (
              <EmpresaSection
                key={emp.empresa}
                empresa={emp}
                colunasDatas={painelData.colunasDatas}
                onCellClick={handleCellClick}
              />
            ))}
          </>
        )}
      </Spin>

      {/* Modais de drill-down */}
      <DetalhamentoModal
        open={detalhamentoOpen}
        params={detalhamentoParams}
        onClose={() => {
          setDetalhamentoOpen(false);
          setDetalhamentoParams(null);
        }}
      />
      {nfParams && (
        <NotaFiscalModal
          open={nfModalOpen}
          params={nfParams}
          onClose={() => {
            setNfModalOpen(false);
            setNfParams(null);
          }}
        />
      )}
    </div>
  );
};

export default ComprasFrutasPage;
