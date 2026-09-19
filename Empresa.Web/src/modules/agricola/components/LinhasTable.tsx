import React from 'react';
import { Table, Tag } from 'antd';
import { LinhaEmpresa, LinhaUf, ValoresDiarios, ColunaClicada } from '../types';
import { PercentBadge } from './PercentBadge';
import { formatNumber } from '../utils/comprasFrutasFormat';

interface LinhasTableProps {
  empresa: string;
  linhas: LinhaEmpresa[];
  colunasDatas: string[];
  onCellClick: (empresa: string, linha: string, uf: string, colunaClicada: ColunaClicada) => void;
}

export const LinhasTable: React.FC<LinhasTableProps> = ({
  empresa,
  linhas,
  colunasDatas,
  onCellClick,
}) => {
  const dateKeys: (keyof ValoresDiarios)[] = ['qtde_d3', 'qtde_d2', 'qtde_d1', 'qtde_d0'];
  if (colunasDatas.length === 5) {
    dateKeys.unshift('qtde_d4');
  }

  const columns = [
    {
      title: 'CÓD. LINHA',
      dataIndex: 'cdLinha',
      key: 'cdLinha',
      width: 110,
    },
    {
      title: 'DESCRIÇÃO',
      dataIndex: 'descricao',
      key: 'descricao',
      render: (text: string) => <strong>{text}</strong>,
    },
    {
      title: 'SAFRA',
      dataIndex: 'safra',
      key: 'safra',
      width: 90,
      render: (safra: string) => (safra ? <Tag color="blue">{safra}</Tag> : '—'),
    },
    ...colunasDatas.map((dataLabel, index) => {
      const keyName = dateKeys[index] || dateKeys[dateKeys.length - 1];
      const colEnumKey: ColunaClicada = index === dateKeys.length - 1 ? 'QTDE_D0' : (`QTDE_D${dateKeys.length - 1 - index}` as ColunaClicada);

      return {
        title: dataLabel,
        key: `date_${index}`,
        align: 'right' as const,
        render: (_: unknown, record: LinhaEmpresa) => {
          const val = record.totais[keyName];
          return (
            <span
              style={{ cursor: 'pointer', color: '#1890ff', textDecoration: 'underline' }}
              onClick={() => onCellClick(empresa, record.cdLinha, '', colEnumKey)}
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
      render: (_: unknown, record: LinhaEmpresa) => (
        <span
          style={{ cursor: 'pointer', color: '#1890ff', fontWeight: 'bold', textDecoration: 'underline' }}
          onClick={() => onCellClick(empresa, record.cdLinha, '', 'ACUMULADO')}
        >
          {formatNumber(record.totais.acumulado, 'ACUMULADO')}
        </span>
      ),
    },
    {
      title: 'META',
      key: 'meta',
      align: 'right' as const,
      render: (_: unknown, record: LinhaEmpresa) => formatNumber(record.totais.meta, 'META'),
    },
    {
      title: '% ATINGIDO',
      key: 'percentual',
      align: 'center' as const,
      render: (_: unknown, record: LinhaEmpresa) => <PercentBadge percentual={record.totais.percentual} />,
    },
  ];

  // Renderizar sub-tabela por UF
  const expandedRowRender = (record: LinhaEmpresa) => {
    if (!record.ufs || record.ufs.length === 0) {
      return <div style={{ padding: '8px 16px', color: '#8c8c8c', fontStyle: 'italic' }}>Sem detalhamento por UF</div>;
    }

    const ufColumns = [
      {
        title: 'UF',
        dataIndex: 'uf',
        key: 'uf',
        render: (uf: string) => <Tag color="green">└── {uf}</Tag>,
      },
      {
        title: 'SAFRA',
        dataIndex: 'safra',
        key: 'safra',
        render: (safra: string) => safra || record.safra || '—',
      },
      ...colunasDatas.map((dataLabel, index) => {
        const keyName = dateKeys[index] || dateKeys[dateKeys.length - 1];
        const colEnumKey: ColunaClicada = index === dateKeys.length - 1 ? 'QTDE_D0' : (`QTDE_D${dateKeys.length - 1 - index}` as ColunaClicada);

        return {
          title: dataLabel,
          key: `uf_date_${index}`,
          align: 'right' as const,
          render: (_: unknown, ufRecord: LinhaUf) => {
            const val = ufRecord.valores[keyName];
            return (
              <span
                style={{ cursor: 'pointer', color: '#1890ff', textDecoration: 'underline' }}
                onClick={() => onCellClick(empresa, record.cdLinha, ufRecord.uf, colEnumKey)}
              >
                {formatNumber(val, 'QTDE_D0')}
              </span>
            );
          },
        };
      }),
      {
        title: 'ACUMULADO',
        key: 'uf_acumulado',
        align: 'right' as const,
        render: (_: unknown, ufRecord: LinhaUf) => (
          <span
            style={{ cursor: 'pointer', color: '#1890ff', fontWeight: 'bold', textDecoration: 'underline' }}
            onClick={() => onCellClick(empresa, record.cdLinha, ufRecord.uf, 'ACUMULADO')}
          >
            {formatNumber(ufRecord.valores.acumulado, 'ACUMULADO')}
          </span>
        ),
      },
      {
        title: 'META',
        key: 'uf_meta',
        align: 'right' as const,
        render: (_: unknown, ufRecord: LinhaUf) => formatNumber(ufRecord.valores.meta, 'META'),
      },
      {
        title: '% ATINGIDO',
        key: 'uf_percentual',
        align: 'center' as const,
        render: (_: unknown, ufRecord: LinhaUf) => <PercentBadge percentual={ufRecord.valores.percentual} />,
      },
    ];

    return (
      <Table
        columns={ufColumns}
        dataSource={record.ufs.map((uf, idx) => ({ ...uf, key: `${record.cdLinha}_${uf.uf}_${idx}` }))}
        pagination={false}
        size="small"
        showHeader={false}
        style={{ backgroundColor: '#fafafa', marginLeft: '24px' }}
      />
    );
  };

  return (
    <Table
      columns={columns}
      dataSource={linhas.map((l) => ({ ...l, key: l.cdLinha }))}
      expandable={{
        expandedRowRender,
        rowExpandable: (record) => record.ufs && record.ufs.length > 0,
      }}
      pagination={false}
      size="small"
    />
  );
};
