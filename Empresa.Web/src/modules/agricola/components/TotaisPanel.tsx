import React from 'react';
import { Card, Table } from 'antd';
import { EmpresaData, ValoresDiarios, ColunaClicada } from '../types';
import { PercentBadge } from './PercentBadge';
import { formatNumber } from '../utils/comprasFrutasFormat';

interface TotaisPanelProps {
  empresas: EmpresaData[];
  totalGeral: ValoresDiarios;
  colunasDatas: string[];
  onCellClick: (empresa: string, linha: string, uf: string, colunaClicada: ColunaClicada) => void;
}

interface TotaisRow {
  key: string;
  empresa: string;
  codigoLinha: string;
  descricao: string;
  valores: ValoresDiarios;
  isTotalGeral: boolean;
}

export const TotaisPanel: React.FC<TotaisPanelProps> = ({
  empresas,
  totalGeral,
  colunasDatas,
  onCellClick,
}) => {
  const dataSource: TotaisRow[] = [
    ...empresas.map((emp) => ({
      key: emp.empresa,
      empresa: emp.empresa,
      codigoLinha: '79',
      descricao: 'TOTAL EMPRESA',
      valores: emp.total,
      isTotalGeral: false,
    })),
    {
      key: 'TOTAL_GERAL',
      empresa: 'TOTAL GERAL',
      codigoLinha: '99',
      descricao: 'TOTAL GERAL',
      valores: totalGeral,
      isTotalGeral: true,
    },
  ];

  // Identificar quais propriedades numéricas correspondem a quais colunasDatas
  const dateKeys: (keyof ValoresDiarios)[] = ['qtde_d3', 'qtde_d2', 'qtde_d1', 'qtde_d0'];
  if (colunasDatas.length === 5) {
    dateKeys.unshift('qtde_d4');
  }

  const columns = [
    {
      title: 'EMPRESA',
      dataIndex: 'empresa',
      key: 'empresa',
      render: (text: string, record: TotaisRow) => (
        <strong style={{ color: record.isTotalGeral ? '#1890ff' : 'inherit' }}>{text}</strong>
      ),
    },
    {
      title: 'DESCRIÇÃO',
      dataIndex: 'descricao',
      key: 'descricao',
    },
    ...colunasDatas.map((dataLabel, index) => {
      const keyName = dateKeys[index] || dateKeys[dateKeys.length - 1];
      const colEnumKey: ColunaClicada = index === dateKeys.length - 1 ? 'QTDE_D0' : (`QTDE_D${dateKeys.length - 1 - index}` as ColunaClicada);

      return {
        title: dataLabel,
        key: `date_${index}`,
        align: 'right' as const,
        render: (_: unknown, record: TotaisRow) => {
          const val = record.valores[keyName];
          return (
            <span
              style={{ cursor: 'pointer', color: '#1890ff', textDecoration: 'underline' }}
              onClick={() => onCellClick(record.empresa, record.codigoLinha, '', colEnumKey)}
            >
              {formatNumber(val, 'QTDE_D0')}
            </span>
          );
        },
      };
    }),
    {
      title: 'ACUMULADO',
      key: 'acumulado',
      align: 'right' as const,
      render: (_: unknown, record: TotaisRow) => (
        <span
          style={{ cursor: 'pointer', color: '#1890ff', fontWeight: 'bold', textDecoration: 'underline' }}
          onClick={() => onCellClick(record.empresa, record.codigoLinha, '', 'ACUMULADO')}
        >
          {formatNumber(record.valores.acumulado, 'ACUMULADO')}
        </span>
      ),
    },
    {
      title: 'META',
      key: 'meta',
      align: 'right' as const,
      render: (_: unknown, record: TotaisRow) => formatNumber(record.valores.meta, 'META'),
    },
    {
      title: '% ATINGIDO',
      key: 'percentual',
      align: 'center' as const,
      render: (_: unknown, record: TotaisRow) => <PercentBadge percentual={record.valores.percentual} />,
    },
  ];

  return (
    <Card
      title={<span style={{ color: '#ffffff', fontWeight: 'bold' }}>📊 TOTAIS CONSOLIDADOS</span>}
      style={{
        marginBottom: '16px',
        position: 'sticky',
        top: 0,
        zIndex: 10,
        boxShadow: '0 4px 12px rgba(0,0,0,0.08)',
      }}
      styles={{
        header: {
          backgroundColor: '#1a3a5c',
          borderBottom: 'none',
          minHeight: '44px',
        },
        body: { padding: '0px' },
      }}
    >
      <Table
        columns={columns}
        dataSource={dataSource}
        pagination={false}
        size="small"
        rowClassName={(record) => (record.isTotalGeral ? 'table-row-total-geral' : '')}
        style={{
          backgroundColor: '#ffffff',
        }}
      />
    </Card>
  );
};
