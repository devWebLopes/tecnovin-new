import React, { useEffect, useState } from 'react';
import { Modal, Spin } from 'antd';
import { FileExcelOutlined } from '@ant-design/icons';
import * as service from '@/modules/agricola/services/comprasFrutasService';
import type { GridDinamica, DetalhamentoParams } from '@/modules/agricola/types';
import {
  isColunaClicavelDetalhamento,
  getDetalhamentoRowStyle,
  formatNumberDetalhamento,
} from '@/modules/agricola/utils/comprasFrutasFormat';
import { exportarCsv } from '@/modules/agricola/utils/exportarCsv';
import { NotaFiscalModal } from '@/modules/agricola/components/NotaFiscalModal';

interface DetalhamentoModalProps {
  open: boolean;
  params: DetalhamentoParams | null;
  onClose: () => void;
}

export const DetalhamentoModal: React.FC<DetalhamentoModalProps> = ({ open, params, onClose }) => {
  const [data, setData] = useState<GridDinamica | null>(null);
  const [loading, setLoading] = useState(false);
  const [nfModalOpen, setNfModalOpen] = useState(false);
  const [nfParams, setNfParams] = useState<{ data: string; empresa: string; linha: string; uf: string; colunaClicada: string; colunaGrauClicada?: string; variedade?: string } | null>(null);

  useEffect(() => {
    if (open && params) {
      setLoading(true);
      service
        .getDetalhamento(params)
        .then((res: GridDinamica) => setData(res))
        .catch(() => setData(null))
        .finally(() => setLoading(false));
    }
  }, [open, params]);

  const handleCellClick = (coluna: string, linha: Record<string, unknown>) => {
    if (!params || !isColunaClicavelDetalhamento(coluna, linha[coluna])) return;
    const variedade = String(linha['VARIEDADE'] ?? '');
    setNfParams({
      data: params.data,
      empresa: params.empresa,
      linha: params.linha,
      uf: params.uf,
      // p_coluna_clicada = coluna do NÍVEL 0 (que abriu o detalhamento),
      // conforme Regra 17 do legado e RF12.1 do PRD.
      colunaClicada: params.colunaClicada,
      // p_coluna_grau_clicada = caption da coluna clicada no nível 1.
      colunaGrauClicada: coluna,
      variedade,
    });
    setNfModalOpen(true);
  };

  const handleExport = () => {
    if (!data) return;
    exportarCsv(data.colunas, data.linhas as Record<string, unknown>[], 'Compras Frutas.csv');
  };

  const contextTitle = params
    ? `${params.data} | ${params.colunaClicada} | ${params.empresa} ${params.uf ? `| UF: ${params.uf}` : ''}`
    : '';

  return (
    <>
      <Modal
        title={`Detalhamento — ${contextTitle}`}
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
                {data.linhas.map((linha: Record<string, unknown>, idx: number) => {
                  const variedade = String(linha['VARIEDADE'] ?? '');
                  const style = getDetalhamentoRowStyle(variedade);
                  return (
                    <tr key={idx} style={style ? { backgroundColor: style.bg, color: style.color, fontWeight: style.bold ? 'bold' : 'normal' } : {}}>
                      {data.colunas.map((col: string) => {
                        const val = linha[col];
                        const clickable = isColunaClicavelDetalhamento(col, val);
                        return (
                          <td
                            key={col}
                            style={{ border: '1px solid #d9d9d9', padding: '4px 8px', fontSize: 12, ...(clickable ? { color: '#1890ff', cursor: 'pointer', textDecoration: 'underline' } : {}) }}
                            onClick={() => clickable && handleCellClick(col, linha)}
                          >
                            {formatNumberDetalhamento(val, col)}
                          </td>
                        );
                      })}
                    </tr>
                  );
                })}
              </tbody>
            </table>
          )}
        </Spin>
      </Modal>
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
    </>
  );
};
