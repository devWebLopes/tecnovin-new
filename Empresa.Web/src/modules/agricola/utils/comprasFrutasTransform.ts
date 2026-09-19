import dayjs from 'dayjs';
import { GridDinamica, PainelData, EmpresaData, LinhaEmpresa, LinhaUf, ValoresDiarios } from '../types';

export function toNum(val: unknown): number | null {
  if (val == null || val === '') return null;
  const num = Number(val);
  return isNaN(num) ? null : num;
}

export function extractValores(linha: Record<string, unknown>): ValoresDiarios {
  const percentual =
    toNum(linha['% ATINGIDO']) ??
    toNum(linha['%_ATINGIDO']) ??
    toNum(linha['PERCENTUAL']) ??
    toNum(linha['% ATINGIMENTO']);

  return {
    qtde_d4: toNum(linha['QTDE_D4']),
    qtde_d3: toNum(linha['QTDE_D3']),
    qtde_d2: toNum(linha['QTDE_D2']),
    qtde_d1: toNum(linha['QTDE_D1']) ?? toNum(linha['DIA_ANTERIOR']),
    qtde_d0: toNum(linha['QTDE_D0']),
    acumulado: toNum(linha['ACUMULADO']),
    meta: toNum(linha['META']),
    percentual,
  };
}

export function extractColunasDatas(dataReferencia: string, colunas: string[]): string[] {
  const data = dayjs(dataReferencia);
  const datasFormatadas: string[] = [];

  // Se grid tem colunas explícitas
  if (colunas.includes('QTDE_D4')) datasFormatadas.push(data.subtract(4, 'day').format('DD/MM'));
  if (colunas.includes('QTDE_D3')) datasFormatadas.push(data.subtract(3, 'day').format('DD/MM'));
  if (colunas.includes('QTDE_D2')) datasFormatadas.push(data.subtract(2, 'day').format('DD/MM'));
  if (colunas.includes('QTDE_D1') || colunas.includes('DIA_ANTERIOR')) {
    const dow = data.day();
    const subDays = dow === 1 && colunas.includes('DIA_ANTERIOR') ? 3 : 1;
    datasFormatadas.push(data.subtract(subDays, 'day').format('DD/MM'));
  }
  if (colunas.includes('QTDE_D0')) datasFormatadas.push(data.format('DD/MM'));

  if (datasFormatadas.length === 0) {
    return [
      data.subtract(3, 'day').format('DD/MM'),
      data.subtract(2, 'day').format('DD/MM'),
      data.subtract(1, 'day').format('DD/MM'),
      data.format('DD/MM'),
    ];
  }

  return datasFormatadas;
}

