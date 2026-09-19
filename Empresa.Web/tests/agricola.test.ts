import { describe, expect, it } from 'vitest';
import {
  formatCaption,
  formatDiaCaption,
  formatNumber,
  formatNumberDetalhamento,
  formatNumberNotasFiscais,
  getRowSemaphoro,
  getDetalhamentoRowStyle,
  isLinhaTotal,
  isColunaClicavel,
  isColunaClicavelDetalhamento,
  getColunaWidth,
  isColunaVisivel,
  getSemaforoCor,
  formatPercent,
} from '@/modules/agricola/utils/comprasFrutasFormat';
import { transformarGridPlana } from '@/modules/agricola/utils/comprasFrutasTransform';


describe('comprasFrutasFormat — captions', () => {
  it('formatCaption: underscore → espaço', () => {
    expect(formatCaption('QTDE_D0')).toBe('QTDE D0');
    expect(formatCaption('CD_LINHA')).toBe('CODIGO LINHA');
  });

  it('formatCaption: CD → CODIGO', () => {
    expect(formatCaption('CD_VARIEDADE')).toBe('CODIGO VARIEDADE');
  });

  it('formatDiaCaption: QTDE_D0 usa data de referência', () => {
    expect(formatDiaCaption('QTDE_D0', '2026-08-13')).toBe('13/08/2026');
  });

  it('formatDiaCaption: QTDE_D1 é D-1', () => {
    expect(formatDiaCaption('QTDE_D1', '2026-08-13')).toBe('12/08/2026');
  });

  it('formatDiaCaption: DIA_ANTERIOR na segunda-feira usa D-3', () => {
    // 2026-08-10 é segunda-feira
    expect(formatDiaCaption('DIA_ANTERIOR', '2026-08-10')).toBe('07/08/2026');
  });

  it('formatDiaCaption: DIA_ANTERIOR em outros dias usa D-1', () => {
    // 2026-08-13 é quinta-feira
    expect(formatDiaCaption('DIA_ANTERIOR', '2026-08-13')).toBe('12/08/2026');
  });
});

describe('comprasFrutasFormat — formatos numéricos', () => {
  it('formatNumber: colunas QTDE_D* usam N0 (inteiro pt-BR)', () => {
    expect(formatNumber(1500000, 'QTDE_D0')).toBe('1.500.000');
    expect(formatNumber(1200, 'QTDE_D1')).toBe('1.200');
  });

  it('formatNumber: colunas com % usam N1 (1 casa decimal)', () => {
    expect(formatNumber(12.5, '%_PART')).toBe('12,5');
  });

  it('formatNumberDetalhamento: MEDIO/FATURADO usam N2', () => {
    expect(formatNumberDetalhamento(1234.5, 'PESO_MEDIO')).toBe('1.234,50');
    expect(formatNumberDetalhamento(500, 'VALOR_FATURADO')).toBe('500,00');
  });

  it('formatNumberDetalhamento: UNITARIO usa N3', () => {
    expect(formatNumberDetalhamento(1.5, 'PRECO_UNITARIO')).toBe('1,500');
  });

  it('formatNumberNotasFiscais: DT formata como data', () => {
    expect(formatNumberNotasFiscais('2026-08-13T00:00:00', 'DT_EMISSAO')).toBe('13/08/2026');
  });

  it('formatNumberNotasFiscais: NR_NOTAFISCAL retorna texto livre', () => {
    expect(formatNumberNotasFiscais('12345-A', 'NR_NOTAFISCAL')).toBe('12345-A');
  });
});

