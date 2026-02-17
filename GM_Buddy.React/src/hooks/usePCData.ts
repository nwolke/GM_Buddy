import { useState, useEffect, useCallback } from 'react';
import { PC } from '@/types/pc';
import { pcApi } from '@/services/api';
import { useAuth } from '@/contexts/AuthContext';

interface UsePCDataReturn {
  pcs: PC[];
  loading: boolean;
  error: string | null;
  refreshPcs: () => Promise<void>;
  savePc: (pcData: Omit<PC, 'id'> | PC) => Promise<void>;
  deletePc: (id: number) => Promise<void>;
}

export function usePCData(selectedCampaignId?: number): UsePCDataReturn {
  const { isAuthenticated } = useAuth();
  const [pcs, setPcs] = useState<PC[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadPcs = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const apiPcs = selectedCampaignId !== undefined && selectedCampaignId !== null
        ? await pcApi.getPcsByCampaign(selectedCampaignId)
        : await pcApi.getPcs();

      setPcs(apiPcs);
    } catch (err: unknown) {
      console.error('[usePCData] Failed to load PCs:', err);
      setError('Failed to load player characters.');
    } finally {
      setLoading(false);
    }
  }, [isAuthenticated, selectedCampaignId]);

  useEffect(() => {
    if (isAuthenticated) {
      loadPcs();
    } else {
      setPcs([]);
      setLoading(false);
    }
  }, [loadPcs, isAuthenticated]);

  const refreshPcs = useCallback(async () => {
    await loadPcs();
  }, [loadPcs]);

  const savePc = useCallback(async (pcData: Omit<PC, 'id'> | PC) => {
    try {
      const request = { name: pcData.name, description: pcData.description };

      if ('id' in pcData && pcData.id) {
        await pcApi.updatePc(pcData.id, request);
      } else {
        await pcApi.createPc(request);
      }

      await loadPcs();
    } catch (err) {
      console.error('[usePCData] Failed to save PC:', err);
    }
  }, [loadPcs]);

  const deletePc = useCallback(async (id: number) => {
    try {
      await pcApi.deletePc(id);
    } catch (err) {
      console.error('[usePCData] Failed to delete PC:', err);
    }

    setPcs(prev => prev.filter(pc => pc.id !== id));
  }, []);

  return {
    pcs,
    loading,
    error,
    refreshPcs,
    savePc,
    deletePc,
  };
}
