import React, { useEffect, useState } from 'react';
import { Modal, Spin } from 'antd';
import { FileExcelOutlined } from '@ant-design/icons';
import * as service from '@/modules/agricola/services/comprasFrutasService';
import type { GridDinamica } from '@/modules/agricola/types';
import { formatNumberNotasFiscais } from '@/modules/agricola/utils/comprasFrutasFormat';
import { exportarCsv } from '@/modules/agricola/utils/exportarCsv';

interface NotaFiscalModalProps {
  open: boolean;
  params: { data: string; empresa: string; linha: string; uf: string; colunaClicada: string; colunaGrauClicada?: string; variedade?: string } | null;
  onClose: () => void;
}

export const NotaFiscalModal: React.FC<NotaFiscalModalProps> = ({ open, params, onClose }) => {
  const [data, setData] = useState<GridDinamica | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (open && params) {
      setLoading(true);
      service
        .getNotasFiscais({
          data: params.data,
          empresa: params.empresa,
          linha: params.linha,
          uf: params.uf,
          colunaClicada: params.colunaClicada as 'ANTERIOR' | 'QTDE_D0' | 'ACUMULADO',
          colunaGrauClicada: params.colunaGrauClicada,
          variedade: params.variedade,
        })
        .then((res: GridDinamica) => setData(res))
        .catch(() => setData(null))
        .finally(() => setLoading(false));
    }
  }, [open, params]);

  const handleExport = () => {
    if (!data) return;
    exportarCsv(data.colunas, data.linhas as Record<string, unknown>[], 'DetalhamentoNotaFiscal.csv');
  };

  return (
    <Modal
      title="Notas Fiscais"
      open={open}
      onCancel={onClose}
      footer={null}
      width="90%"
      styles={{ body: { maxHeight: '70vh', overflow: 'auto' } }}
    >
      <div style={{ marginBottom: 8, textAlign: 'right' }}>
        <FileExcelOutlined onClick={handleExport} style={{ fontSize: 20, cursor: 'pointer', color: '#52c41a' }} />
      </div>
      <Spin spinning={loading}>
        {data && (
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr>
                {data.colunas.map((col: string) => (
                  <th key={col} style={{ border: '1px solid #d9d9d9', padding: '4px 8px', fontSize: 12 }}>
                    {col.replace(/_/g, ' ').replace(/^CD /, 'CODIGO ')}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {data.linhas.map((linha: Record<string, unknown>, idx: number) => (
                <tr key={idx}>
                  {data.colunas.map((col: string) => (
                    <td key={col} style={{ border: '1px solid #d9d9d9', padding: '4px 8px', fontSize: 12 }}>
                      {formatNumberNotasFiscais(linha[col], col)}
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </Spin>
    </Modal>
  );
};