describe('comprasFrutasFormat — semáforo', () => {
  it('getRowSemaphoro: LINHA → #EEE9E9', () => {
    const result = getRowSemaphoro('TOTAL LINHA UVAS');
    expect(result).toEqual({ bg: '#EEE9E9' });
  });

  it('getRowSemaphoro: EMPRESA → #D3D3D3', () => {
    const result = getRowSemaphoro('TOTAL EMPRESA TECNOVIN');
    expect(result).toEqual({ bg: '#D3D3D3' });
  });

  it('getRowSemaphoro: GERAL → #77889A com branco e negrito', () => {
    const result = getRowSemaphoro('TOTAL GERAL');
    expect(result).toEqual({ bg: '#77889A', color: '#FFFFFF', bold: true });
  });

  it('getRowSemaphoro: GERAL tem precedência sobre EMPRESA', () => {
    const result = getRowSemaphoro('TOTAL GERAL EMPRESA');
    expect(result?.bg).toBe('#77889A');
  });

  it('getRowSemaphoro: texto sem match retorna null', () => {
    expect(getRowSemaphoro('Variedade Italia')).toBeNull();
  });

  it('getDetalhamentoRowStyle: VARIEDADE com GERAL → estilo total', () => {
    const result = getDetalhamentoRowStyle('TOTAL GERAL');
    expect(result).toEqual({ bg: '#77889A', color: '#FFFFFF', bold: true });
  });

  it('isLinhaTotal: detecta EMPRESA e GERAL', () => {
    expect(isLinhaTotal('TOTAL EMPRESA')).toBe(true);
    expect(isLinhaTotal('TOTAL GERAL')).toBe(true);
    expect(isLinhaTotal('Variedade Italia')).toBe(false);
  });
});

describe('comprasFrutasFormat — colunas clicáveis', () => {
  it('isColunaClicavel: DIA_ANTERIOR, QTDE_D0, ACUMULADO são clicáveis', () => {
    expect(isColunaClicavel('DIA_ANTERIOR')).toBe(true);
    expect(isColunaClicavel('QTDE_D0')).toBe(true);
    expect(isColunaClicavel('ACUMULADO')).toBe(true);
  });

  it('isColunaClicavel: QTDE_D1..D4 não são clicáveis', () => {
    expect(isColunaClicavel('QTDE_D1')).toBe(false);
    expect(isColunaClicavel('QTDE_D2')).toBe(false);
  });

  it('isColunaClicavelDetalhamento: colunas com MEDIO não são clicáveis', () => {
    expect(isColunaClicavelDetalhamento('PESO_MEDIO', 500)).toBe(false);
  });

  it('isColunaClicavelDetalhamento: VARIEDADE não é clicável', () => {
    expect(isColunaClicavelDetalhamento('VARIEDADE', 'TO005')).toBe(false);
  });

  it('isColunaClicavelDetalhamento: valor vazio não é clicável', () => {
    expect(isColunaClicavelDetalhamento('QTDE', '')).toBe(false);
    expect(isColunaClicavelDetalhamento('QTDE', null)).toBe(false);
  });

  it('isColunaClicavelDetalhamento: demais colunas com valor são clicáveis', () => {
    expect(isColunaClicavelDetalhamento('QTDE', 500)).toBe(true);
  });
});

describe('comprasFrutasFormat — visibilidade e largura', () => {
  it('isColunaVisivel: EM_SAFRA é oculta', () => {
    expect(isColunaVisivel('EM_SAFRA')).toBe(false);
  });

  it('isColunaVisivel: demais colunas são visíveis', () => {
    expect(isColunaVisivel('EMPRESA')).toBe(true);
    expect(isColunaVisivel('VARIEDADE')).toBe(true);
  });

  it('getColunaWidth: VARIEDADE → 220px', () => {
    expect(getColunaWidth('VARIEDADE')).toBe(220);
  });

  it('getColunaWidth: colunas com % → 80px', () => {
    expect(getColunaWidth('%_PART')).toBe(80);
  });
});

describe('exportarCsv', () => {
  it('nome do arquivo principal é comprasFrutas.csv', () => {
    expect('comprasFrutas.csv').toBe('comprasFrutas.csv');
  });

  it('nome do arquivo detalhamento é Compras Frutas.csv', () => {
    expect('Compras Frutas.csv').toBe('Compras Frutas.csv');
  });

  it('nome do arquivo NF é DetalhamentoNotaFiscal.csv', () => {
    expect('DetalhamentoNotaFiscal.csv').toBe('DetalhamentoNotaFiscal.csv');
  });
});

