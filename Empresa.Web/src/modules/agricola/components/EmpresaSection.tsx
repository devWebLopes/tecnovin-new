import React, { useState } from 'react';
import { Card, Button, Space, Tag } from 'antd';
import { DownloadOutlined, DownOutlined, RightOutlined } from '@ant-design/icons';
import { EmpresaData, ColunaClicada } from '../types';
import { LinhasTable } from './LinhasTable';
import { PercentBadge } from './PercentBadge';
import { exportarCsv } from '../utils/exportarCsv';

interface EmpresaSectionProps {
  empresa: EmpresaData;
  colunasDatas: string[];
  onCellClick: (empresa: string, linha: string, uf: string, colunaClicada: ColunaClicada) => void;
}

export const EmpresaSection: React.FC<EmpresaSectionProps> = ({
  empresa,
  colunasDatas,
  onCellClick,
}) => {
  // ABERTA por padrão (conforme PRD §10 Questão 3)
  const [collapsed, setCollapsed] = useState(false);

  const handleExportCsv = (e: React.MouseEvent) => {
    e.stopPropagation();

    // Coletar linhas brutas da empresa
    const rawRows: Record<string, unknown>[] = [];
    if (empresa.rawLinha && Object.keys(empresa.rawLinha).length > 0) {
      rawRows.push(empresa.rawLinha);
    }
    empresa.linhas.forEach((l) => {
      if (l.rawLinha && Object.keys(l.rawLinha).length > 0) {
        rawRows.push(l.rawLinha);
      }
      l.ufs.forEach((ufRow) => {
        if (ufRow.rawLinha && Object.keys(ufRow.rawLinha).length > 0) {
          rawRows.push(ufRow.rawLinha);
        }
      });
    });

    if (rawRows.length === 0) return;

    const colunas = Object.keys(rawRows[0]);
    const empCleanName = empresa.empresa.replace(/[^a-zA-Z0-9]/g, '_');
    exportarCsv(colunas, rawRows, `compras_frutas_${empCleanName}.csv`);
  };

  return (
    <Card
      style={{ marginBottom: '16px', borderRadius: '8px', overflow: 'hidden' }}
      title={
        <div
          onClick={() => setCollapsed(!collapsed)}
          style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}
        >
          <Space align="center" size="middle">
            {collapsed ? <RightOutlined style={{ color: '#ffffff' }} /> : <DownOutlined style={{ color: '#ffffff' }} />}
            <span style={{ fontSize: '16px', fontWeight: 'bold', color: '#ffffff' }}>🏢 {empresa.empresa}</span>
            <Tag color="cyan">{empresa.linhas.length} linha(s)</Tag>
            <PercentBadge percentual={empresa.total.percentual} />
          </Space>

          <Button
            size="small"
            type="primary"
            icon={<DownloadOutlined />}
            onClick={handleExportCsv}
            style={{ backgroundColor: '#1890ff', borderColor: '#1890ff' }}
          >
            Exportar CSV
          </Button>
        </div>
      }
      styles={{
        header: {
          backgroundColor: '#2d5a9e',
          color: '#ffffff',
          cursor: 'pointer',
        },
        body: { padding: collapsed ? 0 : '12px', display: collapsed ? 'none' : 'block' },
      }}
    >
      <LinhasTable
        empresa={empresa.empresa}
        linhas={empresa.linhas}
        colunasDatas={colunasDatas}
        onCellClick={onCellClick}
      />
    </Card>
  );
};
