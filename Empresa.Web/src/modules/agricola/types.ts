export interface MetaCompra {
  idMetaCompra: number;
  cdLinha: number;
  safra: string;
  dtInicial: string;
  dtFinal: string;
  metaQtde: number;
  cdEmpresa: number;
}

export interface MetaCompraRequest {
  cdLinha: number;
  safra: string;
  dtInicial: string | null;
  dtFinal: string | null;
  metaQtde: number | null;
  cdEmpresa: number | null;
}

export interface EmpresaMeta {
  cdEmpresa: number;
  nome: string;
}

export interface GridDinamica {
  dataReferencia: string;
  ultimaAtualizacao: string;
  colunas: string[];
  linhas: Record<string, unknown>[];
}

export type ColunaClicada = 'ANTERIOR' | 'DIA_ANTERIOR' | 'QTDE_D4' | 'QTDE_D3' | 'QTDE_D2' | 'QTDE_D1' | 'QTDE_D0' | 'ACUMULADO';

export interface DetalhamentoParams {
  data: string;
  empresa: string;
  linha: string;
  uf: string;
  colunaClicada: ColunaClicada;
}

export interface NotasFiscaisParams {
  data: string;
  empresa: string;
  linha: string;
  uf: string;
  colunaClicada: ColunaClicada;
  colunaGrauClicada?: string;
  variedade?: string;
}

export interface ValoresDiarios {
  qtde_d4: number | null;
  qtde_d3: number | null;
  qtde_d2: number | null;
  qtde_d1: number | null;
  qtde_d0: number | null;
  acumulado: number | null;
  meta: number | null;
  percentual: number | null;
}

export interface LinhaUf {
  uf: string;
  safra: string;
  valores: ValoresDiarios;
  rawLinha: Record<string, unknown>;
}

export interface LinhaEmpresa {
  cdLinha: string;
  descricao: string;
  safra: string;
  totais: ValoresDiarios;
  ufs: LinhaUf[];
  rawLinha: Record<string, unknown>;
}

export interface EmpresaData {
  empresa: string;
  cdEmpresa: string;
  linhas: LinhaEmpresa[];
  total: ValoresDiarios;
  rawLinha: Record<string, unknown>;
}

export interface PainelData {
  empresas: EmpresaData[];
  totalGeral: ValoresDiarios & { rawLinha?: Record<string, unknown> };
  colunasDatas: string[];
  dataReferencia: string;
  ultimaAtualizacao: string;
}