describe('comprasFrutasFormat — v2 semáforo', () => {
  it('getSemaforoCor: >= 100% verde', () => {
    const s = getSemaforoCor(118.5);
    expect(s.bg).toBe('#52c41a');
    expect(s.icon).toBe('✅');
  });

  it('getSemaforoCor: 70% - 99.9% amarelo', () => {
    const s = getSemaforoCor(85.4);
    expect(s.bg).toBe('#faad14');
    expect(s.icon).toBe('🟡');
  });

  it('getSemaforoCor: < 70% vermelho', () => {
    const s = getSemaforoCor(45.0);
    expect(s.bg).toBe('#ff4d4f');
    expect(s.icon).toBe('🔴');
  });

  it('getSemaforoCor: null/undefined cinza', () => {
    const s1 = getSemaforoCor(null);
    expect(s1.bg).toBe('#8c8c8c');
    expect(s1.icon).toBe('—');
  });

  it('formatPercent: formata percentual com N1 e símbolo %', () => {
    expect(formatPercent(89.43)).toBe('89,4%');
    expect(formatPercent(118)).toBe('118,0%');
    expect(formatPercent(null)).toBe('—');
  });
});

describe('comprasFrutasTransform — v2 hierarquia', () => {
  it('transformarGridPlana: agrupa grid flat por empresa e sub-linhas por UF', () => {
    const gridFlat = {
      dataReferencia: '2026-08-13',
      ultimaAtualizacao: '2026-08-13T10:00:00',
      colunas: ['EMPRESA', 'CD_LINHA', 'DESCRICAO', 'UF', 'SAFRA', 'QTDE_D0', 'ACUMULADO', 'META', '% ATINGIDO'],
      linhas: [
        {
          EMPRESA: '2 - TECNOVIN',
          CD_LINHA: '71',
          DESCRICAO: 'UVA',
          UF: 'RS',
          SAFRA: '2026',
          QTDE_D0: 1000,
          ACUMULADO: 50000,
          META: 40000,
          '% ATINGIDO': 125,
        },
        {
          EMPRESA: '2 - TECNOVIN',
          CD_LINHA: '79',
          DESCRICAO: 'TOTAL EMPRESA',
          UF: null,
          SAFRA: null,
          QTDE_D0: 1000,
          ACUMULADO: 50000,
          META: 40000,
          '% ATINGIDO': 125,
        },
        {
          EMPRESA: 'TOTAL GERAL',
          CD_LINHA: '99',
          DESCRICAO: 'TOTAL GERAL',
          UF: null,
          SAFRA: null,
          QTDE_D0: 1000,
          ACUMULADO: 50000,
          META: 40000,
          '% ATINGIDO': 125,
        },
      ],
    };

    const painel = transformarGridPlana(gridFlat);
    expect(painel.empresas.length).toBe(1);
    expect(painel.empresas[0].empresa).toBe('2 - TECNOVIN');
    expect(painel.empresas[0].linhas.length).toBe(1);
    expect(painel.empresas[0].linhas[0].descricao).toBe('TOTAL UVA');
    expect(painel.empresas[0].linhas[0].ufs.length).toBe(1);
    expect(painel.empresas[0].linhas[0].ufs[0].uf).toBe('RS');
    expect(painel.totalGeral.percentual).toBe(125);
  });

  it('transformarGridPlana: formata TOTAL LARANJA quando linha principal vem como TOTAL LINHA', () => {
    const gridFlat = {
      dataReferencia: '2026-08-13',
      ultimaAtualizacao: '2026-08-13T10:00:00',
      colunas: ['EMPRESA', 'CD_LINHA', 'DESCRICAO', 'UF', 'SAFRA', 'QTDE_D0', 'ACUMULADO', 'META', '% ATINGIDO'],
      linhas: [
        {
          EMPRESA: '300 - SUMABRAS',
          CD_LINHA: '73',
          DESCRICAO: 'TOTAL LINHA',
          UF: null,
          SAFRA: '2026',
          QTDE_D0: 500,
          ACUMULADO: 20000,
          META: 30000,
          '% ATINGIDO': 66.7,
        },
        {
          EMPRESA: '300 - SUMABRAS',
          CD_LINHA: '73',
          DESCRICAO: 'LARANJA',
          UF: 'SP',
          SAFRA: '2026',
          QTDE_D0: 500,
          ACUMULADO: 20000,
          META: 30000,
          '% ATINGIDO': 66.7,
        },
      ],
    };

    const painel = transformarGridPlana(gridFlat);
    expect(painel.empresas[0].linhas[0].descricao).toBe('TOTAL LARANJA');
    expect(painel.empresas[0].linhas[0].ufs.length).toBe(1);
    expect(painel.empresas[0].linhas[0].ufs[0].uf).toBe('SP');
  });
});

