import api from '@/lib/api';
import type { GridDinamica, DetalhamentoParams, NotasFiscaisParams } from '../types';

export async function getRecebimentoFrutas(data?: string) {
  const { data: result } = await api.get<GridDinamica>('/agricola/compras-frutas', {
    params: data ? { data } : {},
  });
  return result;
}

export async function getDetalhamento(params: DetalhamentoParams) {
  const { data } = await api.get<GridDinamica>('/agricola/compras-frutas/detalhamento', {
    params,
  });
  return data;
}

export async function getNotasFiscais(params: NotasFiscaisParams) {
  const { data } = await api.get<GridDinamica>('/agricola/compras-frutas/notas-fiscais', {
    params,
  });
  return data;
}
