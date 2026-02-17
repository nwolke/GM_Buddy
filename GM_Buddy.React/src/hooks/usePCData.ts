import { useState, useEffect, useCallback } from 'react';
import { PC } from '@/types/pc';
import { pcApi, CreatePcRequest, UpdatePcRequest } from '@/services/api';
import { useAuth } from '@/contexts/AuthContext';

interface UsePCDataReturn {
  pcs: PC[];
  loading: boolean;
  error: string | null;
  refreshPcs: () => Promise<void>;
  savePC: (pcData: Omit<PC, 'id'> | PC) => Promise<void>;
  deletePC: (id: number) => Promise<void>;
}

export function usePCData(): UsePCDataReturn {
  const { isAuthenticated } = useAuth();
  const [pcs, setPCs] = useState<PC[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Load PCs from API
  const loadPcs = useCallback(async () => {
    setLoading(true);
    setError(null);

    console.log(`[usePCData] Loading PCs for authenticated user, isAuthenticated: ${isAuthenticated}`);

    const storageKey = 'gm-buddy-pcs';

    try {
      console.log(`[usePCData] Calling pcApi.getPcs()`);
      const apiPcs = await pcApi.getPcs();
      console.log(`[usePCData] API returned ${apiPcs.length} PCs:`, apiPcs);
      
      setPCs(apiPcs);

      // Only save to localStorage if we have data
      if (apiPcs.length > 0) {
        localStorage.setItem(storageKey, JSON.stringify(apiPcs));
      }
    } catch (err: unknown) {
      console.error('[usePCData] Failed to load PCs:', err);
      setError('Using cached data. Changes may not be saved.');
      
      // Fallback to localStorage
      console.log('[usePCData] Falling back to localStorage...');
      
      const storedPCs = localStorage.getItem(storageKey);
      const localPcs = storedPCs ? JSON.parse(storedPCs) : [];
      
      if (localPcs.length > 0) {
        console.log(`[usePCData] Loaded ${localPcs.length} PCs from localStorage`);
        setPCs(localPcs);
      }
    } finally {
      setLoading(false);
    }
  }, [isAuthenticated]);

  useEffect(() => {
    // Only load PCs if user is authenticated
    if (isAuthenticated) {
      loadPcs();
    } else {
      // Clear PCs when not authenticated
      console.log('[usePCData] User not authenticated, clearing PCs');
      setPCs([]);
      setLoading(false);
    }
  }, [loadPcs, isAuthenticated]);

  const refreshPcs = useCallback(async () => {
    await loadPcs();
  }, [loadPcs]);

  const savePC = useCallback(async (pcData: Omit<PC, 'id'> | PC) => {
    try {
      const request: CreatePcRequest | UpdatePcRequest = {
        name: pcData.name,
        description: pcData.description,
      };

      if ('id' in pcData && pcData.id) {
        // Update existing PC
        await pcApi.updatePc(pcData.id, request);
        console.log('Updated PC:', pcData.id);
      } else {
        // Create new PC
        console.log('Creating new PC for authenticated user');
        const createdPc = await pcApi.createPc(request);
        console.log('Created PC:', createdPc);
      }
      
      // Refresh PCs from server
      await loadPcs();
    } catch (err) {
      console.error('Failed to save PC:', err);
      // Toast is shown automatically by the axios interceptor
    }
  }, [loadPcs]);

  const deletePC = useCallback(async (id: number) => {
    try {
      await pcApi.deletePc(id);
      console.log('Deleted PC:', id);
    } catch (err) {
      console.error('Failed to delete PC from server:', err);
      // Continue with local deletion even if server fails
    }
    
    // Remove from local state
    setPCs(prev => prev.filter(pc => pc.id !== id));
  }, []);

  return {
    pcs,
    loading,
    error,
    refreshPcs,
    savePC,
    deletePC,
  };
}
