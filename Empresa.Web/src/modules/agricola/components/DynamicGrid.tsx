import React from 'react';
import { Table } from 'antd';
import type { ColumnsType, TablePaginationConfig } from 'antd/es/table';
import {
  formatDiaCaption,
  formatNumber,
  getRowSemaphoro,
  isColunaVisivel,
  getColunaWidth,
  isColunaClicavel,
} from '@/modules/agricola/utils/comprasFrutasFormat';

interface DynamicGridProps {
  colunas: string[];
  linhas: Record<string, unknown>[];
  dataReferencia?: string;
  pagination?: false | TablePaginationConfig;
  onCellClick?: (coluna: string, linha: Record<string, unknown>) => void;
}

export const DynamicGrid: React.FC<DynamicGridProps> = ({
  colunas,
  linhas,
  dataReferencia = '',
  pagination = { pageSize: 100 },
  onCellClick,
}) => {
  const columns: ColumnsType<Record<string, unknown>> = colunas
    .filter(isColunaVisivel)
    .map((col) => {
      const title = dataReferencia && isColunaClicavel(col)
        ? formatDiaCaption(col, dataReferencia)
        : col.replace(/_/g, ' ').replace(/^CD /, 'CODIGO ');

      return {
        title,
        dataIndex: col,
        key: col,
        width: getColunaWidth(col),
        render: (value: unknown, record: Record<string, unknown>) => {
          const formatted = formatNumber(value, col);
          const isClickable = isColunaClicavel(col) && value != null && value !== '';
          return isClickable && onCellClick ? (
            <span
              style={{ color: '#1890ff', cursor: 'pointer', textDecoration: 'underline' }}
              onClick={() => onCellClick(col, record)}
            >
              {formatted}
            </span>
          ) : (
            <span>{formatted}</span>
          );
        },
      };
    });

  const rowClassName = (record: Record<string, unknown>) => {
    const descricao = String(record['DESCRICAO'] ?? '');
    const semaforo = getRowSemaphoro(descricao);
    if (!semaforo) return '';
    return semaforo.bold ? 'row-total-geral' : semaforo.bg === '#D3D3D3' ? 'row-total-empresa' : 'row-total-linha';
  };

  return (
    <Table
      columns={columns}
      dataSource={linhas.map((row, idx) => ({ ...row, key: idx }))}
      pagination={pagination}
      rowClassName={rowClassName}
      size="small"
      scroll={{ x: 'max-content', y: pagination ? undefined : 400 }}
    />
  );
};
