import api from '@/lib/api';
import type { MetaCompra, MetaCompraRequest, EmpresaMeta } from '../types';

export async function getMetas() {
  const { data } = await api.get<MetaCompra[]>('/agricola/metas');
  return data;
}

export async function getEmpresas() {
  const { data } = await api.get<EmpresaMeta[]>('/agricola/metas/empresas');
  return data;
}

export async function createMeta(request: MetaCompraRequest) {
  const { data } = await api.post<MetaCompra>('/agricola/metas', request);
  return data;
}

export async function updateMeta(id: number, request: MetaCompraRequest) {
  await api.put(`/agricola/metas/${id}`, request);
}

export async function deleteMeta(id: number) {
  await api.delete(`/agricola/metas/${id}`);
}
