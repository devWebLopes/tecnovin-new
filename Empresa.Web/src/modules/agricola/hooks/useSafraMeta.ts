import { create } from 'zustand';
import type { MetaCompra, MetaCompraRequest, EmpresaMeta } from '../types';
import * as service from '../services/safraMetaService';

interface SafraMetaState {
  metas: MetaCompra[];
  empresas: EmpresaMeta[];
  loading: boolean;
  error: string | null;
  loadMetas: () => Promise<void>;
  loadEmpresas: () => Promise<void>;
  createMeta: (request: MetaCompraRequest) => Promise<void>;
  updateMeta: (id: number, request: MetaCompraRequest) => Promise<void>;
  deleteMeta: (id: number) => Promise<void>;
}

export const useSafraMeta = create<SafraMetaState>((set, get) => ({
  metas: [],
  empresas: [],
  loading: false,
  error: null,

  loadMetas: async () => {
    set({ loading: true, error: null });
    try {
      const metas = await service.getMetas();
      set({ metas });
    } catch (err: unknown) {
      set({ error: err instanceof Error ? err.message : 'Erro ao carregar metas' });
    } finally {
      set({ loading: false });
    }
  },

  loadEmpresas: async () => {
    try {
      const empresas = await service.getEmpresas();
      set({ empresas });
    } catch {
      set({ empresas: [] });
    }
  },

  createMeta: async (request) => {
    await service.createMeta(request);
    await get().loadMetas();
  },

  updateMeta: async (id, request) => {
    await service.updateMeta(id, request);
    await get().loadMetas();
  },

  deleteMeta: async (id) => {
    await service.deleteMeta(id);
    await get().loadMetas();
  },
}));
