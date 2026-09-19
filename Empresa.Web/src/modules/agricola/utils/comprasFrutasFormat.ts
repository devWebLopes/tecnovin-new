import dayjs from 'dayjs';

export function formatCaption(coluna: string): string {
  let caption = coluna.replace(/_/g, ' ');
  caption = caption.replace(/^CD /, 'CODIGO ');
  return caption;
}

export function formatDiaCaption(coluna: string, dataRef: string): string {
  const data = dayjs(dataRef);
  const dow = data.day();

  switch (coluna) {
    case 'QTDE_D0':
      return data.format('DD/MM/YYYY');
    case 'QTDE_D1':
      return data.subtract(1, 'day').format('DD/MM/YYYY');
    case 'QTDE_D2':
      return data.subtract(2, 'day').format('DD/MM/YYYY');
    case 'QTDE_D3':
      return data.subtract(3, 'day').format('DD/MM/YYYY');
    case 'QTDE_D4':
      return data.subtract(4, 'day').format('DD/MM/YYYY');
    case 'DIA_ANTERIOR':
      return dow === 1
        ? data.subtract(3, 'day').format('DD/MM/YYYY')
        : data.subtract(1, 'day').format('DD/MM/YYYY');
    default:
      return formatCaption(coluna);
  }
}

export function formatNumber(value: unknown, coluna: string): string {
  if (value == null || value === '') return String(value ?? '');
  const num = Number(value);
  if (isNaN(num)) return String(value);

  if (coluna.includes('%')) {
    return num.toLocaleString('pt-BR', { minimumFractionDigits: 1, maximumFractionDigits: 1 });
  }
  if (['QTDE_D0', 'QTDE_D1', 'QTDE_D2', 'QTDE_D3', 'QTDE_D4', 'ACUMULADO', 'DIA_ANTERIOR'].includes(coluna)) {
    return num.toLocaleString('pt-BR', { maximumFractionDigits: 0 });
  }
  return num.toLocaleString('pt-BR', { maximumFractionDigits: 0 });
}

export function formatNumberDetalhamento(value: unknown, coluna: string): string {
  if (value == null || value === '') return String(value ?? '');
  const num = Number(value);
  if (isNaN(num)) return String(value);

  const upperCol = coluna.toUpperCase();
  if (upperCol.includes('MEDIO') || upperCol.includes('FATURADO')) {
    return num.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }
  if (upperCol.includes('UNITARIO')) {
    return num.toLocaleString('pt-BR', { minimumFractionDigits: 3, maximumFractionDigits: 3 });
  }
  return num.toLocaleString('pt-BR', { maximumFractionDigits: 0 });
}

export function formatNumberNotasFiscais(value: unknown, coluna: string): string {
  if (value == null || value === '') return String(value ?? '');
  const num = Number(value);
  const upperCol = coluna.toUpperCase();

  if (upperCol === 'NR_NOTAFISCAL') return String(value);
  if (upperCol.includes('DT')) {
    try {
      return dayjs(value as string | number | Date).format('DD/MM/YYYY');
    } catch {
      return String(value);
    }
  }
  if (isNaN(num)) return String(value);

  if (upperCol.includes('MEDIO') || upperCol.includes('FATURADO')) {
    return num.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }
  if (upperCol.includes('UNITARIO')) {
    return num.toLocaleString('pt-BR', { minimumFractionDigits: 3, maximumFractionDigits: 3 });
  }
  return num.toLocaleString('pt-BR', { maximumFractionDigits: 0 });
}

export function getRowSemaphoro(descricao: string): { bg: string; color?: string; bold?: boolean } | null {
  const desc = descricao.toUpperCase();
  if (desc.includes('GERAL')) return { bg: '#77889A', color: '#FFFFFF', bold: true };
  if (desc.includes('EMPRESA')) return { bg: '#D3D3D3' };
  if (desc.includes('LINHA') || desc.startsWith('TOTAL')) return { bg: '#EEE9E9' };
  return null;
}

export function getDetalhamentoRowStyle(variedade: string): { bg: string; color: string; bold: boolean } | null {
  if (variedade.toUpperCase().includes('GERAL')) {
    return { bg: '#77889A', color: '#FFFFFF', bold: true };
  }
  return null;
}

export function isLinhaTotal(descricao: string): boolean {
  const desc = descricao.toUpperCase();
  return desc.includes('EMPRESA') || desc.includes('GERAL');
}

export function isColunaClicavel(coluna: string): boolean {
  return ['DIA_ANTERIOR', 'QTDE_D0', 'ACUMULADO'].includes(coluna);
}

export function isColunaClicavelDetalhamento(coluna: string, valor: unknown): boolean {
  if (valor == null || valor === '') return false;
  const upper = coluna.toUpperCase();
  if (upper.includes('MEDIO') || upper.includes('VARIEDADE')) return false;
  return true;
}

export function getColunaWidth(coluna: string): number | undefined {
  if (coluna === 'VARIEDADE') return 220;
  if (coluna.includes('%')) return 80;
  return undefined;
}

export function isColunaVisivel(coluna: string): boolean {
  return coluna !== 'EM_SAFRA';
}

export interface SemaforoInfo {
  bg: string;
  color: string;
  icon: string;
  status: 'success' | 'warning' | 'error' | 'default';
}

export function getSemaforoCor(percentual: number | null | undefined): SemaforoInfo {
  if (percentual == null || isNaN(percentual)) {
    return { bg: '#8c8c8c', color: '#ffffff', icon: '—', status: 'default' };
  }

  if (percentual >= 100) {
    return { bg: '#52c41a', color: '#ffffff', icon: '✅', status: 'success' };
  }

  if (percentual >= 70) {
    return { bg: '#faad14', color: '#000000', icon: '🟡', status: 'warning' };
  }

  return { bg: '#ff4d4f', color: '#ffffff', icon: '🔴', status: 'error' };
}

export function formatPercent(value: number | null | undefined): string {
  if (value == null || isNaN(value)) return '—';
  return `${value.toLocaleString('pt-BR', { minimumFractionDigits: 1, maximumFractionDigits: 1 })}%`;
}

