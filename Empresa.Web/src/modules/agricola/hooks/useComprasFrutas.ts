import { create } from 'zustand';
import type { GridDinamica } from '../types';
import * as service from '../services/comprasFrutasService';

type RefreshInterval = 0 | 1800000 | 3600000;

interface ComprasFrutasState {
  data: GridDinamica | null;
  loading: boolean;
  error: string | null;
  dataReferencia: string;
  refreshInterval: RefreshInterval;
  loadData: (data?: string) => Promise<void>;
  setRefreshInterval: (interval: RefreshInterval) => void;
  startAutoRefresh: () => void;
  stopAutoRefresh: () => void;
}

let intervalId: ReturnType<typeof setInterval> | null = null;

export const useComprasFrutas = create<ComprasFrutasState>((set, get) => ({
  data: null,
  loading: false,
  error: null,
  dataReferencia: new Date().toISOString().split('T')[0],
  refreshInterval: 0,

  loadData: async (data?: string) => {
    set({ loading: true, error: null });
    try {
      const result = await service.getRecebimentoFrutas(data);
      set({ data: result, dataReferencia: result.dataReferencia });
    } catch (err: unknown) {
      set({ error: err instanceof Error ? err.message : 'Erro ao carregar dados' });
    } finally {
      set({ loading: false });
    }
  },

  setRefreshInterval: (interval) => {
    set({ refreshInterval: interval });
    get().stopAutoRefresh();
    if (interval > 0) {
      get().startAutoRefresh();
    }
  },

  startAutoRefresh: () => {
    get().stopAutoRefresh();
    const interval = get().refreshInterval;
    if (interval > 0) {
      intervalId = setInterval(() => {
        get().loadData(get().dataReferencia);
      }, interval);
    }
  },

  stopAutoRefresh: () => {
    if (intervalId) {
      clearInterval(intervalId);
      intervalId = null;
    }
  },
}));