export function transformarGridPlana(grid: GridDinamica | null | undefined): PainelData {
  if (!grid || !grid.linhas || grid.linhas.length === 0) {
    return {
      empresas: [],
      totalGeral: {
        qtde_d4: null,
        qtde_d3: null,
        qtde_d2: null,
        qtde_d1: null,
        qtde_d0: null,
        acumulado: null,
        meta: null,
        percentual: null,
      },
      colunasDatas: [],
      dataReferencia: grid?.dataReferencia || dayjs().format('YYYY-MM-DD'),
      ultimaAtualizacao: grid?.ultimaAtualizacao || '',
    };
  }

  const colunasDatas = extractColunasDatas(grid.dataReferencia, grid.colunas || []);

  let totalGeralRow = grid.linhas.find((l) => {
    const desc = String(l['DESCRICAO'] || '').toUpperCase();
    return desc.includes('TOTAL GERAL');
  });

  const empresasMap = new Map<string, { totalRow?: Record<string, unknown>; productRows: Record<string, unknown>[] }>();

  grid.linhas.forEach((linha) => {
    const desc = String(linha['DESCRICAO'] || '').toUpperCase();
    if (desc.includes('TOTAL GERAL')) {
      if (!totalGeralRow) totalGeralRow = linha;
      return;
    }

    const empresaName = String(linha['EMPRESA'] || 'Outras').trim();
    if (!empresasMap.has(empresaName)) {
      empresasMap.set(empresaName, { productRows: [] });
    }

    const empData = empresasMap.get(empresaName)!;
    if (desc.includes('TOTAL EMPRESA')) {
      empData.totalRow = linha;
    } else {
      empData.productRows.push(linha);
    }
  });

  const empresas: EmpresaData[] = [];

  empresasMap.forEach((empObj, empresaName) => {
    const cdEmpresaMatch = empresaName.match(/^(\d+)/);
    const cdEmpresa = cdEmpresaMatch ? cdEmpresaMatch[1] : '';

    const totalValores = empObj.totalRow ? extractValores(empObj.totalRow) : {
      qtde_d4: null, qtde_d3: null, qtde_d2: null, qtde_d1: null, qtde_d0: null, acumulado: null, meta: null, percentual: null,
    };

    // Agrupar linhas por CD_LINHA
    const linhasMap = new Map<string, { mainRow?: Record<string, unknown>; ufRows: Record<string, unknown>[] }>();

    empObj.productRows.forEach((row) => {
      const cdLinha = String(row['CD_LINHA'] ?? row['DESCRICAO'] ?? '').trim();
      if (!linhasMap.has(cdLinha)) {
        linhasMap.set(cdLinha, { ufRows: [] });
      }

      const lGroup = linhasMap.get(cdLinha)!;
      const uf = String(row['UF'] || '').trim();

      if (!uf) {
        lGroup.mainRow = row;
      } else {
        lGroup.ufRows.push(row);
      }
    });

    const listLinhasEmpresa: LinhaEmpresa[] = [];

    const LINHAS_NOMES_FALLBACK: Record<string, string> = {
      '23': 'GOIABA',
      '71': 'UVA',
      '72': 'MAÇÃ',
      '73': 'LARANJA',
      '74': 'BERGAMOTA',
      '78': 'PÊSSEGO',
      '79': 'MORANGO',
    };

    linhasMap.forEach((lGroup, cdLinha) => {
      const mainRow = lGroup.mainRow || (lGroup.ufRows.length > 0 ? lGroup.ufRows[0] : {});
      
      // Obter nome da fruta / linha
      let nomeFruta = '';
      const ufWithDesc = lGroup.ufRows.find(
        (u) => u['DESCRICAO'] && String(u['DESCRICAO']).trim().toUpperCase() !== 'TOTAL LINHA'
      );
      if (ufWithDesc) {
        nomeFruta = String(ufWithDesc['DESCRICAO']).trim();
      } else if (mainRow['DESCRICAO'] && String(mainRow['DESCRICAO']).trim().toUpperCase() !== 'TOTAL LINHA') {
        nomeFruta = String(mainRow['DESCRICAO']).trim();
      }

      const numCdLinha = cdLinha.includes('_') ? cdLinha.split('_')[1] : cdLinha;

      let descricao: string;
      if (nomeFruta) {
        descricao = nomeFruta.toUpperCase().startsWith('TOTAL ')
          ? nomeFruta.toUpperCase()
          : `TOTAL ${nomeFruta.toUpperCase()}`;
      } else {
        const rawDesc = String(mainRow['DESCRICAO'] || '').trim();
        if (rawDesc && rawDesc.toUpperCase() !== 'TOTAL LINHA') {
          descricao = rawDesc.toUpperCase().startsWith('TOTAL ')
            ? rawDesc.toUpperCase()
            : `TOTAL ${rawDesc.toUpperCase()}`;
        } else if (LINHAS_NOMES_FALLBACK[numCdLinha]) {
          descricao = `TOTAL ${LINHAS_NOMES_FALLBACK[numCdLinha]}`;
        } else {
          descricao = `TOTAL ${cdLinha}`;
        }
      }

      const safra = String(mainRow['SAFRA'] || (lGroup.ufRows.length > 0 ? lGroup.ufRows[0]['SAFRA'] || '' : ''));

      const ufs: LinhaUf[] = lGroup.ufRows.map((ufRow) => ({
        uf: String(ufRow['UF'] || ''),
        safra: String(ufRow['SAFRA'] || safra),
        valores: extractValores(ufRow),
        rawLinha: ufRow,
      }));

      // Se a linha principal não veio do BD, agregar das UFs
      let totais: ValoresDiarios;
      if (lGroup.mainRow) {
        totais = extractValores(lGroup.mainRow);
      } else {
        const sumD4 = ufs.reduce((acc, curr) => acc + (curr.valores.qtde_d4 || 0), 0);
        const sumD3 = ufs.reduce((acc, curr) => acc + (curr.valores.qtde_d3 || 0), 0);
        const sumD2 = ufs.reduce((acc, curr) => acc + (curr.valores.qtde_d2 || 0), 0);
        const sumD1 = ufs.reduce((acc, curr) => acc + (curr.valores.qtde_d1 || 0), 0);
        const sumD0 = ufs.reduce((acc, curr) => acc + (curr.valores.qtde_d0 || 0), 0);
        const sumAcum = ufs.reduce((acc, curr) => acc + (curr.valores.acumulado || 0), 0);
        const sumMeta = ufs.reduce((acc, curr) => acc + (curr.valores.meta || 0), 0);
        const pct = sumMeta > 0 ? (sumAcum / sumMeta) * 100 : null;

        totais = {
          qtde_d4: sumD4,
          qtde_d3: sumD3,
          qtde_d2: sumD2,
          qtde_d1: sumD1,
          qtde_d0: sumD0,
          acumulado: sumAcum,
          meta: sumMeta,
          percentual: pct,
        };
      }

      listLinhasEmpresa.push({
        cdLinha,
        descricao,
        safra,
        totais,
        ufs,
        rawLinha: mainRow,
      });
    });

    empresas.push({
      empresa: empresaName,
      cdEmpresa,
      linhas: listLinhasEmpresa,
      total: totalValores,
      rawLinha: empObj.totalRow || {},
    });
  });

  const totalGeralValores = totalGeralRow ? extractValores(totalGeralRow) : {
    qtde_d4: null, qtde_d3: null, qtde_d2: null, qtde_d1: null, qtde_d0: null, acumulado: null, meta: null, percentual: null,
  };

  return {
    empresas,
    totalGeral: {
      ...totalGeralValores,
      rawLinha: totalGeralRow,
    },
    colunasDatas,
    dataReferencia: grid.dataReferencia,
    ultimaAtualizacao: grid.ultimaAtualizacao,
  };
}
